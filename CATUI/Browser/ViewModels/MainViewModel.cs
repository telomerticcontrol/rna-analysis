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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Windows.Input;
using Bio.Data.Providers.Interfaces;
using Bio.Views;
using Bio.Views.ViewModels;
using BioBrowser.Models;
using JulMar.Windows.Extensions;
using JulMar.Windows.Interfaces;
using JulMar.Windows.Mvvm;
using Bio.Views.Interfaces;
using Microsoft.Win32;

namespace BioBrowser.ViewModels
{
    /// <summary>
    /// This class represents the primary View Model which supports the BioBrowser application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private BioViewModel _selectedView, _activeView;
        private string[] _commandLine;

        /// <summary>
        /// Sidebar items
        /// </summary>
        public ObservableCollection<ISidebarViewItem> SidebarItems { get; private set; }

        /// <summary>
        /// This holds all created and visible views (docked + floating)
        /// </summary>
        public ObservableCollection<BioViewModel> AllViews { get; private set; }

        /// <summary>
        /// Holds all the docked windows
        /// </summary>
        public ObservableCollection<BioViewModel> DockedViews { get; private set; }

        /// <summary>
        /// The open data sources
        /// </summary>
        public ObservableCollection<SelectionViewModel> OpenDataSources { get; private set; }

        /// <summary>
        /// Main menu items
        /// </summary>
        public ObservableCollection<MenuItem> OpenFileMenu { get; private set; }

        /// <summary>
        /// The options from the currently selected view
        /// </summary>
        public ObservableCollection<MenuItem> OptionsMenu
        {
            get
            {
                return SelectedView != null ? SelectedView.OptionsMenu : null;
            }
        }

        /// <summary>
        /// The view options from the currently selected view
        /// </summary>
        public ObservableCollection<MenuItem> ViewMenu
        {
            get
            {
                return SelectedView != null ? SelectedView.ViewMenu : null;
            }
        }

        /// <summary>
        /// The edit from the currently selected view
        /// </summary>
        public ObservableCollection<MenuItem> EditMenu
        {
            get
            {
                return SelectedView != null ? SelectedView.EditMenu : null;
            }
        }

        /// <summary>
        /// The currently selected view in the menu
        /// </summary>
        public BioViewModel SelectedView
        {
            get { return _selectedView; }
            set 
            {
                if (_selectedView == value)
                    return;

                // Set the selected tab
                _selectedView = value;
                SetActiveView(value);

                OnPropertyChanged("SelectedView", "OptionsMenu", "EditMenu", "ViewMenu"); 
            }
        }

        /// <summary>
        /// Command line passed to the application
        /// </summary>
        public string[] CommandLine
        {
            get { return _commandLine; }
            set { _commandLine = value; ProcessCommandLine(); }
        }

        /// <summary>
        /// This is the OpenDataFile command - it displays the OpenFileDialog for all valid file types.
        /// </summary>
        public ICommand OpenDataFile { get; private set; }

        /// <summary>
        /// This closes the application
        /// </summary>
        public ICommand CloseApplication { get; private set; }

        /// <summary>
        /// This activates the passed view
        /// </summary>
        public ICommand ActivateView { get; private set; }

        /// <summary>
        /// This closes the current view
        /// </summary>
        public ICommand CloseCurrentView { get; private set; }

