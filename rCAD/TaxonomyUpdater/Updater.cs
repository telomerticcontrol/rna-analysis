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
using Utilities.Data;
using Microsoft.SqlServer.Dts.Runtime;
using System.IO;
using Microsoft.Win32;

namespace TaxonomyUpdater
{
    /// <summary>
    /// Currently Updater configuration is handled through enviroment variables, but this can
    /// easily be moved to the registry. In the future, may want to open the variables up to
    /// external configuration.
    /// </summary>
    public class Updater
    {
        public Updater() { }

        public bool Run(string connectionString, string databaseName)
        {
            if (LoadEnvironment() && connectionString!=null && databaseName!=null)
            {
                _connectionString = connectionString;
                _databaseName = databaseName;
                if (UpdateTaxonomy())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false; //App configuration is incorrect
            }

        }

        #region Private Methods and Properties

        private readonly static string TAXONOMYUPDATER_KEY = @"SOFTWARE\Gutell Lab\rCAD\TaxonomyUpdater";
        private readonly static string DATADIR_KEY = @"DataDirectory";
        private readonly static string INSTALLDIR_KEY = @"InstallDirectory";
        private readonly static string SSISPACKAGE_KEY = "SSISPackage";
        private string _dataDirectory;
        private string _installedDirectory;
        private string _ssisPackage;
        private string _connectionString;
        private string _databaseName;

        private bool LoadEnvironment()
        {
            try
            {
                RegistryKey taxonomyUpdaterKey = Registry.LocalMachine.OpenSubKey(TAXONOMYUPDATER_KEY);
                if (taxonomyUpdaterKey != null)
                {
                    if (taxonomyUpdaterKey.GetValue(DATADIR_KEY, null) != null && taxonomyUpdaterKey.GetValueKind(DATADIR_KEY) == RegistryValueKind.String)
                    {
                        _dataDirectory = taxonomyUpdaterKey.GetValue(DATADIR_KEY).ToString();
                    }

                    if (taxonomyUpdaterKey.GetValue(INSTALLDIR_KEY, null) != null && taxonomyUpdaterKey.GetValueKind(INSTALLDIR_KEY) == RegistryValueKind.String)
                    {
                        _installedDirectory = taxonomyUpdaterKey.GetValue(INSTALLDIR_KEY).ToString();
                    }

                    if (taxonomyUpdaterKey.GetValue(SSISPACKAGE_KEY, null) != null && taxonomyUpdaterKey.GetValueKind(SSISPACKAGE_KEY) == RegistryValueKind.String)
                    {
                        _ssisPackage = taxonomyUpdaterKey.GetValue(SSISPACKAGE_KEY).ToString();     
                    }
                   
                    return true;
                }
                else { return false; } //Error condition, failed to open TaxonomyUpdater registry key
            }
            catch { return false;  } //Error condition, failed to initialize directories
        }

        private bool UpdateTaxonomy()
        {
            if (_ssisPackage == null)
                return false;
            
            Application ssisUpdateApp = new Application();
            SSISEventListener eventListener = new SSISEventListener();

            Package taxonomyUpdaterPkg = null;
            try
            {
                taxonomyUpdaterPkg = ssisUpdateApp.LoadPackage(_ssisPackage, eventListener);
                taxonomyUpdaterPkg.Variables["InstallDirectory"].Value = _installedDirectory;
                taxonomyUpdaterPkg.Variables["TempDirectory"].Value = _dataDirectory;
                taxonomyUpdaterPkg.Variables["rCADConnectionString"].Value = _connectionString;
                taxonomyUpdaterPkg.Variables["DatabaseName"].Value = _databaseName;
                
                DTSExecResult result = taxonomyUpdaterPkg.Execute(null, null, eventListener, null, null);
                if (result == DTSExecResult.Success)
                {
                    return true;
                }
                else
                {
                    using (StreamWriter output = new StreamWriter(new FileStream(Path.GetTempPath() + "TaxonomyUpdater.ssis_errors.out", FileMode.Create)))
                    {
                        output.Write(eventListener.ErrorLog);
                        output.Flush();
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion
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
