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
using System.Linq;
using System.Windows.Input;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Interfaces;
using JulMar.Windows;
using JulMar.Windows.Mvvm;

namespace Bio.Views.Structure.ViewModels
{
    class CircleViewModel : StructureViewModelBase<CircleStructureBrowser>
    {
        public MTObservableCollection<CircleElementBaseViewModel> ModelElements { get; private set; }

        public ICommand LoadPhyloDDataCommand { get; private set; }
        public ICommand ClosePhyloDDataCommand { get; private set; }

        public double _width;
        public double Width
        {
            get { return _width; }
            private set { _width = value; }
        }

        public double _height;
        public double Height
        {
            get { return _height; }
            private set { _height = value; }
        }

        public string _statusText;
        public string Status
        {
            get { return _statusText; }
            private set { _statusText = value; OnPropertyChanged("Status"); }
        }

        private PhyloDCircleViewModel _phyloDVM;
        public PhyloDCircleViewModel PhyloDVM
        {
            get { return _phyloDVM; }
            private set { _phyloDVM = value; }
        }

        public override string Title
        {
            get { return string.Format("Circle Structure View: {0}", base.Title); }
            set { base.Title = value; }
        }

        public override bool Initialize(IBioDataLoader data)
        {
            var sdloader = data as IBioDataLoader<IStructureModelBioEntity>;
            if (sdloader != null)
            {
                var basePairs = from bp in sdloader.Entities.OfType<IBasePairEntity>()
                                select bp;
                IBioEntity sequence = basePairs.First(bp => bp.Sequence != null).Sequence;
                
                _seqVM = new CircleSequenceViewModel(sequence);
                PhyloDVM = new PhyloDCircleViewModel(_seqVM);
                LoadPhyloDDataCommand = new DelegatingCommand(_phyloDVM.OnLoadPhyloDData);
                Width = _seqVM.Width;
                Height = _seqVM.Height;

                var basePairVM = from bp in basePairs
                                 select new CircleBasePairViewModel(_seqVM, bp);

                foreach (var vm in basePairVM) { ModelElements.Add(vm); }
                ModelElements.Add(_seqVM);
                foreach (var vm in _seqVM.Elements) { ModelElements.Add(vm); }
                foreach (var vm in _seqVM.TickMarks) { ModelElements.Add(vm); }
                foreach (var vm in _seqVM.TickLabels) { ModelElements.Add(vm); }
            }
            return base.Initialize(data);
        }

        public CircleViewModel()
        {
            RegisterWithMessageMediator();
            ModelElements = new MTObservableCollection<CircleElementBaseViewModel>();
        }

        private CircleSequenceViewModel _seqVM;

        [MessageMediatorTarget(CircleStructureBrowserMessages.ShowStatusMessage)]
        public void OnDisplayStatusMessage(object parameter)
        {
            if (parameter != null)
            {
                Status = parameter.ToString();
            }
        }

        [MessageMediatorTarget(CircleStructureBrowserMessages.LoadAllPhyloDInteractions)]
        public void OnLoadAllPhyloDInteractions(List<PhyloDInteractionCircleViewModel> vms)
        {
            foreach (var vm in vms)
            {
                ModelElements.Add(vm);
            }
        }
    }
}