        /// <summary>
        /// This creates a new workspace
        /// </summary>
        public ICommand CreateNewWorkspace { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            // Create our commands
            OpenDataFile = new DelegatingCommand(OpenFileDataSource);
            ActivateView = new DelegatingCommand<BioViewModel>(OnActivateView);
            CloseApplication = new DelegatingCommand(RaiseCloseRequest);
            CloseCurrentView = new DelegatingCommand(() => SelectedView.RaiseCloseRequest(), () => SelectedView != null);
            CreateNewWorkspace = new DelegatingCommand(OnCreateNewWorkspace);

            OpenFileMenu = new ObservableCollection<MenuItem>();

            // Initialize MEF and add ourselves and the data loaders
            try
            {
                MefManager.Current.ComposeParts(this, Extensions.Current);
            }
            catch (CompositionException ex)
            {
                var errorVisualizer = ServiceProvider.Resolve<IErrorVisualizer>();
                if (errorVisualizer != null)
                    errorVisualizer.Show("Failed to locate required components", ex.Message);
                Environment.Exit(-1);
            }

            // Get all the loaders
            var formatLoaders = Extensions.Current.DataProviders;
            if (formatLoaders == null || formatLoaders.Count() == 0)
            {
                var errorVisualizer = ServiceProvider.Resolve<IErrorVisualizer>();
                if (errorVisualizer != null)
                    errorVisualizer.Show("Failed to locate required components", "No Data Providers found - make sure all required assemblies are copied into the deployment directory.");
                Environment.Exit(-1);
            }

            // Register file format styles
            formatLoaders
                .Where(fl => !String.IsNullOrEmpty(fl.FileFilter))
                .Select(fl => fl.FileFilter)
                .ForEach(RegisterFileExtension);

            // Register with message mediator
            RegisterWithMessageMediator();

            // Setup the docked views + menu handlers
            AllViews = new ObservableCollection<BioViewModel>();
            DockedViews = new ObservableCollection<BioViewModel>();
            SidebarItems = new ObservableCollection<ISidebarViewItem>();
            OpenDataSources = new ObservableCollection<SelectionViewModel>();

            // See if we have files we can open.  If so, add a File|Open DataFile menu option.
            if (formatLoaders.Any(ldr => !string.IsNullOrEmpty(ldr.FileFilter)))
                OpenFileMenu.Add(new MenuItem("Data _File") { Command = OpenDataFile, GestureText = "CTRL+O" });

            // Add in data sources which are not file based.  Each one of these gets its own menu item.
            foreach (var ldr in formatLoaders.Where(ldr => string.IsNullOrEmpty(ldr.FileFilter)))
            {
                var innerLoader = ldr;
                OpenFileMenu.Add(new MenuItem(ldr.Description)
                                     {Command = new DelegatingCommand(() => OpenDbDataSource(innerLoader))});
            }
        }

        /// <summary>
        /// This is called to prompt the user for a file data source.
        /// </summary>
        private void OpenFileDataSource()
        {
            // Get a list of all the file types we can load.
            var loaders = Extensions.Current.DataProviders.Where(ldr => !string.IsNullOrEmpty(ldr.FileFilter));
            StringBuilder sb = new StringBuilder();

            foreach (var extension in loaders.Select(ldr => ldr.FileFilter.Split('|')))
            {
                for (int i = 1; i < extension.Count() ; i+=2)
                    sb.Append(extension[i]);
                sb.Append(";");
            }

            string allExtensions = sb + "*" + Workspace.WS_EXTENSION;
            sb.Clear();
            sb.Append("All Supported Formats|" + allExtensions);
            sb.Append("|Bio Workspaces|*" + Workspace.WS_EXTENSION);

            foreach (var ldr in loaders.Where(ldr => !string.IsNullOrEmpty(ldr.FileFilter)))
                sb.Append("|" + ldr.FileFilter);

            OpenFileDialog ofd = new OpenFileDialog { Filter = sb.ToString() };
            if (ofd.ShowDialog().Value)
            {
                bool rc = false;
                var filename = ofd.FileName;
                var formatLoader = GetProviderForFile(filename);
                if (formatLoader == null)
                {
                    var ws = Workspace.Load(filename);
                    if (ws != null)
                    {
                        OpenDataSources.Add(new WorkspaceViewModel(ws));
                        rc = true;
                    }
                }
                else
                {
                    rc = OpenFile(formatLoader, filename);
                }

                if (!rc)
                {
                    var errorVisualizer = ServiceProvider.Resolve<IErrorVisualizer>();
                    if (errorVisualizer != null)
                    {
                        errorVisualizer.Show("Failed to open file",
                                             "Could not open " + filename +
                                             ".\r\nThe provider could not be found or the file is not in the correct format.");
                    }
                }
            }
        }

