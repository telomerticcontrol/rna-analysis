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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bio.Data.Providers;
using JulMar.Windows;

namespace BioBrowser.Models
{
    /// <summary>
    /// Represents a single file used in a workspace
    /// </summary>
    public class WorkspaceEntry
    {
        public string LoaderData { get; set; }
        public BioFormatType FormatType { get; set; }
        public string LoaderKey { get; set; }
    }

    /// <summary>
    /// Represents a workspace entry
    /// </summary>
    public class Workspace
    {
        public const string WS_EXTENSION = ".biows";

        public string Filename { get; set; }
        public string Name { get; set; }
        public MTObservableCollection<WorkspaceEntry> DataSources { get; private set; }

        public Workspace()
        {
            DataSources = new MTObservableCollection<WorkspaceEntry>();
        }

        /// <summary>
        /// Loads workspace entries from an XML file.
        /// </summary>
        /// <param name="filename">File to load workspace entries from </param>
        /// <returns>List of Workspace entries loaded</returns>
        public static Workspace Load(string filename)
        {
            try
            {
                var doc = XElement.Load(filename);
                var ws = new Workspace
                      {
                          Filename = filename,
                          Name = doc.Attribute("name").Value,
                          DataSources = new MTObservableCollection<WorkspaceEntry>(
                              from file in doc.Element("entries").Elements("entry")
                              select new WorkspaceEntry
                                 {
                                     LoaderData = file.Attribute("loaderData").Value,
                                     LoaderKey = file.Attribute("loaderKey").Value,
                                     FormatType = (BioFormatType) Enum.Parse(typeof(BioFormatType), file.Attribute("type").Value)
                                 })
                      };

                return ws;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Writes the workspace out to a persistent file.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            Debug.Assert(!string.IsNullOrEmpty(Filename));
            if (string.Compare(Path.GetExtension(Filename), WS_EXTENSION) != 0)
                Filename += WS_EXTENSION;

            try
            {
                var doc = new XElement("workspace",
                                          new XAttribute("name", Name),
                                          new XElement("entries",
                                              from file in DataSources
                                              select new XElement("entry",
                                                  new XAttribute("loaderData", file.LoaderData),
                                                  new XAttribute("loaderKey", file.LoaderKey),
                                                  new XAttribute("type", file.FormatType)
                                              )
                                          )
                                      );
                doc.Save(Filename);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}
