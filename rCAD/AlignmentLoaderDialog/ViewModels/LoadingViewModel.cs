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
using System.Text;
using JulMar.Windows.Mvvm;
using Alignment;
using System.Windows.Input;
using System.Collections.ObjectModel;
using JulMar.Windows;
using Microsoft.Win32;
using System.ComponentModel;
using Utilities.Data;
using JulMar.Windows.Interfaces;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;

namespace AlignmentLoaderDialog.ViewModels
{
    public class LoadingViewModel : ViewModel
    {
        public string Icon
        {
            get { return "Images/alignmentDB.ico"; }
        }

        public rCADConnection RCADConnectionObj
        {
            get { return _rCADConnection; }
        }

        public string ConnectionString
        {
            get { return _rCADConnection.BuildConnectionString(); }
        }

        public string Host
        {
            get { return _rCADConnection.Host; }
            set
            {
                _rCADConnection.Host = value;
                ConnectionValid = false;
                InstanceDatabases.Clear();
                OnPropertiesChanged("Host", "ConnectionString", "RCADConnectionObj");
            }
        }

        public string Instance
        {
            get { return _rCADConnection.Instance; }
            set
            {
                _rCADConnection.Instance = value;
                ConnectionValid = false;
                InstanceDatabases.Clear();
                OnPropertiesChanged("Instance", "ConnectionString", "RCADConnectionObj");
            }
        }

        public bool IsSQLServerSecurity
        {
            get { return _rCADConnection.SecurityType == SecurityType.SQLServerAuthentication; }
            set
            {
                _rCADConnection.SecurityType = (value == false) ? SecurityType.WindowsAuthentication : SecurityType.SQLServerAuthentication;
                ConnectionValid = false;
                InstanceDatabases.Clear();
                OnPropertiesChanged("IsSQLServerSecurity", "ConnectionString", "RCADConnectionObj");
            }
        }

        public bool IsWindowsSecurity
        {
            get { return _rCADConnection .SecurityType == SecurityType.WindowsAuthentication; }
            set
            {
                _rCADConnection.SecurityType = (value == false) ? SecurityType.SQLServerAuthentication : SecurityType.WindowsAuthentication;
                ConnectionValid = false;
                InstanceDatabases.Clear();
                OnPropertiesChanged("IsWindowsSecurity", "ConnectionString", "RCADConnectionObj");
            }
        }

        public string Username
        {
            get { return _rCADConnection.Username; }
            set
            {
                _rCADConnection.Username = value;
                ConnectionValid = false;
                InstanceDatabases.Clear();
                OnPropertiesChanged("Username", "ConnectionString", "RCADConnectionObj");
            }
        }

        public string Password
        {
            get { return _rCADConnection.Password; }
            set
            {
                _rCADConnection.Password = value;
                ConnectionValid = false;
                InstanceDatabases.Clear();
                OnPropertiesChanged("Password", "ConnectionString", "RCADConnectionObj");
            }
        }

        public string Database
        {
            get { return _rCADConnection.Database; }
            set
            {
                _rCADConnection.Database = value;
                OnPropertiesChanged("Database", "ConnectionString", "RCADConnectionObj");
            }
        }

        public ObservableCollection<string> InstanceDatabases
        {
            get { return _instanceDatabases; }
        }

        public bool ConnectionValid
        {
            get { return _connectionValid; }
            set { _connectionValid = value; OnPropertyChanged("ConnectionValid"); }
        }

        public List<AlignmentTypeViewModel> AlignmentTypes
        {
            get { return _alignmentTypes; }
        }

        public int CurrentStep
        {
            get { return _step; }
            private set { _step = value; OnPropertyChanged("CurrentStep"); }
        }

        public AlignmentTypeViewModel LoadingAlignmentType 
        { 
            get { return _loadingAlignmentType; }
            set
            {
                _loadingAlignmentType = value;
                if (!OnSelectAlignment()) _loadingAlignmentType = null;
                OnPropertyChanged("LoadingAlignmentType");
            }
        }

