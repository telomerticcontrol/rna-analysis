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
using System.Data.SqlClient;
using System.Data.OleDb;

namespace Utilities.Data
{
    public enum SecurityType
    {
        WindowsAuthentication,
        SQLServerAuthentication
    };

    public class rCADConnection
    {
        public rCADConnection()
        {
            Host = ".";
            SecurityType = SecurityType.WindowsAuthentication;
        }

        public string Provider { get; set; }
        public string Database { get; set; }
        public string Host { get; set; }
        public string Instance { get; set; }
        public bool IsValid 
        {
            get 
            { 
                if(string.IsNullOrEmpty(Host) ||
                   string.IsNullOrEmpty(Database) ||
                    (((SecurityType==SecurityType.SQLServerAuthentication) && string.IsNullOrEmpty(Username))))
                {
                    return false;
                }
                return true;
            } 
        }

        private SecurityType _securityType;
        public SecurityType SecurityType
        {
            get { return _securityType; }
            set
            {
                _securityType = value;
                if (_securityType == SecurityType.WindowsAuthentication)
                {
                    _username = null;
                    _password = null;
                }
            }

        }

        private string _username;
        public string Username
        {
            get { return _username ?? string.Empty; }
            set { _username = value; }
        }

        private string _password;
        public string Password
        {
            get { return _password ?? string.Empty; }
            set { _password = value; }
        }

        public string BuildConnectionString()
        {
            StringBuilder cs = new StringBuilder();

            if (!string.IsNullOrEmpty(Host))
                cs.Append(string.Format("Data Source={0}", Host));
            if (!string.IsNullOrEmpty(Instance))
                cs.Append(string.Format("\\{0}", Instance));

            if (!string.IsNullOrEmpty(Database))
            {
                cs.Append(string.Format(";Initial Catalog={0}", Database));
            }

            if (SecurityType == SecurityType.WindowsAuthentication)
            {
                cs.Append(";Integrated Security=SSPI");
            }
            else //Using SQL Server authentication
            {
                if (!string.IsNullOrEmpty(Username))
                {
                    cs.Append(string.Format(";User ID={0}", Username));
                    if (!string.IsNullOrEmpty(Password))
                        cs.Append(string.Format(";Password={0}", Password));
                }
            }

            if (!string.IsNullOrEmpty(Provider))
            {
                cs.Append(string.Format(";Provider={0}", Provider));
            }

            return cs.ToString();
        }

        public bool TestConnection()
        {
            if (string.IsNullOrEmpty(Provider))
            {
                SqlConnection conn = new SqlConnection(BuildConnectionString());
                try
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                OleDbConnection conn = new OleDbConnection(BuildConnectionString());
                try
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
