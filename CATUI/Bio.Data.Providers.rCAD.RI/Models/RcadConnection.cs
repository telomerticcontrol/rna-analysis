using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using JulMar.Windows.Interfaces;
using JulMar.Windows.Mvvm;

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

namespace Bio.Data.Providers.rCAD.RI.Models
{
    /// <summary>
    /// This class represents a single connection to a SQL Server 2005/2008 server.
    /// </summary>
    public class RcadConnection
    {
        private readonly List<string> _dbNames = new List<string>();
        private string _server;
        private SecurityType _securityType;
        private string _username;
        private string _password;

        /// <summary>
        /// The server name (.\SQLEXPRESS, (local), etc.)
        /// </summary>
        public string Server
        {
            get { return _server; }
            set 
            { 
                _server = value;
                _dbNames.Clear(); 
            }
        }

        /// <summary>
        /// The database name on above server
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Database provider (SQL Server)
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Type of security to login with
        /// </summary>
        public SecurityType SecurityType
        {
            get { return _securityType; }
            set 
            { 
                _securityType = value;
                if (_securityType == SecurityType.WindowsAuthentication)
                    _username = _password = null;
                _dbNames.Clear();
            }
        }

        /// <summary>
        /// User name (if SecurityType = SQL)
        /// </summary>
        public string Username
        {
            get { return _username ?? string.Empty; }
            set 
            {
                _username = value;
                _dbNames.Clear(); 
            }
        }

        /// <summary>
        /// Password for SecurityType = SQL
        /// </summary>
        public string Password
        {
            get { return _password ?? string.Empty; }
            set 
            { 
                _password = value;
                _dbNames.Clear(); 
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RcadConnection()
        {
            // Only support SQL server in this initial release.
            Provider = "System.Data.SqlClient";
            Server = @"(local)";
            SecurityType = SecurityType.WindowsAuthentication;
        }

        /// <summary>
        /// This builds a connection string to hit this connection
        /// </summary>
        /// <returns></returns>
        public string BuildConnectionString()
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

            if (!string.IsNullOrEmpty(Server))
                csb.DataSource = Server;
            if (!string.IsNullOrEmpty(Database))
                csb.InitialCatalog = Database;
            csb.IntegratedSecurity = (SecurityType == SecurityType.WindowsAuthentication);

            if (csb.IntegratedSecurity == false && !string.IsNullOrEmpty(Username))
            {
                csb.UserID = Username;
                if (!string.IsNullOrEmpty(Password))
                    csb.Password = Password;
            }

            // Turn on MARS
            csb.Add("MultipleActiveResultSets", true);

            return csb.ConnectionString;
        }

        /// <summary>
        /// Returns whether this connection is "valid"
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Server) ||
                    (SecurityType==SecurityType.SqlAuthentication && string.IsNullOrEmpty(Username)))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Parses a connection string
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        public static RcadConnection Parse(string connString)
        {
            SqlConnectionStringBuilder scb = new SqlConnectionStringBuilder(connString);

            return new RcadConnection
               {
                   Server = scb.DataSource,
                   Database = scb.InitialCatalog,
                   SecurityType = (scb.IntegratedSecurity) ? SecurityType.WindowsAuthentication : SecurityType.SqlAuthentication,
                   Username = (!scb.IntegratedSecurity) ? scb.UserID : string.Empty,
                   Password = (!scb.IntegratedSecurity) ? scb.Password : string.Empty,
               };
        }

        /// <summary>
        /// This retrieves a list of databases for this connection
        /// </summary>
        /// <returns></returns>
        public List<string> GetDatabaseList(bool showError)
        {
            if (_dbNames.Count == 0 && IsValid)
            {
                DbProviderFactory dbFac = DbProviderFactories.GetFactory(Provider);
                using (DbConnection dbConn = dbFac.CreateConnection())
                {
                    dbConn.ConnectionString = BuildConnectionString();

                    DbCommand cmd = dbConn.CreateCommand();
                    cmd.CommandText = "sp_databases";
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {

                        dbConn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                _dbNames.Add(reader[0].ToString());
                        }
                    }
                    catch (DbException ex)
                    {
                        if (showError == true)
                        {
                            IErrorVisualizer errorVisualizer = ViewModel.ServiceProvider.Resolve<IErrorVisualizer>();
                            if (errorVisualizer != null)
                                errorVisualizer.Show("Error connecting to database", ex.Message);
                        }
                    }
                }
            }

            return _dbNames;
        }
   }
}