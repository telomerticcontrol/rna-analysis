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
using Utilities.Data;
using System.Windows.Input;
using JulMar.Windows.Interfaces;
using System.Collections.ObjectModel;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.ComponentModel;
using Microsoft.SqlServer.Dts.Runtime;
using System.IO;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Xml;
using System.Security.AccessControl;

namespace rCADLoaderDialog.ViewModels
{
    public class LoaderViewModel : ViewModel
    {
        public string Icon
        {
            get { return "Images/alignmentDB.ico"; }
        }

        public ObservableCollection<string> LocalDatabases
        {
            get { return _localDatabases; }
        }

        public string ConnectionString
        {
            get { return _connection.BuildConnectionString(); }
        }

        public string Host
        {
            get { return _connection.Host; }
            set
            {
                _connection.Host = value;
                ConnectionValid = false;
                OnPropertiesChanged("Host", "ConnectionString");
            }
        }

        public string Instance
        {
            get { return _connection.Instance; }
            set
            {
                _connection.Instance = value;
                ConnectionValid = false;
                OnPropertiesChanged("Instance", "ConnectionString");
            }
        }

        public string Database
        {
            get { return _desiredRCADDBName; }
            set
            {
                _desiredRCADDBName = value;
                OnPropertiesChanged("Database");
            }
        }

        public bool ConnectionValid
        {
            get { return _connectionValid; }
            set 
            {
                _connectionValid = value; 
                //Enumerate databases.
                OnPropertyChanged("ConnectionValid"); 
            }
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

        public string ScriptTargetFile
        {
            get { return _scriptTargetFile; }
            set
            {
                _scriptTargetFile = value;
                try
                {
                    if (!string.IsNullOrEmpty(_scriptTargetFile))
                    {
                        if (File.Exists(_scriptTargetFile))
                        {
                            IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                            MessageResult result = mesg.Show("Warning",
                                string.Format("Warning: Selected file {0} exists, do you want to overwrite this file?", _scriptTargetFile),
                                MessageButtons.YesNo);
                            if (result == MessageResult.No)
                            {
                                _scriptTargetFile = null;
                                OnPropertyChanged("ScriptTargetFile");
                                return;
                            }
                        }
                        StreamWriter test = File.CreateText(_scriptTargetFile);
                        test.Close();
                    }
                }
                catch (Exception uae)
                {
                    IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                    mesg.Show("Error",
                        string.Format("Error: {0}\nPlease select a different file name and/or location", uae.Message),
                        MessageButtons.OK);
                    _scriptTargetFile = null;
                }
                OnPropertyChanged("ScriptTargetFile");
            }
        }

        public int CurrentStep
        {
            get { return _step; }
            private set { _step = value; OnPropertyChanged("CurrentStep"); }
        }

        public bool CreatingDB
        {
            get { return _creatingDB; }
            set { _creatingDB = value; OnPropertyChanged("CreatingDB"); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged("StatusMessage"); }
        }

        public LoaderViewModel()
        {
            ExitCommand = new DelegatingCommand(OnExit, OnCanExit);
            CreateRCADDBCommand = new DelegatingCommand(OnCreateRCADDB, OnCanCreateRCADDB);
            PreviousCommand = new DelegatingCommand(OnPreviousStep, OnCanGoToPreviousStep);
            NextCommand = new DelegatingCommand(OnNextStep, OnCanGoToNextStep);
            TestDBConnectionCommand = new DelegatingCommand(OnTestDBConnection);
            RefreshDBListCommand = new DelegatingCommand(OnRefreshDBList, OnCanRefreshDBList);
            SelectScriptTargetFileCommand = new DelegatingCommand(OnSelectTargetScriptFile, OnCanSelectScriptTargetFile);
            _connection = new rCADConnection();
            _connection.Host = ".";
            _connection.Instance = string.Empty;
            _connection.Database = string.Empty;
            _connection.SecurityType = SecurityType.WindowsAuthentication;
            _step = 1;
            _localDatabases = new ObservableCollection<string>();
            _workerDTS = new BackgroundWorker();
            _workerDTS.DoWork += new DoWorkEventHandler(BackgroundCreateRCADDB_DTS);
            _workerDTS.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundCreateRCADDBCompleted);
            _workerSQLCMD = new BackgroundWorker();
            _workerSQLCMD.DoWork +=new DoWorkEventHandler(BackgroundCreateRCADDB_SQLCMD);
            _workerSQLCMD.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundCreateRCADDBCompleted);
            UsingSQLExpress = false;
            ScriptTargetFile = string.Empty;
            InitializeEnvironment();
        }

