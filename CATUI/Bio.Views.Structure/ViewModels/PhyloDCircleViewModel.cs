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
using System.Threading;
using Bio.Views.Structure.Models;
using JulMar.Windows;
using JulMar.Windows.Mvvm;
using Microsoft.Win32;

namespace Bio.Views.Structure.ViewModels
{
    public class PhyloDCircleViewModel : ViewModel
    {
        public MTObservableCollection<PhyloDInteractionCircleViewModel> PhyloDElements { get; private set; }

        public bool _interactionDataLoaded;
        public bool IsInteractionDataLoaded
        {
            get { return _interactionDataLoaded; }
            private set
            {
                _interactionDataLoaded = value;
                if (_interactionDataLoaded)
                {
                    SendMessage(CircleStructureBrowserMessages.LoadAllPhyloDInteractions, _allInteractions);
                }
                OnPropertyChanged("IsInteractionDataLoaded");
            }
        }

        public bool _isFilteringInteractions;
        public bool IsInteractionDataFiltered
        {
            get { return _isFilteringInteractions; }
            set
            {
                _isFilteringInteractions = value;
                PhyloDElements.Clear();
                if (!_isFilteringInteractions && IsInteractionDataLoaded)
                {
                    foreach (var vm in _allInteractions) { PhyloDElements.Add(vm); }
                }
                OnPropertyChanged("IsInteractionDataFiltered");
            }
        }

        public bool ShowAllInteractions
        {
            get { return !IsInteractionDataFiltered; }
        }

        public PhyloDCircleViewModel(CircleSequenceViewModel seqVM)
        {
            RegisterWithMessageMediator();
            _seqVM = seqVM;
            PhyloDElements = new MTObservableCollection<PhyloDInteractionCircleViewModel>();
            IsInteractionDataLoaded = false;
            IsInteractionDataFiltered = false;
        }

        private Dictionary<int, List<PhyloDInteractionCircleViewModel>> _orderedInteractions;
        private List<PhyloDInteractionCircleViewModel> _allInteractions;
        private CircleSequenceViewModel _seqVM;

        public void OnLoadPhyloDData()
        {
            OpenFileDialog openPhyloDDataFileDialog = new OpenFileDialog();
            openPhyloDDataFileDialog.Title = "Select PhyloD Association Data Flat File...";
            if (openPhyloDDataFileDialog.ShowDialog().Value)
            {
                PhyloDElements.Clear();
                if (_orderedInteractions != null) _orderedInteractions.Clear();
                else _orderedInteractions = new Dictionary<int, List<PhyloDInteractionCircleViewModel>>();
                if (_allInteractions != null) _allInteractions.Clear();
                else _allInteractions = new List<PhyloDInteractionCircleViewModel>();
                string filename = openPhyloDDataFileDialog.FileName;
                SendMessage(CircleStructureBrowserMessages.ShowStatusMessage, "Loading PhyloD Data...");
                new Thread(LoadPhyloDInteractions) { IsBackground = true, Name = "Association Loader Thread" }.Start(filename);
            }
        }

        [MessageMediatorTarget(CircleStructureBrowserMessages.ShowPhyloDInteractions)]
        private void OnShowOnlyPhyloDInteractionsForPredictorIndex(int predictorIndex)
        {
            if (IsInteractionDataLoaded && IsInteractionDataFiltered)
            {
                PhyloDElements.Clear();
                if (_orderedInteractions.ContainsKey(predictorIndex))
                {
                    foreach (var vm in _orderedInteractions[predictorIndex])
                    {
                        PhyloDElements.Add(vm);
                    }
                }
            }
        }

        private void LoadPhyloDInteractions(object filenameArg)
        {
            IEnumerable<PhyloDInteraction> associations = PhyloDData.Load((string)filenameArg, _seqVM.Sequence);
            foreach (PhyloDInteraction association in associations)
            {
                PhyloDInteractionCircleViewModel assVM = new PhyloDInteractionCircleViewModel(association, _seqVM);
                if (!_orderedInteractions.ContainsKey(association.PredictorIndex))
                {
                    _orderedInteractions.Add(association.PredictorIndex, new List<PhyloDInteractionCircleViewModel>());
                }
                _orderedInteractions[association.PredictorIndex].Add(assVM);
                _allInteractions.Add(assVM);
            }

            IsInteractionDataLoaded = true;
            IsInteractionDataFiltered = false;
            SendMessage(CircleStructureBrowserMessages.ShowStatusMessage, string.Format("Loaded {0} Associations", _allInteractions.Count));
        }


    }
}
