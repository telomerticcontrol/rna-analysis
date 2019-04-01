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
using System.Collections;
using System.Threading;

namespace Bio.Data.Providers.Virtualization
{
    /// <summary>
    /// A data virtualizing List<T> class/>
    /// </summary>
    /// <typeparam name="T">Underlying type</typeparam>
    public class VirtualizingList<T> : IList, IList<T>
    {
        internal class CacheEntry
        {
            public DateTime LastAccessTime { get; set; }
            public IList<T> Data { get; set; }

            public CacheEntry()
            {
                Touch();
            }

            public void Touch()
            {
                LastAccessTime = DateTime.Now;
            }
        }

        private readonly int _sliceSize, _timeout;

        private const int MIN_SLICE_THRESHOLD = 5;
        private readonly IVirtualizingListDataProvider<T> _dataProvider;
        private readonly Dictionary<int, CacheEntry> _loadedData = new Dictionary<int, CacheEntry>();
        private Timer _cleanupTimer;

        public VirtualizingList(IVirtualizingListDataProvider<T> dataProvider, int sliceSize, int timeout)
        {
            if (sliceSize == 0)
                throw new ArgumentOutOfRangeException("sliceSize", "Slice Size must be greater than zero.");
            if (timeout != 0 && timeout < 10)
                throw new ArgumentOutOfRangeException("timeout", "Timeout must be zero or greater than 10 seconds");
            
            _sliceSize = sliceSize;
            _timeout = timeout;

            if (timeout > 0)
                _cleanupTimer = new Timer(ClearStaleData, null, timeout * 1000, timeout * 1000);

            _dataProvider = dataProvider;
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                // Determine the index into our mapped area
                return GetByIndex(index);
            }
            set
            {
                SetByIndex(index, value);
            }
        }

        private T GetByIndex(int index)
        {
            int sliceIndex = index / _sliceSize;
            int sliceOffset = index % _sliceSize;

            // Load the specific slice necessary
            var ce = GetCacheEntryByIndex(sliceIndex);
            return ce == null || ce.Data == null ||
                   ce.Data.Count <= sliceOffset
                       ? default(T) 
                       :ce.Data[sliceOffset];
        }

        private void SetByIndex(int index, object data)
        {
            throw new NotImplementedException();
        }

        private void ClearStaleData(object data)
        {
            if (_loadedData.Count <= MIN_SLICE_THRESHOLD)
                return;

            lock(_loadedData)
            {
                var staleData = (from entry in _loadedData.AsEnumerable()
                             let now = DateTime.Now
                             where (now - entry.Value.LastAccessTime).TotalSeconds > _timeout
                             select entry.Key).ToList();

                foreach (var key in staleData)
                    _loadedData.Remove(key);
            }
        }
        
        private CacheEntry GetCacheEntryByIndex(int sliceIndex)
        {
            lock (_loadedData)
            {
                if (_loadedData.ContainsKey(sliceIndex))
                {
                    var entry = _loadedData[sliceIndex];
                    entry.Touch();
                    return entry;
                }
            }

            var data = LoadData(sliceIndex);

            lock (_loadedData)
            {
                if (!_loadedData.ContainsKey(sliceIndex))
                {
                    var entry = new CacheEntry {Data = data};
                    _loadedData.Add(sliceIndex, entry);
                    return entry;
                }
                return _loadedData[sliceIndex];
            }
        }

        protected virtual IList<T> LoadData(int sliceIndex)
        {
            return _dataProvider.LoadRange(sliceIndex*_sliceSize, _sliceSize).ToList();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            lock (_loadedData)
            {
                _loadedData.Clear();
            }
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int count = Math.Min(array.Length-arrayIndex, Count);
            for (int i = 0; i < count; i++)
                array[arrayIndex + count] = this[i];
        }

        public int Count
        {
            get 
            {
                return _dataProvider.GetCount();
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return GetByIndex(i);
        }

        #region IList Implementation Forwarder
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList.Add(object value)
        {
            Add((T) value);
            return -1;
        }

        bool IList.Contains(object value)
        {
            return Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T) value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T) value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Remove((T) value);
        }

        object IList.this[int index]
        {
            get
            {
                return GetByIndex(index);
            }
            set
            {
                SetByIndex(index, (T) value);
            }
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }
        #endregion
    }
}