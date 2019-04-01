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

using Bio.Data.Providers.Interfaces;
using Bio.Data.Providers.Structure;
using Bio.Data.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using Bio.Views.ViewModels;

namespace Bio.Views.Structure.ViewModels
{
    /// <summary>
    /// This is the view model for the secondary structure diagram
    /// </summary>
    class SSViewModel : BioViewModel<SecondaryStructureBrowser>
    {
        public ObservableCollection<SSElementBaseViewModel> ModelElements { get; private set; }

        private double _maxWidth;
        public double MaxWidth
        {
            get
            {
                return _maxWidth;
            }
            private set
            {
                _maxWidth = value;
                OnPropertyChanged("MaxWidth");
            }
        }

        private double _maxHeight;
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            private set
            {
                _maxHeight = value;
                OnPropertyChanged("MaxHeight");
            }
        }

        public override string Title
        {
            get { return string.Format("2D Structure View: {0}", base.Title); }
            set { base.Title = value; }
        }

        public override bool Initialize(IBioDataLoader data)
        {
            Dictionary<int, SSBasePairViewModel> basePairVMs = new Dictionary<int, SSBasePairViewModel>();

            var sdloader = data as IBioDataLoader<IStructureModelBioEntity>;
            if (sdloader != null)
            {
                var basePairs = from bp in sdloader.Entities.OfType<XRNABasePair>()
                                select bp;

                foreach (XRNABasePair bp in basePairs)
                {
                    SSBasePairViewModel bpvm = BuildBasePairViewModel(bp);
                    ModelElements.Add(bpvm);
                    ModelElements.Add(bpvm.FivePrimeNucleotide);
                    ModelElements.Add(bpvm.ThreePrimeNucleotide);
                    basePairVMs.Add(bpvm.FivePrimeNucleotide.Index, bpvm);
                }

                var unpairedNt = from nt in sdloader.Entities.OfType<XRNANucleotide>()
                                 select nt;
                foreach (XRNANucleotide nt in unpairedNt)
                {
                    SSSymbolViewModel svm = BuildSymbolViewModel(nt);
                    ModelElements.Add(svm);
                }

                var allEntities = from entity in sdloader.Entities
                                  select entity;

                foreach (IStructureModelBioEntity entity in allEntities)
                {
                    if (entity is XRNABasePairConnector)
                    {
                        XRNABasePairConnector lc = (XRNABasePairConnector)entity;
                        ModelElements.Add(new SSBasePairLineConnectorViewModel(basePairVMs[lc.BasePair.FivePrimeIndex + 1])
                                            {
                                                Color = lc.Color,
                                                Thickness = lc.Thickness,
                                                Visible = lc.Visible
                                            });
                    }
                    else if (entity is XRNABasePairConnectorCircle)
                    {
                        XRNABasePairConnectorCircle cc = (XRNABasePairConnectorCircle)entity;
                        ModelElements.Add(new SSBasePairCircleConnectorViewModel(basePairVMs[cc.BasePair.FivePrimeIndex + 1])
                                            {
                                                Color = cc.Color,
                                                Radius = cc.Radius,
                                                Filled = cc.Filled,
                                                Thickness = cc.Thickness
                                            });
                    }
                    else if (entity is TextLabel)
                    {
                        TextLabel lc = (TextLabel)entity;
                        ModelElements.Add(new SSTextLabelViewModel(lc.Start.X, lc.Start.Y, lc.Text, new FontFamily(lc.FontFace), (FontStyle)_fscvtr.ConvertFrom(lc.FontStyle),
                                       (FontWeight)_fwcvtr.ConvertFrom(lc.FontWeight), lc.FontSize, (Brush)_colorCvtr.ConvertFrom(lc.Color)));
                    }
                    else if (entity is LineLabel)
                    {
                        LineLabel ll = (LineLabel)entity;
                        ModelElements.Add(new SSLineLabelViewModel()
                                            {
                                                X = ll.Start.X,
                                                Y = ll.Start.Y,
                                                X1 = ll.End.X - ll.Start.X,
                                                Y1 = ll.End.Y - ll.Start.Y,
                                                Color = (Brush)_colorCvtr.ConvertFrom(ll.Color),
                                                Thickness = ll.LineWeight
                                            });
                    }
                    else if (entity is ParallelogramLabel)
                    {
                        ParallelogramLabel lc = (ParallelogramLabel)entity;
                        SSParallelogramLabelViewModel vm = new SSParallelogramLabelViewModel()
                                                            {
                                                                CenterX = lc.Center.X,
                                                                CenterY = lc.Center.Y,
                                                                Side1Length = lc.Side1Length,
                                                                RotationAngle = lc.Side1Angle,
                                                                Side2Length = lc.Side2Length,
                                                                ParallelogramAngle = lc.Side2Angle,
                                                                Color = (Brush)_colorCvtr.ConvertFrom(lc.Color),
                                                                LineWeight = lc.LineWeight
                                                            };
                        vm.ComputeParallelogram();
                        ModelElements.Add(vm);
                    }
                    else if (entity is ArcLabel)
                    {
                        ArcLabel ac = (ArcLabel)entity;
                        SSArcLabelViewModel vm = new SSArcLabelViewModel()
                                                    {
                                                        CenterX = ac.Center.X,
                                                        CenterY = ac.Center.Y,
                                                        Color = (Brush)_colorCvtr.ConvertFrom(ac.Color),
                                                        LineWeight = ac.LineWeight,
                                                        Radius = ac.Radius,
                                                        Angle1 = ac.Angle1,
                                                        Angle2 = ac.Angle2,
                                                    };
                        vm.ComputeArc();
                        ModelElements.Add(vm);
                    }
                    else if (entity is ArrowLabel)
                    {
                        ArrowLabel al = (ArrowLabel)entity;
                        SSArrowLabelViewModel vm = new SSArrowLabelViewModel()
                        {
                            ArrowTipX = al.ArrowTip.X,
                            ArrowTipY = al.ArrowTip.Y,
                            LeftTipX = al.LeftTip.X,
                            LeftTipY = al.LeftTip.Y,
                            Color = (Brush)_colorCvtr.ConvertFrom(al.Color),
                            LineWeight = al.LineWeight,
                            TailLength = al.TailLength,
                            RotationAngle = al.Angle
                        };
                        vm.ComputeArrow();
                        ModelElements.Add(vm);
                    }
                }

                MeasureAndArrangeCanvas();

                return base.Initialize(data);
            }
            return false;
        }

