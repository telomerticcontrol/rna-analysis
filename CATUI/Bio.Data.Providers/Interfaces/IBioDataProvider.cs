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

namespace Bio.Data.Providers.Interfaces
{
    /// <summary>
    /// This interface is our creation factory for a BioDataLoader
    /// object.  Each unique loader should be created through this mechanism.
    /// </summary>
    public interface IBioDataProvider
    {
        /// <summary>
        /// Unique key identifying this type.  Allows client application to store
        /// settings about this loader (i.e. db connection, etc.)
        /// </summary>
        string Key { get; }

        /// <summary>
        /// A textual description (short) of the type.  This is used to prompt
        /// the user when no file filter is supplied.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Image used to represent this provider in a UI (resource in PackURI format)
        /// </summary>
        string ImageUrl { get;  }

        /// <summary>
        /// Types of formats expected
        /// </summary>
        BioFormatType SupportedTypes { get; }

        /// <summary>
        /// Filters (for file-based formats).  This is a "|" separated list.
        /// </summary>
        /// <example>
        /// 
        /// "All Files (*.*)|*.*"
        /// 
        /// </example>
        string FileFilter { get; }

        /// <summary>
        /// This is used to create an instance of the given BioLoader
        /// </summary>
        /// <param name="initData"></param>
        /// <returns></returns>
        IBioDataLoader Create(string initData);
    }
}