        /// <summary>
        /// Routine to open a file
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool OpenFile(IBioDataProvider entry, string filename)
        {
            IBioDataLoader dataLoader = entry.Create(filename);
            if (dataLoader != null)
            {
                var dvm = new OpenBioDataViewModel(filename, entry.SupportedTypes, dataLoader, entry);
                dvm.Load();
                dvm.CloseRequest += OnDataSourceClosing;

                OpenDataSources.Add(dvm);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Event handler called when a data source is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDataSourceClosing(object sender, EventArgs e)
        {
            var dvm = sender as OpenBioDataViewModel;
            if (dvm != null)
            {
                dvm.CloseRequest -= OnDataSourceClosing;
                OpenDataSources.Remove(dvm);
            }
        }

        /// <summary>
        /// This prompts the user for a non-file based data load.
        /// </summary>
        /// <param name="dataProvider"></param>
        private void OpenDbDataSource(IBioDataProvider dataProvider)
        {
            Debug.Assert(string.IsNullOrEmpty(dataProvider.FileFilter));
            OpenFile(dataProvider, null);
        }

        /// <summary>
        /// This method creates a new workspace
        /// </summary>
        private void OnCreateNewWorkspace()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                CheckPathExists = true,
                AddExtension = true,
                Filter = "Bio Workspaces|*" + Workspace.WS_EXTENSION,
                RestoreDirectory = true,
                DefaultExt = Workspace.WS_EXTENSION,
                OverwritePrompt = true,
                Title = "Select the name and location of the new Workspace to create."
            };
            bool? result = sfd.ShowDialog();
            if (result.Value)
            {
                var ws = new Workspace {Filename = sfd.FileName, Name = Path.GetFileNameWithoutExtension(sfd.FileName)};
                OpenDataSources.Add(new WorkspaceViewModel(ws));
                ws.Save();
            }
        }

        /// <summary>
        /// This is invoked when the user closes a workspace.
        /// </summary>
        /// <param name="vm">Workspace ViewModel that was closed</param>
        [MessageMediatorTarget(ViewMessages.RemoveWorkspace)]
        private void OnRemoveWorkspace(WorkspaceViewModel vm)
        {
            OpenDataSources.Remove(vm);
        }

        /// <summary>
        /// This returns the IBioDataProvider for a given filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static IBioDataProvider GetProviderForFile(string filename)
        {
            var formatLoaders = Extensions.Current.DataProviders;
            string extension = Path.GetExtension(filename).ToUpper().Trim();
            foreach (var loader in formatLoaders.Where(fl => !string.IsNullOrEmpty(fl.FileFilter)))
            {
                foreach (var format in loader.FileFilter.Split(','))
                {
                    var extensionList = format.Split('|');
                    if (extensionList.Length != 2)
                        continue;

                    string fileExtension = extensionList[1].ToUpper().Trim();
                    if (fileExtension.StartsWith("*."))
                        fileExtension = fileExtension.Substring(1);
                    if (fileExtension == extension)
                        return loader;
                }
            }
            return null;
        }

        /// <summary>
        /// This activates a specific view.
        /// </summary>
        /// <param name="vm">View to activate</param>
        [MessageMediatorTarget(ViewMessages.ActivateView)]
        private void OnActivateView(BioViewModel vm)
        {
            if (vm.IsDocked)
                SelectedView = vm;
            else
                vm.RaiseActivateRequest();
        }

        /// <summary>
        /// This method processes the command line.  It is expected to contain a list of files to open.
        /// </summary>
        private void ProcessCommandLine()
        {
            foreach (var filename in CommandLine)
            {
                var loader = GetProviderForFile(filename);
                if (loader != null)
                    OpenFile(loader, filename);
            }
        }

        /// <summary>
        /// This is the target for the internal WCF service that listens for command line changes.
        /// </summary>
        /// <param name="commandLine"></param>
        [MessageMediatorTarget(ViewMessages.OpenFile)]
        private void OnSetCommandLine(string[] commandLine)
        {
            // Set the command line - this will load the file(s)
            CommandLine = commandLine;

            // Surface + Activate our window
            RaiseActivateRequest();
        }

