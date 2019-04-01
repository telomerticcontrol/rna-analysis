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
using System.ComponentModel;
using TaxonomyUpdater;
using System.Collections.ObjectModel;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TaxonomyUpdaterDialog.ViewModels
{
    public class TaxonomyUpdaterViewModel : ViewModel
    {
        public string Icon
        {
            get { return "Images/alignmentDB.ico"; }
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
                InstanceDatabases.Clear();
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
                InstanceDatabases.Clear();
                OnPropertiesChanged("Instance", "ConnectionString");
            }
        }

        public string Database
        {
            get { return _connection.Database; }
            set
            {
                _connection.Database = value;
                OnPropertiesChanged("Database", "ConnectionString");
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

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged("StatusMessage"); }
        }

        public bool UpdatingDB
        {
            get { return _updatingDB; }
            private set { _updatingDB = value; OnPropertyChanged("UpdatingDB"); }
        }

        public TaxonomyUpdaterViewModel()
        {
            ExitCommand = new DelegatingCommand(OnExit, OnCanExit);
            TestDBConnectionCommand = new DelegatingCommand(OnTestDBConnection);
            UpdateTaxonomyCommand = new DelegatingCommand(OnUpdateTaxonomy, OnCanUpdateTaxonomy);
            _updatingDB = false;
            _instanceDatabases = new ObservableCollection<string>();
            _connection = new rCADConnection();
            _connection.Host = ".";
            _connection.Instance = string.Empty;
            _connection.Database = string.Empty;
            _connection.SecurityType = SecurityType.WindowsAuthentication;
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(BackgroundUpdateTaxonomy);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundUpdateTaxonomyCompleted);
        }

        public ICommand UpdateTaxonomyCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand TestDBConnectionCommand { get; private set; }

        private ObservableCollection<string> _instanceDatabases;
        private bool _updatingDB;
        private rCADConnection _connection;
        private bool _connectionValid;
        private string _statusMessage;
        private BackgroundWorker _worker;

        private readonly string UPDATING_TAXONOMY = "Updating Taxonomy...";
        private readonly string FAILED_UPDATING_TAXONOMY = "Failed Updating Taxonomy...";
        private readonly string FAILED_CONNECTING_FORUPDATE = "Failed Connection with Database";
        private readonly string FINISHED_UPDATING_TAXONOMY = "Finished Updating Taxonomy...";

        private bool OnCanExit()
        {
            return (UpdatingDB) ? false : true;
        }

        private bool OnCanUpdateTaxonomy()
        {
            return (!UpdatingDB && ConnectionValid && !string.IsNullOrEmpty(Database)) ? true : false;
        }

        private void OnExit()
        {
            App.Current.Shutdown();
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
            }
            else
            {
                ConnectionValid = true;
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

        private void OnUpdateTaxonomy()
        {
            UpdatingDB = true;
            StatusMessage = UPDATING_TAXONOMY;

            rCADConnection oleConn = new rCADConnection();
            oleConn.Instance = Instance;
            oleConn.Host = Host;
            oleConn.SecurityType = SecurityType.WindowsAuthentication;
            oleConn.Database = Database;
            oleConn.Provider = "SQLNCLI10.1";

            TaxonomyLoaderArgs args = new TaxonomyLoaderArgs { ConnectionString = oleConn.BuildConnectionString(), DatabaseName = Database };
            _worker.RunWorkerAsync(args);
        }

        private void BackgroundUpdateTaxonomy(object sender, DoWorkEventArgs args)
        {
            if (args.Argument == null) return;
            TaxonomyLoaderArgs loaderArgs = args.Argument as TaxonomyLoaderArgs;
            bool? retValue = null;
            if (loaderArgs != null)
            {
                if (loaderArgs.ConnectionString == null || loaderArgs.DatabaseName == null)
                {
                    args.Cancel = true;
                    return;
                }

                Updater taxonomyUpdater = new Updater();
                if (taxonomyUpdater.Run(loaderArgs.ConnectionString, loaderArgs.DatabaseName))
                {
                    retValue = true;
                }
                else
                {
                    retValue = false;
                }
                args.Result = retValue;
            }
        }

        private void BackgroundUpdateTaxonomyCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                StatusMessage = FAILED_CONNECTING_FORUPDATE;
                UpdatingDB = false;
                return;
            }

            bool? updateResult = args.Result as bool?;
            UpdatingDB = false;
            if (updateResult != null)
            {
                if (updateResult.Value)
                {
                    StatusMessage = FINISHED_UPDATING_TAXONOMY;
                    return;
                }
            }

            StatusMessage = FAILED_UPDATING_TAXONOMY;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    internal class TaxonomyLoaderArgs
    {
        //Only have to set connection string now, other settings come from enviroment.
        //Eventually, we will make this configurable through the app.
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
