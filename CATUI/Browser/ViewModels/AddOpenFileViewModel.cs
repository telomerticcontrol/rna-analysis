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
using JulMar.Windows.Mvvm;
using System.Windows.Input;

namespace BioBrowser.ViewModels
{
    /// <summary>
    /// This ViewModel is used in the add file option in Workspaces.
    /// </summary>
    public class AddOpenFileViewModel : ViewModel
    {
        /// <summary>
        /// This is the open file definition.
        /// </summary>
        public class OpenFileDefinitionViewModel : ViewModel
        {
            private bool _isSelected;
            internal OpenBioDataViewModel _data;

            public string Text { get { return _data.Header; } }
            public string Details { get { return _data.LoadData;  } }
            public bool InWorkspace { get; private set; }

            public bool IsSelected
            {
                get { return _isSelected; }
                set { _isSelected = value; OnPropertyChanged("IsSelected"); }
            }

            public OpenFileDefinitionViewModel(OpenBioDataViewModel vm, bool inWorkspace)
            {
                _data = vm;
                InWorkspace = inWorkspace;
            }
        }

        public ObservableCollection<OpenFileDefinitionViewModel> Children { get; private set; }
        public ICommand SelectChildrenCommand { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="openFiles"></param>
        /// <param name="existingFiles"></param>
        public AddOpenFileViewModel(IEnumerable<OpenBioDataViewModel> openFiles, IEnumerable<OpenBioDataViewModel> existingFiles)
        {
            Children = new ObservableCollection<OpenFileDefinitionViewModel>();
            SelectChildrenCommand = new DelegatingCommand(delegate { /* Do nothing */ }, HasSelectedItems);

            foreach (var file in openFiles)
                Children.Add(new OpenFileDefinitionViewModel(file, existingFiles.Contains(file)));
        }

        public bool HasSelectedItems()
        {
            return Children.Any(child => child.IsSelected);
        }

        public IEnumerable<OpenBioDataViewModel> SelectedFiles
        {
            get { return Children.Where(child => child.IsSelected).Select(child => child._data); }
        }
    }
}