        public AlignmentViewModel Alignment
        {
            get { return _alignmentViewModel; }
            private set
            {
                _alignmentViewModel = value;
                OnPropertiesChanged("Alignment", "IsAlignmentLoaded");
            }
        }

        public bool IsAlignmentLoaded
        {
            get { return (_alignmentViewModel == null) ? false : true; }
        }

        public string LoadingAlignmentFile
        {
            get { return _loadingAlignmentFile; }
            private set
            {
                _loadingAlignmentFile = value;
                OnPropertyChanged("LoadingAlignmentFile");
            }
        }

        public string LoadingAlignmentStatusMessage
        {
            get { return _loadingAlignmentStatusMessage; }
            private set { _loadingAlignmentStatusMessage = value; OnPropertyChanged("LoadingAlignmentStatusMessage"); }
        }

        public bool UsingSQLExpress
        {
            get { return _usingSQLExpress; }
            set
            {
                _usingSQLExpress = value;
                OnPropertyChanged("UsingSQLExpress");
            }
        }

        public string TargetDirectory
        {
            get { return _targetDirectory; }
            set
            {
                _targetDirectory = value;
                if (!string.IsNullOrEmpty(_targetDirectory))
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(_targetDirectory);
                        if (attributes == FileAttributes.Directory)
                        {
                            if (Directory.GetFileSystemEntries(_targetDirectory).Length > 0)
                            {
                                IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                                MessageResult result = mesg.Show("Warning",
                                    "Selected directory is not empty, an empty directory is preferred.",
                                    MessageButtons.OKCancel);
                                if (result == MessageResult.Cancel)
                                {
                                    _targetDirectory = null;
                                    OnPropertyChanged("TargetDirectory");
                                    return;
                                }
                            }
                            //Test to see if we can write the existing directory
                            try
                            {
                                string testFile = _targetDirectory + "\\" + Guid.NewGuid();
                                StreamWriter test = File.CreateText(testFile);
                                test.Close();
                                File.Delete(testFile);
                                OnPropertyChanged("TargetDirectory");
                                return;
                            }
                            catch (Exception ce)
                            {
                                IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                                mesg.Show("Error",
                                    string.Format("{0}\n. Please identify a different output directory.", ce.Message),
                                    MessageButtons.OK);
                                _targetDirectory = null;
                                OnPropertyChanged("TargetDirectory");
                                return;
                            }   
                        }
                        else
                        {
                            IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                            mesg.Show("Error",
                                "Please identify an output directory, not a file",
                                MessageButtons.OK);
                            _targetDirectory = null;
                            OnPropertyChanged("TargetDirectory");
                            return;
                        }
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        //Test to see if we can create the requested directory.
                        try
                        {
                            Directory.CreateDirectory(_targetDirectory);
                            Directory.Delete(_targetDirectory);
                            OnPropertyChanged("TargetDirectory");
                            return;
                        }
                        catch (Exception e)
                        {
                            IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                            mesg.Show("Error",
                                string.Format("Error: {0}\nPlease select/create a different output directory", e.Message),
                                MessageButtons.OK);
                            _targetDirectory = null;
                            OnPropertyChanged("TargetDirectory");
                            return;
                        }
                    }                        
                }
            }
        }

        public ICommand NextCommand { get; private set; }
        public ICommand PreviousCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand LoadAlignmentCommand { get; private set; }
        public ICommand ClearAlignmentCommand { get; private set; }
        public ICommand TestDBConnectionCommand { get; private set; }
        public ICommand SelectOutputDirectoryCommand { get; private set; }

        public LoadingViewModel()
        {
            NextCommand = new DelegatingCommand(OnNextStep, OnCanGoToNextStep);
            PreviousCommand = new DelegatingCommand(OnPreviousStep, OnCanGoToPreviousStep);
            ExitCommand = new DelegatingCommand(OnExit, OnCanExit);
            LoadAlignmentCommand = new DelegatingCommand(OnLoadAlignment, OnCanLoadAlignment);
            ClearAlignmentCommand = new DelegatingCommand(OnClearAlignment, () => IsAlignmentLoaded);
            TestDBConnectionCommand = new DelegatingCommand(OnTestDBConnection);
            SelectOutputDirectoryCommand = new DelegatingCommand(OnSelectOutputDirectory);
            _instanceDatabases = new ObservableCollection<string>();
            _rCADConnection = new rCADConnection();
            _rCADConnection.Host = ".";
            _rCADConnection.Instance = string.Empty;
            _rCADConnection.Database = string.Empty;
            _step = 1;
            _loadingAlignmentStatusMessage = string.Empty;
            UsingSQLExpress = false;
            Initialize();
        }

        private ObservableCollection<string> _instanceDatabases;
        private rCADConnection _rCADConnection;
        private bool _connectionValid = false;
        //private bool _testingConnection = false;
        private int _step;
        private bool _loadingAlignment = false;
        private AlignmentTypeViewModel _loadingAlignmentType;
        private string _loadingAlignmentFile;
        private AlignmentViewModel _alignmentViewModel;
        private List<AlignmentTypeViewModel> _alignmentTypes;
        private string _loadingAlignmentStatusMessage;
        private BackgroundWorker _worker;
        private bool _usingSQLExpress;
        private string _targetDirectory;

        private static string LOADING_ALIGNMENT_MESSAGE = "Loading Alignment...";
        private static string FINISHED_LOADING_ALIGNMENT_MESSAGE = "Finished Loading Alignment...";
        private static string LOADING_ALIGNMENT_FAILED_MESSAGE = "Failed Loading Alignment...";

        //In here, we only want to do things that take a relatively short period of time.
        private void Initialize()
        {
            _alignmentTypes = new List<AlignmentTypeViewModel>();
            foreach(AlignmentType type in Enum.GetValues(typeof(AlignmentType)))
            {
                _alignmentTypes.Add(new AlignmentTypeViewModel() { Value = type, Text = type.ToString() });
            }
        }

        private bool OnCanGoToNextStep()
        {
            switch (_step)
            {
                case 1:
                    if (!UsingSQLExpress)
                        return ConnectionValid && !string.IsNullOrEmpty(Database);
                    else
                        return ConnectionValid && !string.IsNullOrEmpty(Database) && !string.IsNullOrEmpty(TargetDirectory);
                case 2:
                    return IsAlignmentLoaded && !_loadingAlignment;
                default:
                    return false;
            };
        }

        private bool OnCanGoToPreviousStep()
        {
            return (_step >= 1 && !_loadingAlignment && 
                !(IsAlignmentLoaded && Alignment.MappingToRCAD) && 
                !(IsAlignmentLoaded && Alignment.LoadingToRCAD) && 
                !(IsAlignmentLoaded && Alignment.IsLoadedToRCAD) &&
                !(IsAlignmentLoaded && Alignment.LoadingToRCADFailed)) ? true : false;
        }

        private bool OnCanExit()
        {
            return !(IsAlignmentLoaded && Alignment.MappingToRCAD) &&
                !(IsAlignmentLoaded && Alignment.LoadingToRCAD) ? true : false;
        }

        private bool OnCanLoadAlignment()
        {
            return (LoadingAlignmentFile != null && !IsAlignmentLoaded && !_loadingAlignment) ? true : false;
        }

        private void OnPreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        }

        private void OnNextStep()
        {
            if (CurrentStep <= 2)
            {
                CurrentStep++;
            }
        }

        private void OnExit()
        {
            App.Current.Shutdown();
        }

        private void OnSelectOutputDirectory()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Folders only|*.FOLDER",
                ValidateNames = false,
                FileName = "(Folder)",
                Title = "Select Output Directory..."
            };
            if (dialog.ShowDialog().Value)
            {
                string tempSelection = dialog.FileName;
                if (tempSelection != null && tempSelection.EndsWith("(Folder)"))
                {
                    tempSelection = tempSelection.Remove(tempSelection.Length - 8);
                }
                TargetDirectory = tempSelection;
            }
        }

        //Might want to put this in a worker thread.
        private void OnTestDBConnection()
        {
            if (!_rCADConnection.TestConnection())
            {
                IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                mesg.Show("Error",
                    string.Format("Error: Database connection test failed. Either database is not available or the configured connection is invalid."),
                    MessageButtons.OK);
                ConnectionValid = false;
            }
            else
            {
                ConnectionValid = true;
                if (Alignment != null) Alignment.ClearRCADMapping();
                //Load instance databases
                InstanceDatabases.Clear(); //If it was previously populated.
                ServerConnection conn = new ServerConnection();
                conn.ConnectionString = ConnectionString;
                Server sqlServer = new Server(conn);
                foreach (Database db in sqlServer.Databases)
                {
                    InstanceDatabases.Add(db.Name);
                }
            }
        }

        private void OnClearAlignment()
        {
            Alignment = null;
            LoadingAlignmentFile = null;
            LoadingAlignmentType = null;
            LoadingAlignmentStatusMessage = string.Empty;
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnLoadAlignment()
        {
            LoadingAlignmentStatusMessage = LOADING_ALIGNMENT_MESSAGE;
            _loadingAlignment = true;
            CommandManager.InvalidateRequerySuggested();

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(LoadAlignmentWorker);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadAlignmentWorkerCompleted);
            _worker.RunWorkerAsync(new SequenceAlignmentLoaderArgs() { AlignmentFile = LoadingAlignmentFile, AlignmentType = LoadingAlignmentType.Value });
        }

        private bool OnSelectAlignment()
        {
            if (LoadingAlignmentType != null)
            {
                string fileFilter = string.Empty;
                switch(LoadingAlignmentType.Value) {
                    case AlignmentType.CRW:
                        fileFilter = "CRW Packaged Alignments (.crw, .zip) | *.crw;.zip";
                        break;
                    /*case AlignmentType.Genbank:
                        fileFilter = "Genbank Alignments (.gb, .gen) | *.gb;.gen";
                        break;*/
                };
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Filter = fileFilter,
                    Multiselect = false,
                    CheckFileExists = true,
                    Title = "Select Alignment..."
                };

                if (dialog.ShowDialog().Value)
                {
                    LoadingAlignmentFile = dialog.FileName;
                    return true;
                }
            }
            return false;
        }

        private void LoadAlignmentWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                LoadingAlignmentStatusMessage = LOADING_ALIGNMENT_FAILED_MESSAGE;
                _loadingAlignment = false;
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            SequenceAlignment aln = e.Result as SequenceAlignment;
            _loadingAlignment = false;
            CommandManager.InvalidateRequerySuggested();
            LoadingAlignmentStatusMessage = FINISHED_LOADING_ALIGNMENT_MESSAGE;
            Alignment = new AlignmentViewModel(aln);
        }

        private void LoadAlignmentWorker(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null) return;
            SequenceAlignmentLoaderArgs args = e.Argument as SequenceAlignmentLoaderArgs;
            if (args != null)
            {
                if (args.AlignmentFile == null) return;
                if (args.AlignmentType == AlignmentType.CRW)
                {
                    ISequenceAlignmentLoader loader = new CRWSequenceAlignmentLoader();
                    SequenceAlignment aln = loader.Load(args.AlignmentFile);
                    e.Result = aln;
                }
            }
        }
    }

    internal class SequenceAlignmentLoaderArgs
    {
        public string AlignmentFile { get; set; }
        public AlignmentType AlignmentType { get; set; }
    }
}