        public ICommand NextCommand { get; private set; }
        public ICommand PreviousCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand TestDBConnectionCommand { get; private set; }
        public ICommand RefreshDBListCommand { get; private set; }
        public ICommand CreateRCADDBCommand { get; private set; }
        public ICommand SelectScriptTargetFileCommand { get; private set; }

        private string _scriptTargetFile;
        private string _statusMessage;
        private bool _creatingDB;
        private rCADConnection _connection;
        private bool _connectionValid;
        private string _desiredRCADDBName;
        private int _step;
        private Server _desiredServer;
        private ObservableCollection<string> _localDatabases;
        private BackgroundWorker _workerDTS;
        private BackgroundWorker _workerSQLCMD;
        private string _installDirectory;
        private string _dataDirectory;
        private string _logDirectory;
        private string _rCADLoaderSSISPkg;
        private bool _usingSQLExpress;

        private static readonly string RCADLOADER_KEY = @"SOFTWARE\Gutell Lab\rCAD\rCADCreator";
        private static readonly string SSISPACKAGE_KEY = "SSISPackage";
        private static readonly string INSTALLDIR_KEY = "InstallDirectory";
        private static readonly string DATADIR_KEY = "DataDirectory";
        private static readonly string CREATING_RCAD_DB_MESSAGE = "Creating rCAD Database...";
        private static readonly string CREATING_RCAD_DB_INSTALLSCRIPT = "Creating rCAD Database and Install Script...";
        private static readonly string SUCCESSFULLY_CREATED_RCAD_DB_MESSAGE = "Successfully created rCAD Database.";
        private static readonly string SUCCESSFULLY_CEREATE_RCAD_DBANDSCRIPT_MESSAGE = "Successfully created rCAD Database, please execute the Install Script with sqlcmd.";
        private static readonly string FAILED_CREATING_RCAD_DB_MESSAGE = "Failed creating rCAD Database.";
        private static readonly string FAILED_CREATING_RCAD_DB_INTERNALERROR_MESSAGE = "Failed creating rCAD Database with internal error.";
        private static readonly string RCADCREATOR_XMLFILE = "rCADLoader.xml";
        private static readonly string SEQUENCE_METADATA_DIR = "Sequence.Metadata";
        private static readonly string STRUCTURAL_RELATIONSHIPS_DIR = "Structural.Relationships";
        private static readonly string EVOLUTIONARY_RELATIONSHIPS_DIR = "Evolutionary.Relationships";

        private void InitializeEnvironment()
        {
            try
            {
                RegistryKey rCADLoaderKey = Registry.LocalMachine.OpenSubKey(RCADLOADER_KEY);
                if (rCADLoaderKey != null)
                {
                    if (rCADLoaderKey.GetValue(INSTALLDIR_KEY, null) != null && rCADLoaderKey.GetValueKind(INSTALLDIR_KEY) == RegistryValueKind.String)
                    {
                        _installDirectory = rCADLoaderKey.GetValue(INSTALLDIR_KEY).ToString();
                    }
                    else
                    {
                        IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                        mesg.Show("Fatal",
                            string.Format("Unable to find Registry SubKey {0} in Key {1}, installation corrupted!", INSTALLDIR_KEY, RCADLOADER_KEY),
                            MessageButtons.OK);
                        App.Current.Shutdown();
                    }

                    if (rCADLoaderKey.GetValue(SSISPACKAGE_KEY, null) != null && rCADLoaderKey.GetValueKind(SSISPACKAGE_KEY) == RegistryValueKind.String)
                    {
                        _rCADLoaderSSISPkg = rCADLoaderKey.GetValue(SSISPACKAGE_KEY).ToString();
                    }
                    else
                    {
                        IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                        mesg.Show("Fatal",
                            string.Format("Unable to find Registry SubKey {0} in Key {1}, installation corrupted!", SSISPACKAGE_KEY, RCADLOADER_KEY),
                            MessageButtons.OK);
                        App.Current.Shutdown();
                    }

                    if (rCADLoaderKey.GetValue(DATADIR_KEY, null) != null && rCADLoaderKey.GetValueKind(DATADIR_KEY) == RegistryValueKind.String)
                    {
                        _dataDirectory = rCADLoaderKey.GetValue(DATADIR_KEY).ToString();
                    }
                    else
                    {
                        IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                        mesg.Show("Fatal",
                            string.Format("Unable to find Registry SubKey {0} in Key {1}, installation corrupted!", DATADIR_KEY, RCADLOADER_KEY),
                            MessageButtons.OK);
                        App.Current.Shutdown();
                    }
                }
                else 
                {
                    IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                    mesg.Show("Fatal",
                        string.Format("Unable to find Local Machine Registry Key {0}, installation corrupted!", RCADLOADER_KEY),
                        MessageButtons.OK);
                    App.Current.Shutdown();
                } //Error condition, failed to open TaxonomyUpdater registry key
            }
            catch 
            {
                IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                mesg.Show("Fatal",
                    string.Format("Unknown Error reading Application configuration from Registry, installation corrupted"),
                    MessageButtons.OK);
                App.Current.Shutdown();
            } //Error condition, failed to initialize directories
        }

