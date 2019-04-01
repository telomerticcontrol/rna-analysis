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

namespace Bio.Data.Interfaces
{
    /// <summary>
    /// This interface is used to provide a named indexer
    /// </summary>
    public interface IDictionaryIndexer<TKey,TValue>
    {
        TValue this[TKey key] { get; set; }
    }

    /// <summary>
    /// This interface is used to extend BioData objects with dynamic
    /// properties at runtime.  This might include positional information,
    /// file-specific information, or runtime calculated values.
    /// </summary>
    public interface IExtensibleProperties
    {
        /// <summary>
        /// Returns true/false if property value exists.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>True = property exists, False = property does not exists</returns>
        bool DoesExtendedPropertyExist(string key);
        /// <summary>
        /// Sets an extended property value onto the object.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        void SetExtendedProperty(string key, object value);
        /// <summary>
        /// Retrieves a property value.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>Property value, or null if property does not exist.</returns>
        object GetExtendedProperty(string key);
        /// <summary>
        /// Removes the extended property
        /// </summary>
        /// <param name="key">Property Key</param>
        /// <returns>True if property existed, False if it did not</returns>
        bool ClearExtendedProperty(string key);

        /// <summary>
        /// Indexer to retrieve extended property values, also searches
        /// default values. Intended for WPF data binding scenarios.
        /// </summary>
        /// <returns>Dictionary of properties</returns>
        IDictionaryIndexer<string,object> ExtendedProperties { get; }
    }
}