        /// <summary>
        /// This method is invoked when a docked view is created or changed from floating to docked.
        /// </summary>
        /// <param name="vm">ViewModel for view</param>
        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.AddDockedView)]
        private void OnAddNewDockedWindow(BioViewModel vm)
        {
            DockedViews.Add(vm);
            SelectedView = vm;
        }

        /// <summary>
        /// This method is called when a new view is created
        /// </summary>
        /// <param name="vm"></param>
        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.ViewCreated)]
        private void OnNewViewCreated(BioViewModel vm)
        {
            AllViews.Add(vm);
        }

        /// <summary>
        /// This is called when a view is closed
        /// </summary>
        /// <param name="vm"></param>
        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.ViewClosed)]
        private void OnViewClosed(BioViewModel vm)
        {
            AllViews.Remove(vm);
        }

        /// <summary>
        /// This method is invoked when a docked view is removed (closed or changed to floating).
        /// </summary>
        /// <param name="vm">ViewModel being removed</param>
        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.RemoveDockedView)]
        private void OnRemoveDockedWindow(BioViewModel vm)
        {
            DockedViews.Remove(vm);
            if (SelectedView == vm)
                SelectedView = DockedViews.Count > 0 ? DockedViews[0] : null;
        }

        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.FloatingViewActivated)]
        private void OnFloatingWindowActivated(BioViewModel vm)
        {
            SetActiveView(vm);
        }

        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.AddFloatingWindowToSidebar)]
        private void OnAddToSidebar(ISidebarViewItem sidebarItem)
        {
            OnRemoveFromSidebar(sidebarItem);
            SidebarItems.Add(sidebarItem);
        }

        [MessageMediatorTarget(Bio.Views.ViewModels.ViewMessages.RemoveFloatingWindowFromSidebar)]
        private void OnRemoveFromSidebar(ISidebarViewItem sidebarItem)
        {
            // Remove all of that type.
            SidebarItems
                .Where(sbi => sbi.GetType() == sidebarItem.GetType())
                .ToList()
                .ForEach(item => SidebarItems.Remove(item));
        }

        /// <summary>
        /// This disposes of the view model.  It unregisters from the message mediator.
        /// </summary>
        /// <param name="isDisposing">True if IDisposable.Dispose was called</param>
        protected override void  Dispose(bool isDisposing)
        {
            OpenDataSources.ForEach(c => c.Dispose());
            base.Dispose(isDisposing);
        }

        /// <summary>
        /// This method writes the registry keys for a file format extension handler. We cannot
        /// do this in an installer since the file formats are dynamically loaded.
        /// </summary>
        /// <param name="fileFilter">File Filter</param>
        internal static void RegisterFileExtension(string fileFilter)
        {
            string[] formats = fileFilter.Split(',');
            foreach (var format in formats)
            {
                var extensionList = format.Split('|');
                if (extensionList.Length != 2)
                    continue;

                string extension = extensionList[1];
                if (extension.StartsWith("*."))
                    extension = extension.Substring(2);
                else if (extension.StartsWith("."))
                    extension = extension.Substring(1);

                // Attempt global registration
                if (Registry.CurrentUser.GetValue(@"Software\Classes\." + extension) == null)
                    RegisterExtensionForCurrentUser(extension);
            }
        }

        /// <summary>
        /// This registers the given extension for the current user.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static bool RegisterExtensionForCurrentUser(string extension)
        {
            try
            {
                var rootKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\." + extension);
                if (rootKey != null)
                    rootKey.SetValue(string.Empty, "BioBrowser." + extension, RegistryValueKind.String);
                rootKey = Registry.CurrentUser.CreateSubKey(string.Format(@"Software\Classes\BioBrowser.{0}\shell\open\command", extension));
                if (rootKey != null)
                    rootKey.SetValue(string.Empty, Assembly.GetEntryAssembly().Location + " \"%1\"", RegistryValueKind.String);

                rootKey = Registry.CurrentUser.CreateSubKey(string.Format(@"Software\Classes\BioBrowser.{0}\DefaultIcon", extension));
                if (rootKey != null)
                    rootKey.SetValue(string.Empty, Assembly.GetEntryAssembly().Location + @",0", RegistryValueKind.String);

                return true;
            }
            catch (SecurityException)
            {
            }

            return false;
        }

        /// <summary>
        /// The View calls this when it is activated
        /// </summary>
        public void OnMainWindowActivated()
        {
            SetActiveView(SelectedView);
        }

        /// <summary>
        /// This sets the "active" view
        /// </summary>
        /// <param name="vm"></param>
        private void SetActiveView(BioViewModel vm)
        {
            if (_activeView != vm)
            {
                if (_activeView != null)
                    _activeView.OnDeactivate();
                _activeView = vm;
                if (_activeView != null)
                    _activeView.OnActivate();
            }
        }
    }
}