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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Interfaces;
using Bio.Views.ViewModels;

namespace Bio.Views.Structure.ViewModels
{
    /// <summary>
    /// This is the nested structure view model
    /// </summary>
    class NestedViewModel : BioViewModel<NestedStructureBrowser>
    {
        public ObservableCollection<NestedElementBaseViewModel> ModelElements { get; private set; }

        private double _width;
        public double Width
        {
            get { return _width; }
            private set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            private set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }
      
        private double _elementSpacing;
        public double ElementSpacing
        {
            get { return _elementSpacing; }
            private set
            {
                _elementSpacing = value;
            }
        }

        private double _startY;
        public double StartY
        {
            get { return _startY; }
            private set
            {
                _startY = value;
            }
        }

        private double _startX;
        public double StartX
        {
            get { return _startX; }
            private set
            {
                _startX = value;
            }
        }

        public override string Title
        {
            get { return string.Format("Nested Structure View: {0}", base.Title); }
            set { base.Title = value; }
        }

        public override bool Initialize(IBioDataLoader data)
        {
            var sdloader = data as IBioDataLoader<IStructureModelBioEntity>;
            if (sdloader != null)
            {
                var basePairs = from bp in sdloader.Entities.OfType<IBasePairEntity>()
                                select bp;
                _sequence = basePairs.First(bp => bp.Sequence != null).Sequence;
                //ElementSpacing = (Width - (2 * _xPadding)) / _sequence.RawData.Count;
                Width = (2 * _xPadding) + ElementSpacing * _sequence.RawData.Count;

                for (int i = 0; i < _sequence.RawData.Count; i++)
                {
                    NestedElementViewModel nevm = new NestedElementViewModel(this, _sequence.RawData[i], i);
                    _nestedVMs.Add(nevm);
                }

                _maxBPDistance = basePairs.Max(e => e.ThreePrimeIndex - e.FivePrimeIndex - 1);
                _minBPDistance = basePairs.Min(e => e.ThreePrimeIndex - e.FivePrimeIndex - 1);

                var basepairVMs = from bp in basePairs
                                  join fpVM in _nestedVMs on bp.FivePrimeIndex equals fpVM.Index
                                  join tpVM in _nestedVMs on bp.ThreePrimeIndex equals tpVM.Index
                                  select new NestedBasePairViewModel(this, fpVM, tpVM);

                foreach (NestedBasePairViewModel bp in basepairVMs)
                {
                    ModelElements.Add(bp);
                }

                foreach (NestedElementViewModel elem in _nestedVMs)
                {
                    ModelElements.Add(elem);
                }

                var tickElems = from elem in _nestedVMs
                                where elem.Index == 1 || elem.Index % 10 == 0
                                select elem; 
                foreach (NestedElementViewModel elem in tickElems)
                {
                    ModelElements.Add(new NestedElementTickViewModel(this, elem));
                }
            }
            return base.Initialize(data);
        }

        public NestedViewModel()
        {
            ModelElements = new ObservableCollection<NestedElementBaseViewModel>();
            _nestedVMs = new List<NestedElementViewModel>();
            _xPadding = 100;
            ElementSpacing = 3;
            //Width = 600;
            Height = 600;
            StartX = _xPadding;
            StartY = 500;
            _maxBPHeight = 490;
            _minBPHeight = 10;
        }

        public double ComputeBasePairHeight(int distance)
        {
            return _minBPHeight + (distance - _minBPDistance)*((_maxBPHeight - _minBPHeight)/(_maxBPDistance-_minBPDistance));
        }

        private IBioEntity _sequence;
        private List<NestedElementViewModel> _nestedVMs;
        private double _minBPHeight;
        private double _maxBPHeight;
        private int _minBPDistance;
        private int _maxBPDistance;
        private double _xPadding;
    }
}
