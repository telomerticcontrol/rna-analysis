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
using System.IO;
using Bio.Data.Interfaces;
using System.Text.RegularExpressions;
using Bio.Data;

namespace Bio.Views.Structure.Models
{
    public class PhyloDData
    {
        const string PhyloDFormatPattern = @"(?<leafdistribution>[A-Za-z]+)(?:\s+(?<predictornt>[AGCUN])@(?<predictoridx>\d+))(?:\s+(?<targetnt>[AGCUN])@(?<targetidx>\d+))(\s\d+){4}(?<pvalue>\s+[0-9]+(\.[0-9]+)?([Ee][+-][0-9]+)?)(?<qvalue>\s+[0-9]+(\.[0-9]+)?([Ee][+-][0-9]+)?)";

        public static IEnumerable<PhyloDInteraction> Load(string filename, IBioEntity sequence)
        {
            try
            {
                if(sequence.RawData.Count <= 0) return new List<PhyloDInteraction>();
                string filedata = File.ReadAllText(filename);
                IEnumerable<PhyloDInteraction> retValue = from Match match in Regex.Matches(filedata, PhyloDFormatPattern)
                                                          let predictoridx = Int32.Parse(match.Groups["predictoridx"].Value)
                                                          let targetidx = Int32.Parse(match.Groups["targetidx"].Value)
                                                          where predictoridx <= sequence.RawData.Count && targetidx <= sequence.RawData.Count
                                                          /*&& sequence.RawData[predictoridx-1].Text.Equals(match.Groups["predictornt"].Value)
                                                          && sequence.RawData[targetidx-1].Text.Equals(match.Groups["targetnt"].Value)*/
                                                          select new PhyloDInteraction()
                                                          {
                                                              PValue = Double.Parse(match.Groups["pvalue"].Value),
                                                              QValue = Double.Parse(match.Groups["qvalue"].Value),
                                                              PredictorIndex = predictoridx,
                                                              PredictorNucleotide = new BioSymbol(BioSymbolType.Nucleotide, match.Groups["predictornt"].Value[0]),
                                                              TargetIndex = targetidx,
                                                              TargetNucleotide = new BioSymbol(BioSymbolType.Nucleotide, match.Groups["targetnt"].Value[0])
                                                          };
                return retValue;
            }
            catch (Exception)
            {
                return new List<PhyloDInteraction>();
            }
        }
    }
}
