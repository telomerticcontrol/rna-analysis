using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
    /// Represents an alignment filter to pull a set of sequences
    /// </summary>
    public class AlignmentFilter
    {
        /// <summary>
        /// User friendly name for the filter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Root taxononmy ID used for filter.  Only sequences under this ID will be retrieved.
        /// </summary>
        public int ParentTaxId { get; set; }

        /// <summary>
        /// RNA location identifier
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Sequence type (5s, 16s, 23s)
        /// </summary>
        public int SequenceTypeId { get; set; }

        /// <summary>
        /// Alignment id
        /// </summary>
        public int AlignmentId { get; set; }

        /// <summary>
        /// Database connection information
        /// </summary>
        public RcadConnection Connection { get; set; }

        /// <summary>
        /// Returns whether this filter + DB connection is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Connection != null && Connection.IsValid && SequenceTypeId > 0 && AlignmentId > 0 &&
                       !string.IsNullOrEmpty(Name) && ParentTaxId > 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AlignmentFilter()
        {
            Connection = new RcadConnection();            
        }

        /// <summary>
        /// This loads the connections from a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static List<AlignmentFilter> Load(string filename)
        {
            try
            {
                XDocument doc = XDocument.Load(filename);
                return (from c in doc.Root.Elements("filter")
                        let dbConn = c.Element("connection")
                        let user = dbConn.Attribute("user")
                        let pw = dbConn.Attribute("pw")
                        select new AlignmentFilter
                        {
                            Name = c.Attribute("name").Value,
                            LocationId = Int32.Parse(c.Attribute("locationid").Value),
                            ParentTaxId = Int32.Parse(c.Attribute("taxid").Value),
                            SequenceTypeId = Int32.Parse(c.Attribute("seqtypeid").Value),
                            AlignmentId = Int32.Parse(c.Attribute("alignmentid").Value),
                            Connection = new RcadConnection
                                             {
                                                 Server = dbConn.Attribute("server").Value,
                                                 Database = dbConn.Attribute("database").Value,
                                                 Provider = dbConn.Attribute("provider").Value,
                                                 Username = (user != null) ? user.Value : "",
                                                 Password = (pw != null) ? pw.Value : "",
                                                 SecurityType = (SecurityType)
                                                     Enum.Parse(typeof(SecurityType), dbConn.Attribute("security").Value)
                                             },

                        }).ToList();
            }
            catch (Exception)
            {
                return new List<AlignmentFilter>();
            }
        }

        /// <summary>
        /// This method saves our connection list to a file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="list"></param>
        public static void Save(string filename, IEnumerable<AlignmentFilter> list)
        {
            XDocument doc = new XDocument(
                new XElement("savedFilters",
                             from c in list
                             select new XElement("filter",
                                         new XAttribute("name", c.Name),
                                         new XAttribute("locationid", c.LocationId),
                                         new XAttribute("taxid", c.ParentTaxId),
                                         new XAttribute("seqtypeid", c.SequenceTypeId),
                                         new XAttribute("alignmentid", c.AlignmentId),
                                         new XElement("connection",
                                             new XAttribute("server", c.Connection.Server),
                                             new XAttribute("database", c.Connection.Database),
                                             new XAttribute("provider", c.Connection.Provider),
                                             new XAttribute("user", c.Connection.Username),
                                             new XAttribute("pw", c.Connection.Password),
                                             new XAttribute("security", c.Connection.SecurityType)
                                        ))));
            doc.Save(filename);
        }
 
    }
}
