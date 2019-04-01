using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using JulMar.Windows.Extensions;
using JulMar.Windows.Mvvm;
using Bio.Data.Providers.rCAD.RI.Models;
using JulMar.Windows.Interfaces;
using System.Diagnostics;

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

namespace Bio.Data.Providers.rCAD.RI.ViewModels
{
    public class FilterListViewModel : ViewModel
    {
        private FilterViewModel _selectedFilter;

        /// <summary>
        /// The list of filters
        /// </summary>
        public ObservableCollection<FilterViewModel> Filters { get; private set; }

        /// <summary>
        /// Gets/Sets the selected filter.
        /// </summary>
        public FilterViewModel SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                OnPropertyChanged("SelectedFilter");
            }
        }

        /// <summary>
        /// Returns the selected filter
        /// </summary>
        public AlignmentFilter GetFilter()
        {
            return (_selectedFilter == null) ? null : _selectedFilter.Filter;
        }

        public ICommand CreateNewCommand { get; private set; }
        public ICommand CloseOkCommand { get; private set; }
        public ICommand EditConnectionCommand { get; private set; }
        public ICommand RemoveConnectionCommand { get; private set; }

        public FilterListViewModel(IEnumerable<AlignmentFilter> loadedFilters)
        {
            Filters = new ObservableCollection<FilterViewModel>();

            foreach (var conn in loadedFilters)
                Filters.Add(new FilterViewModel(conn, true));

            if (Filters.Count == 0)
                Filters.Add(new FilterViewModel());

            SelectedFilter = Filters[0];

            CreateNewCommand = new DelegatingCommand(OnCreateNewConnection);
            CloseOkCommand = new DelegatingCommand(OnOK, IsValidConnection);
            RemoveConnectionCommand = new DelegatingCommand(OnRemoveConnection, () => SelectedFilter != null);
            EditConnectionCommand = new DelegatingCommand(OnEditConnection, () => SelectedFilter != null);
        }

        private void OnCreateNewConnection()
        {
            // Create the new view
            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            Debug.Assert(uiVisualizer != null);
            FilterViewModel vm = new FilterViewModel();
            if (uiVisualizer.ShowDialog(RcadSequenceProvider.RCADRI_CREATE_CONNECTION_UI, vm).Value)
            {
                Filters.Add(vm);
                SelectedFilter = vm;
                OnOK();
            }
        }

        private void OnRemoveConnection()
        {
            IMessageVisualizer messageVisualizer = Resolve<IMessageVisualizer>();
            if (SelectedFilter != null &&
                messageVisualizer.Show("Delete Existing Connection", 
                    "Are you sure you want to delete " + SelectedFilter.Name, 
                    MessageButtons.YesNo) == MessageResult.Yes)
            {
                Filters.Remove(SelectedFilter);
                SelectedFilter.Dispose();
                SelectedFilter = null;
            }
        }

        private void OnEditConnection()
        {
            if (SelectedFilter != null)
            {
                // Create the new view
                IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
                Debug.Assert(uiVisualizer != null);
                if (uiVisualizer.ShowDialog(RcadSequenceProvider.RCADRI_CREATE_CONNECTION_UI, SelectedFilter) == true)
                    OnOK();
            }
        }

        private void OnOK()
        {
            if (SelectedFilter != null)
            {
                Filters.ForEach(f => f.Dispose());
                RaiseCloseRequest(true);
            }
        }

        private bool IsValidConnection()
        {
            return (SelectedFilter != null && SelectedFilter.IsValid);
        }
    }
}