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
using Bio.Data.Providers.Interfaces;
using Bio.Views;
using BioBrowser.Models;
using BioBrowser.Views;
using JulMar.Windows;
using JulMar.Windows.Extensions;
using JulMar.Windows.Mvvm;
using JulMar.Windows.Interfaces;

namespace BioBrowser.ViewModels
{
    /// <summary>
    /// This ViewModel is used for a workspace selection - this is where we have a subset of data source children
    /// that is loaded and saved together.
    /// </summary>
    public class WorkspaceViewModel : SelectionViewModel
    {
        internal Workspace WorkspaceData { get; private set; }

        /// <summary>
        /// List of data sources associated with the workspace.
        /// </summary>
        public MTObservableCollection<OpenBioDataViewModel> AvailableDataSources { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workspace"></param>
        public WorkspaceViewModel(Workspace workspace)
        {
            WorkspaceData = workspace;
            Header = WorkspaceData.Name;
            Image = @"images/workspace.ico";
            AvailableDataSources = new MTObservableCollection<OpenBioDataViewModel>();

            // Decide where each file goes -- we must have the proper loader available or we do not display the file.
            foreach (var file in WorkspaceData.DataSources)
                AddFileToWorkspace(file);

            IsExpanded = AvailableDataSources.Count > 0;
        }

        /// <summary>
        /// This adds a workspace entry to our available data source list.
        /// </summary>
        /// <param name="entry"></param>
        private void AddFileToWorkspace(WorkspaceEntry entry)
        {
            // Find the specific data loader responsible for this entry.
            var owner = Extensions.Current.DataProviders.Where(ldr => ldr.Key == entry.LoaderKey).FirstOrDefault();
            if (owner != null)
            {
                IBioDataLoader loader = owner.Create(entry.LoaderData);
                if (loader != null)
                {
                    var dvm = new OpenBioDataViewModel(entry.LoaderData, entry.FormatType, loader, owner);
                    dvm.CloseRequest += OnDataSourceClosing;
                    AvailableDataSources.Add(dvm);
                }
            }
            else
            {
                var uiMessage = Resolve<IErrorVisualizer>();
                if (uiMessage != null)
                {
                    uiMessage.Show("Error loading workspace",
                                   "Could not resolve loader provider for " + entry.FormatType +
                                   ".  File will be ignored.");
                }
            }
        }

        /// <summary>
        /// This is invoked when a data source is closed in the workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataSourceClosing(object sender, EventArgs e)
        {
            var dvm = (OpenBioDataViewModel) sender;
            dvm.CloseRequest -= OnDataSourceClosing;
            AvailableDataSources.Remove(dvm);
            
            // Save the change.
            Save();
        }

        /// <summary>
        /// This populates the context menu for the category
        /// </summary>
        public override List<MenuItem> MenuOptions
        {
            get
            {
                IsSelected = true;
                return new List<MenuItem>
                   {
                       new MenuItem("O_pen All Data Sources") { Command = new DelegatingCommand(OnOpenWorkspace, OnCanOpenWorkspace) },
                       new MenuItem("C_lose All Views") { Command = new DelegatingCommand(OnCloseWorkspace, OnCanCloseWorkspace) },
                       new MenuItem("Close _Workspace") { Command = new DelegatingCommand(OnRemoveWorkspace) },
                       new MenuItem(),
                       new MenuItem("Add Open _Data File") { Command = new DelegatingCommand(OnAddExistingFile, OnCanAddExistingFile)},
                   };
            }
        }
        
        private bool OnCanAddExistingFile()
        {
            var openFiles = new List<OpenBioDataViewModel>();
            return SendMessage(ViewMessages.QueryOpenViewModels, openFiles) && openFiles.Count > 0 &&
                   AvailableDataSources.Intersect(openFiles).Count() != openFiles.Count;
        }

        private void OnAddExistingFile()
        {
            var openFiles = new List<OpenBioDataViewModel>();
            SendMessage(ViewMessages.QueryOpenViewModels, openFiles);

            if (openFiles.Count > 0)
            {
                AddOpenFileViewModel afvm = new AddOpenFileViewModel(openFiles, AvailableDataSources);
                IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
                if (uiVisualizer != null)
                {
                    bool? result = uiVisualizer.ShowDialog(typeof (AddOpenFileView).ToString(), afvm);
                    if (result != null && result.Value)
                    {
                        foreach (var file in afvm.SelectedFiles)
                        {
                            AddFileToWorkspace(new WorkspaceEntry
                                   {
                                       FormatType = file.FormatType,
                                       LoaderData = file.LoadData,
                                       LoaderKey = file.LoaderKey
                                   });
                        }
                        Save();
                    }
                }
            }
        }

        private bool OnCanOpenWorkspace()
        {
            return AvailableDataSources.Any(child => !child.IsLoaded);
        }

        private void OnOpenWorkspace()
        {
            AvailableDataSources
                .Where(child => !child.IsLoaded && !child.IsLoading)
                .ForEach(child => child.Load());
        }


        private bool OnCanCloseWorkspace()
        {
            return AvailableDataSources.Any(child => child.IsLoaded);
        }

        private void OnCloseWorkspace()
        {
            AvailableDataSources
                .Where(child => child.IsLoaded)
                .ForEach(child => child.CloseCommand.Execute(null));
        }

        private void OnRemoveWorkspace()
        {
            // Close all views.
            OnCloseWorkspace();
            // Remove the workspace.
            SendMessage(ViewMessages.RemoveWorkspace, this);
        }

        public void Save()
        {
            WorkspaceData.DataSources.Clear();
            WorkspaceData.DataSources.AddRange(
                AvailableDataSources.Select(child => new WorkspaceEntry { LoaderData = child.LoadData, LoaderKey = child.LoaderKey, FormatType = child.FormatType }));
            WorkspaceData.Save();
        }
    }
}
