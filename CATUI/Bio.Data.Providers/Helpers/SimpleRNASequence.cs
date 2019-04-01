using System.Collections.Generic;
using System.Diagnostics;
using Bio.Data.Interfaces;

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

namespace Bio.Data.Providers.Helpers
{
    public class SimpleRNASequence : BioExtensiblePropertyBase, IBioEntity
    {
        public string ScientificName { get; set; }
        public string TaxonomyId { get; set; }

        public IBioValidator Validator
        {
            get { return _validator; }
        }

        public IList<IBioSymbol> RawData
        {
            get { return _rawData; }
        }

        public SimpleRNASequence()
        {
            _validator = BioValidator.rRNAValidator;
            _rawData = new List<IBioSymbol>();
        }

        public bool AddSymbol(char c)
        {
            Debug.Assert(Validator != null);
            return AddSymbol(new BioSymbol(BioSymbolType.Nucleotide, c));
        }

        public bool AddSymbol(IBioSymbol s)
        {
            Debug.Assert(Validator != null);
            if(Validator.IsValid(s))
            {
                _rawData.Add(s);
                return true;
            }
            return false;
        }
        
        private readonly IBioValidator _validator;
        private readonly List<IBioSymbol> _rawData;
    }
}
