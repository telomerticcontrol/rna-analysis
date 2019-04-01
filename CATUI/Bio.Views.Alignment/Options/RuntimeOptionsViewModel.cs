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
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Controls;
using JulMar.Windows.Mvvm;
using System.Windows.Media;

namespace Bio.Views.Alignment.Options
{
    /// <summary>
    /// Basic options view model used at runtime
    /// </summary>
    public class RuntimeOptionsViewModel : SimpleViewModel
    {
        private readonly List<Brush> _refSeqBrushes = new List<Brush>();
        private Brush _selectionBrush, _backgroundBrush, _selectionTextBrush, _separatorBrush;
        private Brush _selectionBorderBrush, _focusedBrush;
        private Brush _focusedBorderBrush, _foregroundBrush, _lineNumberTextBrush;
        private Brush _focusRectBorderBrush, _focusRectBrush, _groupHeaderBrush, _groupHeaderTextBrush;
        private readonly Dictionary<char, Brush> _nucleotideColors = new Dictionary<char,Brush>();

        public RuntimeOptionsViewModel()
        {
            LoadNucleotideColors();
        }

        /// <summary>
        /// Usable font size (DPI)
        /// </summary>
        public double SelectedFontSizeDpi
        {
            get { return (double)Properties.Settings.Default.SelectedFontSize * 4 / 3; }
        }

        /// <summary>
        /// Alignment editor font size
        /// </summary>
        public double AlignmentFontSizeDpi
        {
            get { return (double) Properties.Settings.Default.AlignmentFontSize*4/3;  }    
        }

        /// <summary>
        /// Selected font
        /// </summary>
        public string SelectedFont
        {
            get { return Properties.Settings.Default.SelectedFont; }
        }

        /// <summary>
        /// Alignment editor font (always fixed width)
        /// </summary>
        public string AlignmentFontFamily
        {
            get { return Properties.Settings.Default.AlignmentFontFamily; }
        }

        /// <summary>
        /// Minimum grouping range
        /// </summary>
        public int MinGroupingRange
        {
            get { return Properties.Settings.Default.MinGroupingRange; }
        }

        /// <summary>
        /// True/False to show row numbers
        /// </summary>
        public bool ShowRowNumbers
        {
            get { return Properties.Settings.Default.ShowRowNumbers; }
        }

        /// <summary>
        /// True/False whether to sort by Taxononmy in non-grouped list
        /// </summary>
        public bool SortByTaxonomy
        {
            get { return Properties.Settings.Default.SortByTaxonomy; }
        }

        /// <summary>
        /// True if we always open with grouping
        /// </summary>
        public bool OpenWithGrouping
        {
            get { return Properties.Settings.Default.OpenWithGrouping; }
        }

        /// <summary>
        /// Whether to show taxononmy group levels.
        /// </summary>
        public bool ShowTaxonomyGroupLevel
        {
            get { return Properties.Settings.Default.ShowTaxonomyGroupLevel;  }    
        }

