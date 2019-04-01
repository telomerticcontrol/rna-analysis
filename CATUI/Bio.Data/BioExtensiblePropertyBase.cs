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
using Bio.Data.Interfaces;

namespace Bio.Data
{
    /// <summary>
    /// This class is used as the base for any extendable property class.
    /// It allows additional dynamic properties to be added to biology data
    /// structures -- from proprietary file formats, visuals, 
    /// </summary>
    public class BioExtensiblePropertyBase : IExtensibleProperties
    {
        private class DictionaryIndexer : IDictionaryIndexer<string,object>
        {
            private readonly BioExtensiblePropertyBase _parent;
            internal DictionaryIndexer(BioExtensiblePropertyBase parent) { _parent = parent; }
            public object this[string key]
            {
                get
                {
                    return _parent.DoesExtendedPropertyExist(key)
                               ? _parent.GetExtendedProperty(key)
                               : GetDefaultValue(key, _parent.GetType(), true);
                }
                set
                {
                    _parent.SetExtendedProperty(key, value);
                }
            }
        }
        private DictionaryIndexer _wrapper;

        // Static dictionary used to hold type-driven default values
        private static readonly Dictionary<string, Dictionary<Type, object>> PropertyMetadata = new Dictionary<string, Dictionary<Type, object>>();

        /// <summary>
        /// Extended property instance based dictionary
        /// </summary>
        private Dictionary<string, object> _propertyDictionary;
        private Dictionary<string,object> PropertyDictionary
        {
            get
            {
                if (_propertyDictionary == null)
                    _propertyDictionary = new Dictionary<string, object>();
                return _propertyDictionary;
            }
        }

        /// <summary>
        /// Returns true/false if property value exists.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>True = property exists, False = property does not exists</returns>
        public bool DoesExtendedPropertyExist(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return (_propertyDictionary != null && _propertyDictionary.ContainsKey(key));
        }

        /// <summary>
        /// Sets an extended property value onto the object.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetExtendedProperty(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            // Allow NULL to be set onto value - will replace "default" values.
            // Use ClearValue() to remove value.
            PropertyDictionary[key] = value;
        }

        /// <summary>
        /// Retrieves a property value.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>Property value, or null if property does not exist.</returns>
        public object GetExtendedProperty(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (_propertyDictionary != null)
            {
                object value;
                if (PropertyDictionary.TryGetValue(key, out value))
                    return value;
            }

            return null;
        }

        /// <summary>
        /// Typed GetExtendedProperty method
        /// </summary>
        /// <typeparam name="T">Type to cast return to</typeparam>
        /// <param name="key">Property key</param>
        /// <returns>Property value or default</returns>
        public T GetExtendedProperty<T>(string key)
        {
            object val = GetExtendedProperty(key);
            return (val == null) ? default(T) : (T) val;
        }

        /// <summary>
        /// Indexer to retrieve extended property values, also searches
        /// default values. Intended for WPF data binding scenarios.
        /// </summary>
        /// <returns>Value or null</returns>
        public IDictionaryIndexer<string, object> ExtendedProperties
        {
            get
            {
                if (_wrapper == null)
                    _wrapper = new DictionaryIndexer(this);
                return _wrapper;
            }
        }

        /// <summary>
        /// Removes the extended property
        /// </summary>
        /// <param name="key">Property Key</param>
        /// <returns>True if property existed, False if it did not</returns>
        public bool ClearExtendedProperty(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return (_propertyDictionary != null) ? PropertyDictionary.Remove(key) : false;
        }

        /// <summary>
        /// This method is used to register a default value for an extended property for 
        /// a given type.  
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="typeOwner">Type owner (object for all)</param>
        /// <param name="defaultValue">Value, null to remove existing value</param>
        public static void RegisterDefaultValue(string key, Type typeOwner, object defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (typeOwner == null)
                throw new ArgumentNullException("typeOwner");

            Dictionary<Type, object> defVals;
            bool hasDefVals = (PropertyMetadata.TryGetValue(key, out defVals));
            if (defaultValue == null)
            {
                if (hasDefVals)
                    defVals.Remove(typeOwner);
                if (defVals.Count == 0)
                    PropertyMetadata.Remove(key);
            }
            else
            {
                if (!hasDefVals)
                {
                    defVals = new Dictionary<Type, object>();
                    PropertyMetadata.Add(key,defVals);
                }
                defVals[typeOwner] = defaultValue;
            }
        }

        /// <summary>
        /// This method is used to retrieve a default value for a given type.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="typeOwner">Type owner</param>
        /// <param name="wantInheritedValue">true to search inheritance chain</param>
        /// <returns></returns>
        public static object GetDefaultValue(string key, Type typeOwner, bool wantInheritedValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (typeOwner == null)
                throw new ArgumentNullException("typeOwner");

            Dictionary<Type, object> defVals;
            bool hasDefVals = (PropertyMetadata.TryGetValue(key, out defVals));
            if (hasDefVals)
            {
                object value;
                if (defVals.TryGetValue(typeOwner, out value))
                    return value;
                if (wantInheritedValue)
                {
                    while (typeOwner != null)
                    {
                        typeOwner = typeOwner.BaseType;
                        if (defVals.TryGetValue(typeOwner, out value))
                            return value;
                    }
                }
            }
            return null;
        }
    }
}