        private bool OnCanGoToNextStep()
        {
            switch (_step)
            {
                case 1:
                    return ConnectionValid;
                default:
                    return false;
            };
        }

        private bool OnCanGoToPreviousStep()
        {
            return (_step > 1 && !CreatingDB) ? true : false;
        }

        private bool OnCanCreateRCADDB()
        {
            if (!UsingSQLExpress)
            {
                return (ConnectionValid && _desiredServer != null && !string.IsNullOrEmpty(Database) && !CreatingDB) ? true : false;
            }
            else
            {
                return (_desiredServer != null && !string.IsNullOrEmpty(Database) && !string.IsNullOrEmpty(ScriptTargetFile) && !CreatingDB) ? true : false;
            }
        }

        private bool OnCanRefreshDBList()
        {
            return (_step > 1 && ConnectionValid && _desiredServer != null && !CreatingDB) ? true : false;
        }

        private bool OnCanSelectScriptTargetFile()
        {
            return (_step > 1 && !CreatingDB) ? true : false;
        }

        private bool OnCanExit()
        {
            return (CreatingDB) ? false : true;
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
            if (CurrentStep < 2)
            {
                CurrentStep++;
            }
        }

        private void OnExit()
        {
            App.Current.Shutdown();
        }

        private void OnSelectTargetScriptFile()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                DefaultExt = ".sql",
                Multiselect = false,
                Title = "Select an Output File",
                CheckFileExists = false,
                CheckPathExists = true,
                AddExtension = true
            };
            if (dialog.ShowDialog().Value)
            {
                ScriptTargetFile = dialog.FileName;
            }
            else { ScriptTargetFile = null; }
        }

        private void OnRefreshDBList()
        {
            if (_desiredServer != null)
            {
                _desiredServer.Refresh();
                _localDatabases.Clear();
                foreach (Database db in _desiredServer.Databases)
                {
                    _localDatabases.Add(db.Name);
                }
            }
            else
            {
                IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                mesg.Show("Error",
                    string.Format("Error: Invalid or missing database connection, failed to update list."),
                    MessageButtons.OK);
            }
        }

        private void OnTestDBConnection()
        {
            if (!_connection.TestConnection())
            {
                IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                mesg.Show("Error",
                    string.Format("Error: Database connection test failed. Either database is not available or the configured connection is invalid."),
                    MessageButtons.OK);
                ConnectionValid = false;
                _desiredServer = null;
                _localDatabases.Clear();
            }
            else
            {
                ConnectionValid = true;
                ServerConnection conn = new ServerConnection();
                conn.ConnectionString = ConnectionString;
                _desiredServer = new Server(conn);
                foreach (Database db in _desiredServer.Databases)
                {
                    _localDatabases.Add(db.Name);
                }
            }
        }

        private void OnCreateRCADDB()
        {
            if(ConnectionValid && _desiredServer!=null && !string.IsNullOrEmpty(Database))
            {
                if (LocalDatabases.Contains(Database))
                {
                    IMessageVisualizer mesg = Resolve<IMessageVisualizer>();
                    mesg.Show("Error",
                        string.Format("Error: Database with name {0} exists on selected instance, please select another name or delete the existing database first.", Database),
                        MessageButtons.OK);
                    Database = null;
                    return;
                }

                CreatingDB = true;
                rCADConnection ssisOleConn = new rCADConnection();
                ssisOleConn.Database = Database;
                ssisOleConn.Instance = Instance;
                ssisOleConn.Host = Host;
                ssisOleConn.SecurityType = SecurityType.WindowsAuthentication;
                ssisOleConn.Provider = "SQLNCLI10.1";

                if (!UsingSQLExpress)
                {
                    CreateRCADDBArgs args = new CreateRCADDBArgs()
                    {
                        TargetInstance = _desiredServer,
                        RCADConnectionString = ssisOleConn.BuildConnectionString(),
                        InstallDirectory = _installDirectory,
                        DataDirectory = _dataDirectory,
                        LoadRCADDBPackage = _rCADLoaderSSISPkg,
                        RCADDatabaseName = Database
                    };
                    StatusMessage = CREATING_RCAD_DB_MESSAGE;
                    _workerDTS.RunWorkerAsync(args);
                }
                else
                {
                    CreateRCADDBArgs args = new CreateRCADDBArgs()
                    {
                        TargetInstance = _desiredServer,
                        RCADDatabaseName = Database,
                        TargetSQLFile = ScriptTargetFile,
                        InstallDirectory = _installDirectory,
                        DataDirectory = _dataDirectory
                    };
                    StatusMessage = CREATING_RCAD_DB_INSTALLSCRIPT;
                    _workerSQLCMD.RunWorkerAsync(args);
                }
            }
        }

        private void BackgroundCreateRCADDB_SQLCMD(object sender, DoWorkEventArgs args)
        {
            if (args.Argument == null) return;

            CreateRCADDBArgs createArgs = args.Argument as CreateRCADDBArgs;
            bool? retValue = null;
            if (createArgs != null)
            {
                if (createArgs.TargetInstance == null ||
                    createArgs.RCADDatabaseName == null ||
                    createArgs.InstallDirectory == null ||
                    createArgs.DataDirectory == null ||
                    createArgs.TargetSQLFile == null)
                {
                    args.Cancel = true;
                    return;
                }

                XDocument creatorXML = null;
                try
                {
                    //Create the new rCAD Database
                    Database rCADDB = new Database(createArgs.TargetInstance, createArgs.RCADDatabaseName);
                    rCADDB.Create();

                    using (var reader = XmlReader.Create(createArgs.InstallDirectory + RCADCREATOR_XMLFILE))
                    {
                        creatorXML = XDocument.Load(reader);
                        IEnumerable<string> createTablesFiles = from item in creatorXML.Descendants("TableFile")
                                                                select (createArgs.InstallDirectory + item.Value);
                        IEnumerable<string> createIndicesFiles = from item in creatorXML.Descendants("IndexFile")
                                                                 select (createArgs.InstallDirectory + item.Value);
                        IEnumerable<string> createViewsFiles = from item in creatorXML.Descendants("ViewFile")
                                                               select (createArgs.InstallDirectory + item.Value);
                        IEnumerable<string> createUDFFiles = from item in creatorXML.Descendants("UDFFile")
                                                             select (createArgs.InstallDirectory + item.Value);
                        IEnumerable<string> batchOpsFiles = from item in creatorXML.Descendants("QueryFile")
                                                            select (createArgs.InstallDirectory + item.Value);

                        //Build up rCAD script:
                        using (var targetScriptFile = File.CreateText(createArgs.TargetSQLFile))
                        {
                            targetScriptFile.WriteLine(@"USE {0};", createArgs.RCADDatabaseName);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            //1. Create rCAD Tables
                            targetScriptFile.WriteLine(@"-- Creating rCAD Tables");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            targetScriptFile.WriteLine(@"PRINT '*** Creating rCAD Tables';");
                            targetScriptFile.WriteLine(@"GO");

                            foreach (var createTableSQLFile in createTablesFiles)
                            {
                                using (var sql = File.OpenText(createTableSQLFile))
                                {
                                    string line = sql.ReadLine();
                                    while (!string.IsNullOrEmpty(line))
                                    {
                                        targetScriptFile.WriteLine(line);
                                        line = sql.ReadLine();
                                    }
                                }
                                targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            }

                            //2. Bulk Load Sequence Metadata Lookup Tables
                            targetScriptFile.WriteLine(@"-- Loading CellLocationInfo");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading CellLocationInfo';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT CellLocationInfo FROM N'{0}{1}\CellLocationInfo.dat'", createArgs.DataDirectory, SEQUENCE_METADATA_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.CellLocationInfo.xml');", createArgs.DataDirectory, SEQUENCE_METADATA_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            targetScriptFile.WriteLine(@"-- Loading SequenceType");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading SequenceType';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT SequenceType FROM N'{0}{1}\SequenceType.dat'", createArgs.DataDirectory, SEQUENCE_METADATA_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.SequenceType.xml');", createArgs.DataDirectory, SEQUENCE_METADATA_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            targetScriptFile.WriteLine(@"-- Loading SecondaryStructureExtentTypes");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading SecondaryStructureExtentTypes';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT SecondaryStructureExtentTypes FROM N'{0}{1}\SecondaryStructureExtentTypes.dat'", createArgs.DataDirectory, STRUCTURAL_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.SecondaryStructureExtentTypes.xml');", createArgs.DataDirectory, STRUCTURAL_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            //3. Bulk Load Evolutionary Relationships Tables
                            targetScriptFile.WriteLine(@"-- Loading Taxonomy");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading Taxonomy';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT Taxonomy FROM N'{0}{1}\Taxonomy.dat'", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.Taxonomy.xml');", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            targetScriptFile.WriteLine(@"UPDATE Taxonomy SET ParentTaxID = 0 WHERE TaxID = 1;");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            targetScriptFile.WriteLine(@"-- Loading NameClasses");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading NameClasses';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT NameClasses FROM N'{0}{1}\NameClasses.dat'", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.NameClasses.xml');", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            targetScriptFile.WriteLine(@"-- Loading TaxonomyNames");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading TaxonomyNames';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT TaxonomyNames FROM N'{0}{1}\TaxonomyNames.dat'", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.TaxonomyNames.xml');", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            targetScriptFile.WriteLine(@"-- Loading AlternateNames");
                            targetScriptFile.WriteLine(@"PRINT '*** Loading AlternateNames';");
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(@"BULK INSERT AlternateNames FROM N'{0}{1}\AlternateNames.dat'", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FORMATFILE=N'{0}{1}\Bulkload.AlternateNames.xml');", createArgs.DataDirectory, EVOLUTIONARY_RELATIONSHIPS_DIR);
                            targetScriptFile.WriteLine(@"GO");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);

                            //4. Create rCAD Indices
                            targetScriptFile.WriteLine(@"-- Creating rCAD Indices");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            targetScriptFile.WriteLine(@"PRINT '*** Creating rCAD Indices';");
                            targetScriptFile.WriteLine(@"GO");

                            foreach (var createIndexSQLFile in createIndicesFiles)
                            {
                                using (var sql = File.OpenText(createIndexSQLFile))
                                {
                                    string line = sql.ReadLine();
                                    while (!string.IsNullOrEmpty(line))
                                    {
                                        targetScriptFile.WriteLine(line);
                                        line = sql.ReadLine();
                                    }
                                }
                                targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            }

                            //5. Create rCAD Views
                            targetScriptFile.WriteLine(@"-- Creating rCAD Views");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            targetScriptFile.WriteLine(@"PRINT '*** Creating rCAD Views';");
                            targetScriptFile.WriteLine(@"GO");
                            foreach (var createViewSQLFile in createViewsFiles)
                            {
                                using (var sql = File.OpenText(createViewSQLFile))
                                {
                                    string line = sql.ReadLine();
                                    while (!string.IsNullOrEmpty(line))
                                    {
                                        targetScriptFile.WriteLine(line);
                                        line = sql.ReadLine();
                                    }
                                }
                                targetScriptFile.WriteLine(@"GO");
                                targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            }

                            //6. Create rCAD UDFs
                            targetScriptFile.WriteLine(@"-- Creating rCAD UDFs");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            targetScriptFile.WriteLine(@"PRINT '*** Creating rCAD UDFs';");
                            targetScriptFile.WriteLine(@"GO");
                            foreach (var createUDFSQLFile in createUDFFiles)
                            {
                                using (var sql = File.OpenText(createUDFSQLFile))
                                {
                                    string line = sql.ReadLine();
                                    while (!string.IsNullOrEmpty(line))
                                    {
                                        targetScriptFile.WriteLine(line);
                                        line = sql.ReadLine();
                                    }
                                }
                                targetScriptFile.WriteLine(@"GO");
                                targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            }

                            //7. BatchOps
                            targetScriptFile.WriteLine(@"-- Batch Operations");
                            targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            targetScriptFile.WriteLine(@"PRINT '*** Executing Batch Operations';");
                            targetScriptFile.WriteLine(@"GO");
                            foreach (var batchOpSQLFile in batchOpsFiles)
                            {
                                using (var sql = File.OpenText(batchOpSQLFile))
                                {
                                    string line = sql.ReadLine();
                                    while (!string.IsNullOrEmpty(line))
                                    {
                                        targetScriptFile.WriteLine(line);
                                        line = sql.ReadLine();
                                    }
                                }
                                targetScriptFile.WriteLine(targetScriptFile.NewLine);
                            }

                            targetScriptFile.Flush();
                        }
                    }
                    retValue = true;                
                }
                catch
                {
                    retValue = false;
                }
                
                args.Result = retValue;
            }
        }

        private void BackgroundCreateRCADDB_DTS(object sender, DoWorkEventArgs args)
        {
            if (args.Argument == null) return;

            CreateRCADDBArgs createArgs = args.Argument as CreateRCADDBArgs;
            bool? retValue = null;
            if (createArgs != null)
            {
                if (createArgs.TargetInstance == null ||
                    createArgs.RCADDatabaseName == null ||
                    createArgs.LoadRCADDBPackage == null ||
                    createArgs.InstallDirectory == null ||
                    createArgs.RCADConnectionString == null ||
                    createArgs.DataDirectory == null)
                {
                    args.Cancel = true;
                    return;
                }

                Database rCADDB = null;

                try
                {
                    //Create the new rCAD Database
                    rCADDB = new Database(createArgs.TargetInstance, createArgs.RCADDatabaseName);
                    rCADDB.Create();

                    //Run the SSIS Package
                    Application ssisRCADLoaderApp = new Application();
                    SSISEventListener eventListener = new SSISEventListener();
                    Package ssisRCADLoader = ssisRCADLoaderApp.LoadPackage(createArgs.LoadRCADDBPackage, eventListener);
                    ssisRCADLoader.Variables["rCADInstallDirectory"].Value = createArgs.InstallDirectory;
                    ssisRCADLoader.Variables["rCADConnectionString"].Value = createArgs.RCADConnectionString;
                    ssisRCADLoader.Variables["rCADDataDirectory"].Value = createArgs.DataDirectory;
                    DTSExecResult result = ssisRCADLoader.Execute(null, null, eventListener, null, null);
                    if (result == DTSExecResult.Success)
                    {
                        retValue = true;
                    }
                    else
                    {
                        using (StreamWriter output = new StreamWriter(new FileStream(Path.GetTempPath() + "rCADCreator.ssis_errors.out", FileMode.Create)))
                        {
                            output.Write(eventListener.ErrorLog);
                            output.Flush();
                        }
                        retValue = false;
                        //rCADDB.Drop();
                    }
                }
                catch
                {
                    retValue = false;
                    //rCADDB.Drop();
                }
                args.Result = retValue;
            }
        }

        private void BackgroundCreateRCADDBCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            CreatingDB = false;
            if (args.Cancelled)
            {
                StatusMessage = FAILED_CREATING_RCAD_DB_INTERNALERROR_MESSAGE;
                Database = null;
                if (UsingSQLExpress) ScriptTargetFile = null;
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            bool? createResult = args.Result as bool?;
            if (createResult != null)
            {
                if (createResult.Value)
                {
                    _desiredServer.Refresh();
                    LocalDatabases.Clear();
                    foreach (Database db in _desiredServer.Databases)
                    {
                        _localDatabases.Add(db.Name);
                    }
                    if (UsingSQLExpress)
                        StatusMessage = SUCCESSFULLY_CEREATE_RCAD_DBANDSCRIPT_MESSAGE;
                    else
                        StatusMessage = SUCCESSFULLY_CREATED_RCAD_DB_MESSAGE;
                    Database = null;
                    if (UsingSQLExpress) ScriptTargetFile = null;
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }
            }

            StatusMessage = FAILED_CREATING_RCAD_DB_MESSAGE;
            Database = null;
            if (UsingSQLExpress) ScriptTargetFile = null;
            CommandManager.InvalidateRequerySuggested();
            return;
        }
    }

    internal class CreateRCADDBArgs
    {
        public Server TargetInstance { get; set; }
        public string RCADDatabaseName { get; set; }
        public string LoadRCADDBPackage { get; set; }
        public string InstallDirectory { get; set; }
        public string DataDirectory { get; set; }
        public string RCADConnectionString { get; set; }
        public string TargetSQLFile { get; set; }
    }

    internal class SSISEventListener : DefaultEvents
    {
        internal SSISEventListener()
        {
            _errorStream = new StringWriter();
        }

        public String ErrorLog
        {
            get { return _errorStream.ToString(); }
        }

        public override bool OnError(DtsObject source, int errorCode, string subComponent, string description, string helpFile, int helpContext, string idofInterfaceWithError)
        {
            _errorStream.WriteLine("SSIS Error in {0}/{1} : {2}", source, subComponent, description);
            return false;
        }

        private StringWriter _errorStream;
    }
}
