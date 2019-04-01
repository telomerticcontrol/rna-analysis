/* 
* Copyright (c) 2009, The University of Texas at Austin
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without modification, 
* are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, 
* this list of conditions and the following disclaimer.
*
* 2. Redistributions in binary form must reproduce the above copyright notice, 
* this list of conditions and the following disclaimer in the documentation and/or other materials 
* provided with the distribution.
*
* Neither the name of The University of Texas at Austin nor the names of its contributors may be 
* used to endorse or promote products derived from this software without specific prior written 
* permission.
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
* BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF 
* THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Views;
using Bio.Views.ViewModels;
using JulMar.Windows;

namespace Bio.Views.Alignment.ViewModels
{
    /// <summary>
    /// The ViewModel for the Birds Eye Viewer (BEV)
    /// </summary>
    public class BirdsEyeViewModel : SidebarViewModel<BirdsEyeViewer>
    {
        #region Private Data

        private readonly int _whiteBlock = ColorToInt(Colors.White);
        private int _stride;
        private int _maxColumn;
        private int _visibleColumns, _visibleRows, _firstColumn, _topRow;
        private readonly AlignmentViewModel _parent;
        private readonly PropertyObserver<AlignmentViewModel> _dataChanged;
        private Thread _thread;
        private bool _isRendering;
        private volatile bool _stopFlag;
        private InteropBitmap _bitmap;
        private IntPtr _section;
        private unsafe int* _view;
        #endregion

        /// <summary>
        /// Entities being displayed in BEV
        /// </summary>
        public IList<IAlignedBioEntity> Entities { get; set; }

        /// <summary>
        /// True/False whether the BEV is being rendered currently.
        /// </summary>
        public bool IsRendering
        {
            get { return _isRendering;  }
            set 
            { 
                _isRendering = value;
                OnPropertyChanged("IsRendering"); 
            }
        }

        /// <summary>
        /// Total columns in the main view
        /// </summary>
        public int TotalColumns { get { return _maxColumn; } }

        /// <summary>
        /// Total rows in the main view
        /// </summary>
        public int TotalRows { get { return (Entities != null) ? Entities.Count : 0; } }

        /// <summary>
        /// The image created from the entities - this is scaled and displayed in the
        /// view.
        /// </summary>
        public ImageSource BevImage
        {
            get
            {
                _bitmap.Invalidate();
                return (BitmapSource) _bitmap.GetAsFrozen();
            }
        }

        /// <summary>
        /// Visible columns in the display - this determines the selection rectangle
        /// </summary>
        public int VisibleColumns
        {
            get { return _visibleColumns; }
            set 
            {
                int newValue = value;
                if (newValue > TotalColumns)
                    newValue = TotalColumns;
                _visibleColumns = newValue;
                OnPropertyChanged("VisibleColumns"); 
            }
        }

        /// <summary>
        /// Visible rows in the display - this determines the selection rectangle
        /// </summary>
        public int VisibleRows
        {
            get { return _visibleRows; }
            set 
            {
                int newValue = value;
                if (newValue > TotalRows)
                    newValue = TotalRows;
                _visibleRows = newValue;
                OnPropertyChanged("VisibleRows"); 
            }
        }

        /// <summary>
        /// First column in the main display - determines the position of the selection rectangle.
        /// </summary>
        public int FirstColumn
        {
            get { return _firstColumn; }
            set 
            {
                _firstColumn = value;
                OnPropertyChanged("FirstColumn", "PositionText"); 
            }
        }

        /// <summary>
        /// Top row in the main display - determines the positon of the selection rectangle.
        /// </summary>
        public int TopRow
        {
            get { return _topRow; }
            set 
            { 
                _topRow = value;
                OnPropertyChanged("TopRow", "PositionText"); 
            }
        }

        /// <summary>
        /// Current position text
        /// </summary>
        public string PositionText
        {
            get { return string.Format("({0},{1})", FirstColumn + 1, TopRow + 1); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Alignment view parent</param>
        public BirdsEyeViewModel(AlignmentViewModel parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            Title = "Birds Eye Viewer";
            ImageUrl = "/Bio.Views.Alignment;component/images/bev_icon.png";
            _parent = parent;
            _dataChanged = new PropertyObserver<AlignmentViewModel>(_parent);
            _dataChanged.RegisterHandler(v => v.TotalRows, v => StartBevGeneration());
            Entities = _parent.GroupedEntities;
            StartBevGeneration();
        }

        /// <summary>
        /// This kicks off the rendering thread.
        /// </summary>
        private void StartBevGeneration()
        {
            // If we are already rendering, then stop.
            StopBevRendering();

            _maxColumn = Entities.Max(e => (e.AlignedData != null) ? e.AlignedData.Count : 0);

            uint numPixels = (uint)(_maxColumn * Entities.Count);
            _section = CreateFileMapping(INVALID_HANDLE_VALUE, IntPtr.Zero, PAGE_READWRITE, 0, numPixels * 4, null);
            unsafe
            {
                _view = (int*)MapViewOfFile(_section, FILE_MAP_ALL_ACCESS, 0, 0, numPixels * 4).ToPointer();
                Parallel.For(0, numPixels, i => _view[i] = _whiteBlock);
            }

            _stride = (_maxColumn * PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
            _bitmap = Imaging.CreateBitmapSourceFromMemorySection(_section, _maxColumn, Entities.Count, PixelFormats.Bgr32, _stride, 0) as InteropBitmap;

            OnPropertyChanged("BevImage");

            // No entities?  We're done.
            if (Entities == null)
                return;

            _stopFlag = false;
            _thread = new Thread(GenerateBevImage) { Name = "Bev Generator", IsBackground = false, Priority = ThreadPriority.BelowNormal };
            _thread.Start();
        }

        /// <summary>
        /// This stops the thread doing the rendering
        /// </summary>
        private void StopBevRendering()
        {
            if (!IsRendering)
                return;

            Thread localThread = _thread;
            if (localThread != null)
            {
                _stopFlag = true;
                if (!localThread.Join(100))
                {
                    localThread.Interrupt();
                    localThread.Join();
                }
                _thread = null;
            }

            if (_section != IntPtr.Zero)
            {
                CloseHandle(_section);
            }
        }

        /// <summary>
        /// This is called by the view to reposition the box when a mouse event occurs.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RepositionBox(int x, int y)
        {
            int xPos = x - VisibleColumns / 2;
            int yPos = y - VisibleRows / 2;

            if (xPos < 0) xPos = 0;
            if (yPos < 0) yPos = 0;
            if (xPos + VisibleColumns > TotalColumns)
                xPos = TotalColumns - VisibleColumns;
            if (yPos + VisibleRows > TotalRows)
                yPos = TotalRows - VisibleRows;

            FirstColumn = xPos;
            TopRow = yPos;
        }

        /// <summary>
        /// This is called by the view when the mouse stops moving to update the current position.
        /// </summary>
        public void UpdatePosition()
        {
            if (_parent != null)
            {
                _parent.ChangeCurrentPosition(_firstColumn, _topRow);
            }
        }

        /// <summary>
        /// This method generates the BEV image.
        /// </summary>
        private unsafe void GenerateBevImage()
        {
            try
            {
                IsRendering = true;

                var localEntities = Entities;
                var backBuffer = _view;
                var maxColumn = _maxColumn;

                int updateThreshold = Math.Max(100, localEntities.Count / 10), count = localEntities.Count;
                Parallel.For(0, count, (y, ps) =>
                   {
                       if (_stopFlag)
                           ps.Stop();
                       else
                       {
                           var entity = localEntities[y];
                           int row = y * maxColumn, dataCount = (entity.AlignedData != null) ? entity.AlignedData.Count : maxColumn;
                           if (entity.AlignedData == null)
                           {
                               // Group header or separator of some type.
                               for (int x = 0; x < dataCount; x++)
                                   backBuffer[row + x] = 0; // black
                           }
                           else
                           {
                               for (int x = 0; x < dataCount; x++)
                               {
                                   if (_stopFlag)
                                   {
                                       ps.Stop();
                                       break;
                                   }

                                   backBuffer[row + x] = GetPixelColor(entity.AlignedData[x]);
                               }
                           }
                           if ((y % updateThreshold) == 0)
                               OnPropertyChanged("BevImage");
                       }
                   });
                OnPropertyChanged("BevImage");
            }
            catch (ThreadInterruptedException)
            {
            }
            finally
            {
                IsRendering = false;
            }
        }

        /// <summary>
        /// Dispse the unmanaged resources
        /// </summary>
        /// <param name="isDisposing"></param>
        protected override void Dispose(bool isDisposing)
        {
            StopBevRendering();
            base.Dispose(isDisposing);
        }

        /// <summary>
        /// Determines the proper pixel color for a given BioSymbol.
        /// </summary>
        /// <param name="bioSymbol"></param>
        /// <returns></returns>
        private int GetPixelColor(IBioSymbol bioSymbol)
        {
            // None translates to whitespace.
            if (bioSymbol.Type == Data.BioSymbolType.None)
                return _whiteBlock;

            if (_parent != null && _parent.NucleotideColorSelector != null)
                return ColorToInt(_parent.NucleotideColorSelector.GetSymbolColor(bioSymbol));

            // Fallback for testing.
            switch (bioSymbol.Value)
            {
                case 'A':
                    return ColorToInt(Colors.Red);
                case 'G':
                    return ColorToInt(Colors.Green);
                case 'U':
                    return ColorToInt(Colors.Blue);
                case 'I':
                    return ColorToInt(Colors.RosyBrown);
                default:
                    return _whiteBlock;
            }
        }

        /// <summary>
        /// Simple function to create an RGB 32-bit value from a Color.
        /// </summary>
        /// <param name="clr"></param>
        /// <returns></returns>
        private static int ColorToInt(Color clr)
        {
            return (clr.R << 16) | (clr.G << 8) | clr.B;
        }

        #region Unmanaged Interop Code
        // Functions used from Kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
        [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
        [DllImport("kernel32.dll", SetLastError = true)] static extern void CloseHandle(IntPtr hFile);

        // Constants from Windows.h
        const uint FILE_MAP_ALL_ACCESS = 0xF001F;
        const uint PAGE_READWRITE = 0x04;
        readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        #endregion
    }
}
