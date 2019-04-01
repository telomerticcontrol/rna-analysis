using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Virtualization;

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

namespace Bio.Data.Providers.FastA
{
    internal class FastASequence : BioExtensiblePropertyBase, IAlignedBioEntity, IBioEntity, IDisposable
    {
        private readonly FastAFileSequenceDataProvider _dp;

        public string CommonName { get; set; }
        public string ScientificName { get; set; }
        public string TaxonomyId { get; set; }
        public IBioValidator Validator { get; private set; }
        public IBioEntity Entity { get { return this; } }
        public int FirstDataColumn { get; private set; }

        private readonly IList<IBioSymbol> _alignedList;

        public IList<IBioSymbol> RawData
        {
            get
            {
                return _alignedList
                    .Where(bs => bs.Type == BioSymbolType.Nucleotide)
                    .ToList();
            }
        }

        public IList<IBioSymbol> AlignedData
        {
            get { return _alignedList; }
        }

        internal FastASequence(FastAFile owner, bool loadAllIntoMemory, string header, long startPos, int length, 
                                int firstNonGap, int count, IBioValidator validator)
        {
            Debug.Assert(header.Length > 0);

            string[] bits = header.Split(new[] { "::" }, StringSplitOptions.None);
            if (bits.Length == 3)
            {
                ScientificName = /*bits[0] +":" +*/ bits[1];
                CommonName = bits[2];
            }
            else
            {
                ScientificName = header;    
            }

            Validator = validator;
            FirstDataColumn = firstNonGap;

            if (string.IsNullOrEmpty(CommonName))
                CommonName = "<Not Available>";
            if (string.IsNullOrEmpty(ScientificName))
                ScientificName = CommonName;

            _dp = new FastAFileSequenceDataProvider(owner, startPos, length, count, validator);

            if (loadAllIntoMemory)
            {
                _alignedList = new List<IBioSymbol>(_dp.LoadRange(0, length));
                _dp.Dispose();
                _dp = null;
            }
            else
                _alignedList = new VirtualizingList<IBioSymbol>(_dp, Math.Min(1024, length), 60);
        }

        internal void ForceLength(int length)
        {
            if (_dp != null)
                _dp.ForceCount(length);   
            else
            {
                for (int i = 0; i < length - _alignedList.Count; i++)
                    _alignedList.Add(BioSymbol.None);
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_dp != null)
                _dp.Dispose();
        }

        #endregion
    }
}