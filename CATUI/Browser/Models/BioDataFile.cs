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
using Bio.Data.Providers;
using Bio.Data.Providers.Interfaces;
using System.Diagnostics;

namespace BioBrowser.Models
{
    /// <summary>
    /// This represents a single data file source of bio data
    /// </summary>
    public class BioDataFile
    {
        /// <summary>
        /// Bio format type
        /// </summary>
        public BioFormatType FormatType { get; private set; }
        /// <summary>
        /// Data (filename if file based)
        /// </summary>
        public string LoadData { get; private set; }
        /// <summary>
        /// Loader to get data
        /// </summary>
        public IBioDataLoader Loader { get; private set; }
        /// <summary>
        /// The Key for the BioLoader
        /// </summary>
        public string LoaderKey { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loadData"></param>
        /// <param name="format"></param>
        /// <param name="loader"></param>
        /// <param name="key"></param>
        public BioDataFile(string loadData, BioFormatType format, IBioDataLoader loader, string key)
        {
            LoadData = loadData;
            FormatType = format;
            Loader = loader;
            LoaderKey = key;
        }

        /// <summary>
        /// Loads the BIO data using an async call.
        /// </summary>
        /// <param name="completedFunc"></param>
        public void LoadAsync(Action<bool> completedFunc)
        {
            Debug.Assert(Loader != null);

            // Load the data
            Func<int> loadAction = Loader.Load;
            loadAction.BeginInvoke(iar => completedFunc(loadAction.EndInvoke(iar) > 0), null);
        }
    }
}