        public SSViewModel()
        {
            ModelElements = new ObservableCollection<SSElementBaseViewModel>();
            _colorCvtr = new BrushConverter();
            _fscvtr = new FontStyleConverter();
            _fwcvtr = new FontWeightConverter();
        }

        private double _minLeft;
        private double _minTop;
        private BrushConverter _colorCvtr;
        private FontStyleConverter _fscvtr;
        private FontWeightConverter _fwcvtr;

        private SSBasePairViewModel BuildBasePairViewModel(XRNABasePair bp)
        {
            SSSymbolViewModel fivePrimeNtVM = BuildSymbolViewModel(bp.FivePrimeNucleotide);
            SSSymbolViewModel threePrimeNtVM = BuildSymbolViewModel(bp.ThreePrimeNucleotide);
            return new SSBasePairViewModel(fivePrimeNtVM, threePrimeNtVM);
        }

        private SSSymbolViewModel BuildSymbolViewModel(XRNANucleotide symbol)
        {
            SSSymbolViewModel retValue = new SSSymbolViewModel(symbol.Sequence.RawData[symbol.Index], symbol.Index + 1,
                                    new FontFamily(symbol.FontFace), (FontStyle)_fscvtr.ConvertFrom(symbol.FontStyle), (FontWeight)_fwcvtr.ConvertFrom(symbol.FontWeight), (Brush)_colorCvtr.ConvertFrom(symbol.Color),
                                    symbol.FontSize, symbol.Center.X, symbol.Center.Y);
            retValue.Visible = !symbol.Hidden;
            return retValue;
        }

        private void MeasureAndArrangeCanvas()
        {
            _minLeft = ModelElements.Min(p => p.X);
            _minTop = ModelElements.Min(p => p.Y);

            double shiftx = (_minLeft < 0) ? -1 * _minLeft : 0;
            double shifty = (_minTop < 0) ? -1 * _minTop : 0;

            foreach (SSElementBaseViewModel elem in ModelElements)
            {
                elem.ShiftX(shiftx);
                elem.ShiftY(shifty);

                if (elem.X + elem.Width > MaxWidth) MaxWidth = elem.X + elem.Width;
                if (elem.Y + elem.Height > MaxHeight) MaxHeight = elem.Y + elem.Height;
            }

            //We add some fudge factor
            MaxHeight = 1.10 * MaxHeight;
            MaxWidth = 1.10 * MaxWidth;
        }
    }
}
