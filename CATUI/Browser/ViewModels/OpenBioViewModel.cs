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
using System.Windows.Input;
using Bio.Views;
using Bio.Views.ViewModels;
using JulMar.Windows.Mvvm;
using Bio.Views.Interfaces;

namespace BioBrowser.ViewModels
{
    /// <summary>
    /// This represents a single open BioView
    /// </summary>
    public class OpenBioViewModel : SelectionViewModel
    {
        public BioViewModel ViewModel { get; set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand ActivateCommand { get; private set; }

        /// <summary>
        /// This populates the context menu for the open file
        /// </summary>
        public override List<MenuItem> MenuOptions
        {
            get
            {
                var menu = new List<MenuItem>();
                if (ViewModel != null)
                {
                    menu.Add(new MenuItem("C_lose") { Command = CloseCommand });
                    menu.Add(new MenuItem("_Activate View") { Command = ActivateCommand });
                }

                return menu;
            }
        }

        public OpenBioViewModel(BioViewModel vm, IBioViewProvider viewInfo)
        {
            ViewModel = vm;
            Header = viewInfo.Description;
            Image = viewInfo.ImageUrl;

            ActivateCommand = new DelegatingCommand(OnActivateView);
            CloseCommand = new DelegatingCommand(() => this.ViewModel.RaiseCloseRequest());
        }

        private void OnActivateView()
        {
            SendMessage(ViewMessages.ActivateView, this.ViewModel);
        }
    }
}