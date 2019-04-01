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
using System.IO;
using System.Linq;
using System.Windows.Input;
using Bio.Views;
using Bio.Views.Interfaces;
using Bio.Views.ViewModels;
using BioBrowser.Models;
using JulMar.Windows;
using JulMar.Windows.Mvvm;
using Bio.Data.Providers;
using Bio.Data.Providers.Interfaces;

namespace BioBrowser.ViewModels
{
    /// <summary>
    /// This is a single open data source.
    /// </summary>
    public class OpenBioDataViewModel : SelectionViewModel
    {
        private bool _isLoaded, _isLoading;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set 
            { 
                _isLoaded = value;
                IsLoading = false; 
                OnPropertyChanged("IsLoaded"); 
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading");}
        }

        public MTObservableCollection<OpenBioViewModel> Children { get; private set; }

        private readonly BioDataFile _bioData;
        public string LoadData { get { return _bioData.LoadData; } }
        public string LoaderKey { get { return _bioData.LoaderKey; } }
        public BioFormatType FormatType { get { return _bioData.FormatType;  } }
        public IBioDataLoader Data { get { return _bioData.Loader; }}

        public ICommand DefaultCommand { get; private set; }
        public ICommand ChangePropertiesCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        /// <summary>
        /// Constructor for the bio file
        /// </summary>
        /// <param name="loadData"></param>
        /// <param name="format"></param>
        /// <param name="loader"></param>
        /// <param name="info"></param>
        public OpenBioDataViewModel(string loadData, BioFormatType format, IBioDataLoader loader, IBioDataProvider info)
        {
            if (format == BioFormatType.Unknown)
                throw new ArgumentException("BioFormatType cannot be unknown.");
            if (loader == null)
                throw new ArgumentNullException("loader");

            RegisterWithMessageMediator();

            DefaultCommand = new DelegatingCommand(OnOpenDefaultView, () => !IsLoading);
            CloseCommand = new DelegatingCommand(OnClose, () => IsLoaded || !IsLoading);
            ChangePropertiesCommand = new DelegatingCommand(OnChangeProperties, () => IsLoaded);
            Children = new MTObservableCollection<OpenBioViewModel>();

            _bioData = new BioDataFile(loadData, format, loader, info.Key);

            Header = string.Format("{0} [{1}]", loader.InitializationData, info.Description);
            Image = info.ImageUrl;
        }

        public void Load()
        {
            IsLoading = true;
            _bioData.LoadAsync(b => IsLoaded = b);
        }

        /// <summary>
        /// This is used to change the properties of a connection
        /// </summary>
        private void OnChangeProperties()
        {
            IBioDataLoaderProperties bioProps = _bioData.Loader as IBioDataLoaderProperties;
            if (bioProps != null)
            {
                bioProps.ChangeProperties();
            }
        }

        /// <summary>
        /// This opens the first available view.
        /// </summary>
        private void OnOpenDefaultView()
        {
            if (!IsLoaded)
            {
                Load();
                return;
            }

            if (Children.Count > 0)
            {
                Children[0].ActivateCommand.Execute(null);
            }
            else
            {
                var loader = Extensions.Current.Visualizations.Where(vis => (vis.SupportedFormats & _bioData.FormatType) > 0).FirstOrDefault();
                if (loader != null)
                    CreateViewCommand(loader);
            }
        }

        /// <summary>
        /// This populates the context menu for the open file
        /// </summary>
        public override List<MenuItem> MenuOptions
        {
            get
            {
                IsSelected = true;

                var menu = new List<MenuItem>();
                MenuItem miOpen = new MenuItem("Show _View") { Command = new DelegatingCommand(delegate { }, () => IsLoaded)};
                foreach (var vi in Extensions.Current.Visualizations.Where(vis => (vis.SupportedFormats & _bioData.FormatType) > 0))
                {
                    var currentView = vi;
                    miOpen.Children.Add(new MenuItem(vi.Description)
                                            {ImageUri = currentView.ImageUrl, Command = new DelegatingCommand(() => CreateViewCommand(currentView), () => IsLoaded)});
                }
                if (miOpen.Children.Count > 0)
                    menu.Add(miOpen);

                menu.Add(new MenuItem("O_pen") { Command = new DelegatingCommand(Load, () => (!IsLoaded && !IsLoading)) });
                menu.Add(new MenuItem("C_lose") { Command = CloseCommand });
                if ((_bioData.Loader as IBioDataLoaderProperties) != null)
                {
                    menu.Add(new MenuItem());
                    menu.Add(new MenuItem("P_roperties...") {Command = ChangePropertiesCommand});
                }

                return menu;
            }
        }

        /// <summary>
        /// This method is invoked when a workspace is querying current (open)
        /// data files for adding to the workspace.
        /// </summary>
        /// <param name="openViews">List of open view models</param>
        [MessageMediatorTarget(ViewMessages.QueryOpenViewModels)]
        private void OnQueryOpenDataFile(List<OpenBioDataViewModel> openViews)
        {
            if (openViews != null)
                openViews.Add(this);
        }

        private void CreateViewCommand(IBioViewProvider viewInfo)
        {
            var viewModel = viewInfo.Create();
            viewModel.Title = Path.GetFileName(LoadData);

            if (!viewModel.Initialize(Data))
                viewModel.Dispose();
            else
            {
                Children.Add(new OpenBioViewModel(viewModel, viewInfo));
                IsExpanded = true;
            }
        }

        /// <summary>
        /// This method is invoked when a view representing this data is closed.
        /// </summary>
        /// <param name="vm"></param>
        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.ViewClosed)]
        private void OnViewClosed(BioViewModel vm)
        {
            if (vm != null)
            {
                var ovm = Children.FirstOrDefault(fvm => fvm.ViewModel == vm);
                if (ovm != null)
                    Children.Remove(ovm);
                vm.Dispose();
            }
        }

        /// <summary>
        /// This command closes all views and the underlying data source.
        /// </summary>
        private void OnClose()
        {
            // Close all the children
            var childCopy = new List<OpenBioViewModel>(Children);
            foreach (var child in childCopy)
                child.CloseCommand.Execute(null);   
            
            // Close this view
            RaiseCloseRequest();
        }
    }
}