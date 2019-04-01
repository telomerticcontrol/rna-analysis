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

using System.Collections;
using System.Collections.Generic;

namespace Bio.Data.Providers.Interfaces
{
    /// <summary>
    /// Base DataLoader interface
    /// </summary>
    public interface IBioDataLoader
    {
        /// <summary>
        /// This is used to initialize the BioDataLoader when it is first created.
        /// </summary>
        /// <param name="initData">String data</param>
        /// <returns>True/False success</returns>
        bool Initialize(string initData);

        /// <summary>
        /// This provides access to any initialization data used to create this loader.
        /// </summary>
        string InitializationData { get; }

        /// <summary>
        /// This method is used to prepare the loader to access the
        /// Entities collection.
        /// </summary>
        /// <returns>Count of loaded records</returns>
        int Load();

        /// <summary>
        /// The list of entities
        /// </summary>
        IList Entities { get; }
    }

    /// <summary>
    /// This interface represents a storage device
    /// </summary>
    /// <typeparam name="T">sequence type being loaded</typeparam>
    public interface IBioDataLoader<T> : IBioDataLoader
    {
        /// <summary>
        /// Type-safe version of entity list
        /// </summary>
        new IList<T> Entities { get; }
    }
}