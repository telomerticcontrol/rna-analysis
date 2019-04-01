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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Interfaces;

namespace Bio.Data.Providers.PDB
{
    /// <summary>
    /// This class provides a simple wrapper loader for Protein database format files (PDBs).
    /// </summary>
    class PdbFile : IBioDataLoader<IStreamBioEntity>, IStreamBioEntity
    {
        /// <summary>
        /// This is used to initialize the BioDataLoader when it is first created.
        /// </summary>
        /// <param name="initData">String data</param>
        /// <returns>True/False success</returns>
        public bool Initialize(string initData)
        {
            Source = initData;
            return true;
        }

        /// <summary>
        /// This provides access to any initialization data used to create this loader.
        /// </summary>
        public string InitializationData
        {
            get { return Path.GetFileName(Source); }
        }

        /// <summary>
        /// This method is used to prepare the loader to access the
        /// Entities collection.
        /// </summary>
        /// <returns>Count of loaded records</returns>
        public int Load()
        {
            return 1;
        }

        /// <summary>
        /// The list of entities
        /// </summary>
        IList IBioDataLoader.Entities
        {
            get
            {
                return new ArrayList {this};
            }
        }

        /// <summary>
        /// Ths list of typesafe entities.
        /// </summary>
        public IList<IStreamBioEntity> Entities
        {
            get
            {
                return new List<IStreamBioEntity> {this};
            }
        }

        /// <summary>
        /// Source (filename) of the PDB information
        /// </summary>
        public string Source
        {
            get; private set;
        }

        /// <summary>
        /// Stream used to access - this is required by the interface but not used for this file format.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            throw new NotImplementedException();
        }
    }
}