        /// <summary>
        /// Brush used in the UI (from SelectionColor)
        /// </summary>
        public Brush SelectionBrush
        {
            get
            {
                if (_selectionBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.SelectionColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _selectionBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.SelectionColor);
                    _selectionBrush.Freeze();
                }
                return _selectionBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from SelectionTextColor)
        /// </summary>
        public Brush SelectionTextBrush
        {
            get
            {
                if (_selectionTextBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.SelectionTextColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _selectionTextBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.SelectionTextColor);
                    _selectionTextBrush.Freeze();
                }
                return _selectionTextBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from SeparatorColor)
        /// </summary>
        public Brush SeparatorBrush
        {
            get
            {
                if (_separatorBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.SeparatorColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _separatorBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.SeparatorColor);
                    _separatorBrush.Freeze();
                }
                return _separatorBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from BackgroundColor)
        /// </summary>
        public Brush BackgroundBrush
        {
            get
            {
                if (_backgroundBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.BackgroundColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _backgroundBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.BackgroundColor);
                    _backgroundBrush.Freeze();
                }
                return _backgroundBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from SelectionBorderColor)
        /// </summary>
        public Brush SelectionBorderBrush
        {
            get
            {
                if (_selectionBorderBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.SelectionBorderColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _selectionBorderBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.SelectionBorderColor);
                    _selectionBorderBrush.Freeze();
                }
                return _selectionBorderBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from FocusedColor)
        /// </summary>
        public Brush FocusedBrush
        {
            get
            {
                if (_focusedBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.FocusedColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _focusedBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.FocusedColor);
                    _focusedBrush.Freeze();
                }
                return _focusedBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from FocusedBorderColor)
        /// </summary>
        public Brush FocusedBorderBrush
        {
            get
            {
                if (_focusedBorderBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.FocusedBorderColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _focusedBorderBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.FocusedBorderColor);
                    _focusedBorderBrush.Freeze();
                }
                return _focusedBorderBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from ForegroundTextColor)
        /// </summary>
        public Brush ForegroundTextBrush
        {
            get
            {
                if (_foregroundBrush == null)
                {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.ForegroundTextColor))
                    {
                        BrushConverter bc = new BrushConverter();
                        _foregroundBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.ForegroundTextColor);
                        _foregroundBrush.Freeze();
                    }
                    else _foregroundBrush = Brushes.Black;
                }
                return _foregroundBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from LineNumberTextColor)
        /// </summary>
        public Brush LineNumberTextBrush
        {
            get
            {
                if (_lineNumberTextBrush == null)
                {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.LineNumberTextColor))
                    {
                        BrushConverter bc = new BrushConverter();
                        _lineNumberTextBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.LineNumberTextColor);
                        _lineNumberTextBrush.Freeze();
                    }
                    else _lineNumberTextBrush = Brushes.Black;
                }
                return _lineNumberTextBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from FocusRectColor)
        /// </summary>
        public Brush FocusRectBrush
        {
            get
            {
                if (_focusRectBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.FocusRectColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _focusRectBrush = (Brush) bc.ConvertFrom(Properties.Settings.Default.FocusRectColor);
                    _focusRectBrush.Freeze();
                }
                return _focusRectBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI (from FocusRectColor)
        /// </summary>
        public Brush FocusRectBorderBrush
        {
            get
            {
                if (_focusRectBorderBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.FocusRectBorderColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _focusRectBorderBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.FocusRectBorderColor);
                    _focusRectBorderBrush.Freeze();
                }
                return _focusRectBorderBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI 
        /// </summary>
        public Brush GroupHeaderBrush
        {
            get
            {
                if (_groupHeaderBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.GroupHeaderBackgroundColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _groupHeaderBrush = (Brush) bc.ConvertFrom(Properties.Settings.Default.GroupHeaderBackgroundColor);
                    _groupHeaderBrush.Freeze();
                }
                return _groupHeaderBrush;
            }
        }

        /// <summary>
        /// Brush used in the UI 
        /// </summary>
        public Brush GroupHeaderTextBrush
        {
            get
            {
                if (_groupHeaderTextBrush == null && !string.IsNullOrEmpty(Properties.Settings.Default.GroupHeaderTextColor))
                {
                    BrushConverter bc = new BrushConverter();
                    _groupHeaderTextBrush = (Brush)bc.ConvertFrom(Properties.Settings.Default.GroupHeaderTextColor);
                    _groupHeaderTextBrush.Freeze();
                }
                return _groupHeaderTextBrush;
            }
        }

        /// <summary>
        /// Method to retreive the next refernece sequence color brush
        /// </summary>
        /// <returns></returns>
        public Brush GetNextReferenceSequenceBrush(int count)
        {
            if (_refSeqBrushes.Count==0)
            {
                BrushConverter bc = new BrushConverter();
                foreach (Brush br in Properties.Settings.Default.ReferenceSequenceColors.Cast<string>().Select(color => (Brush)bc.ConvertFrom(color)))
                {
                    br.Freeze();
                    _refSeqBrushes.Add(br);
                }
            }

            return _refSeqBrushes[(count % _refSeqBrushes.Count)];
        }

        /// <summary>
        /// Returns the proper color for a given symbol
        /// </summary>
        /// <param name="bioSymbol">BioSymbol</param>
        /// <returns>Brush</returns>
        public Brush GetNucleotideColor(IBioSymbol bioSymbol)
        {
            Brush brush;
            return _nucleotideColors.TryGetValue(bioSymbol.Value, out brush)
                       ? brush
                       : (_nucleotideColors.TryGetValue(Char.ToUpper(bioSymbol.Value), out brush)
                              ? brush
                              : Brushes.Black);
        }

        /// <summary>
        /// Sets the nucelotide attributes from the selected options
        /// </summary>
        /// <param name="bioSymbol"></param>
        /// <param name="ta"></param>
        public void GetAttributes(IBioSymbol bioSymbol, ref TextAttributes ta)
        {
            ta.Foreground = GetNucleotideColor(bioSymbol);
        }

        /// <summary>
        /// This marks the change in a set of properties.
        /// </summary>
        public void BroadcastOptionChange()
        {
            _refSeqBrushes.Clear();
            _selectionBrush = _backgroundBrush = _selectionTextBrush = _separatorBrush = null;
            _selectionBorderBrush = _focusedBrush = null;
            _focusedBorderBrush = _foregroundBrush = _lineNumberTextBrush = null;
            _focusRectBorderBrush = _focusRectBrush = _groupHeaderBrush = _groupHeaderTextBrush = null;

            LoadNucleotideColors();
            
            // All properties have changed.
            OnPropertyChanged();
        }

        /// <summary>
        /// Loads the nucleotide colors dictionary
        /// </summary>
        private void LoadNucleotideColors()
        {
            _nucleotideColors.Clear();

            BrushConverter bc = new BrushConverter();
            foreach (var snc in Properties.Settings.Default.NucleotideColors)
            {
                char ch = snc[0];
                string color = snc.Substring(1);
                Brush bColor;
                try
                {
                    bColor = (Brush) bc.ConvertFrom(color);
                    bColor.Freeze();
                }
                catch
                {
                    bColor = Brushes.Black;
                }
                _nucleotideColors.Add(ch, bColor);
            }
        }


    }
}
