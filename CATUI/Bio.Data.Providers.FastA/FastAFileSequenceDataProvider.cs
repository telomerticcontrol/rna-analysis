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
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Virtualization;

namespace Bio.Data.Providers.FastA
{
    /// <summary>
    /// This provides the list of sequences based on a position within a FASTA file format.
    /// </summary>
    class FastAFileSequenceDataProvider : IVirtualizingListDataProvider<IBioSymbol>, IDisposable
    {
        private Stream _fs;
        private int _count;
        private readonly IBioValidator _validator;

        public FastAFileSequenceDataProvider(FastAFile owner, long startPos, int length, int count, IBioValidator bv)
        {
            _fs = owner.MmFile.CreateViewStream(startPos, length, MemoryMappedFileAccess.Read);
            _count = count;
            _validator = bv;
        }

        internal void ForceCount(int count)
        {
            _count = count;
        }

        public int GetCount()
        {
            return _count;
        }

        public IEnumerable<IBioSymbol> LoadRange(int startIndex, int count)
        {
            if (startIndex > _fs.Length)
                return Enumerable.Range(0, count).Select(i => BioSymbol.None);

            int availCount = GetCount();
            if (count + startIndex > availCount)
                count = availCount - startIndex;

            return GetData()
                .Where(ch => ch > 0 && ch != '\r' && ch != '\n')
                .Skip(startIndex)
                .Take(count)
                .Select(CreateBioSymbol)
                .ForceDataLength(count)
                .ToList();
        }

        private IBioSymbol CreateBioSymbol(byte b)
        {
            var ch = (char) b;
            return (ch == '-') ? BioSymbol.Gap : (ch == '~') ? BioSymbol.None
                    : _validator.IsValid(ch)
                        ? new BioSymbol(BioSymbolType.Nucleotide, ch)
                        : new BioSymbol(BioSymbolType.Unknown, ch);
        }

        public IEnumerable<byte> GetData()
        {
            int data;
            while ((data = _fs.ReadByte()) != -1)
                yield return (byte)data;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_fs != null)
            {
                _fs.Dispose();
                _fs = null;
            }
        }

        #endregion
    }
}