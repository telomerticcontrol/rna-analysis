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
using Bio.Data.Providers.Interfaces;
using Bio.Data.Providers.Helpers;
using System.IO;
using System.Text.RegularExpressions;
using Bio.Data.Interfaces;
using System.Collections;

namespace Bio.Data.Providers.Structure
{
    /// <summary>
    /// This loads the bpseq file format
    /// </summary>
    class BPSeqFile : IBioDataLoader<IStructureModelBioEntity>
    {
        public string Filename { get; private set; }

        IList IBioDataLoader.Entities { get { return _basePairs; } }

        public IList<IStructureModelBioEntity> Entities
        {
            get { return _basePairs; }
        }

        public string Description
        {
            get 
            { 
                return ((BioFormatAttribute)typeof(BPSeqFile).GetCustomAttributes(typeof(BioFormatAttribute), true)[0]).Key;
            }
        }

        public int Load()
        {
            //We pretty much ignore the passed in vaidator since we require an RNA sequence.
            _sequence = new SimpleRNASequence();
            _basePairs.Clear();
            return LoadBasePairs();
        }

        public bool Initialize(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            Filename = fileName;
            return true;
        }

        /// <summary>
        /// This provides access to any initialization data used to create this loader.
        /// </summary>
        public string InitializationData
        {
            get { return Path.GetFileName(Filename); }
        }

        private int LoadBasePairs()
        {
            using (var reader = File.OpenText(Filename))
            {
                string line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (Bp_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, @" ");
                        int fivePrimeIdx = Int32.Parse(tokens[0]);
                        int threePrimeIdx = Int32.Parse(tokens[2]);
                        _sequence.AddSymbol(tokens[1][0]);
                        //We simultaneously insure that we are not parsing the reverse designation
                        //of the same base pair.
                        if (threePrimeIdx > 0 && threePrimeIdx > fivePrimeIdx)
                        {
                            SimpleRNABasePair bp = new SimpleRNABasePair(_sequence)
                            {
                                FivePrimeIndex = fivePrimeIdx - 1,
                                ThreePrimeIndex = threePrimeIdx - 1
                            };
                            _basePairs.Add(bp);
                        }
                    }
                    line = reader.ReadLine();
                }
            }
            return _basePairs.Count;
        }

        private readonly List<IStructureModelBioEntity> _basePairs = new List<IStructureModelBioEntity>();
        private SimpleRNASequence _sequence;
        private static Regex Bp_Def = new Regex(@"\d\s[a-zA-Z]\s\d");
    }
}
