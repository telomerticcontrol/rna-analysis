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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bio;

namespace Alignment
{
    public class AE2SequenceAlignmentLoader : ISequenceAlignmentLoader
    {
        public AE2SequenceAlignmentLoader()
        {
            _alignmentHeaderIndex = new Dictionary<string, SequenceMetadata>();
            _alignmentSequenceIndex = new Dictionary<string, ISequence>();
            _sequences = new List<ISequence>();
        }

        public SequenceAlignment Load(string filename)
        {
            using (var ae2File = File.OpenText(filename))
            {
                _sequences.Clear();
                _alignmentHeaderIndex.Clear();
                _alignmentSequenceIndex.Clear();

                string line = ae2File.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (Regex.IsMatch(line, HEADER_REGEX))
                    {
                        Match headerline = Regex.Match(line, HEADER_REGEX);
                        LoadRowMetadata(headerline);
                    }
                    else if (!line.StartsWith("#-") && !line.StartsWith("#:") && Regex.IsMatch(line, SEQDATA_ROW_REGEX))
                    {
                        Match seqdataline = Regex.Match(line, SEQDATA_ROW_REGEX);
                        LoadSequenceData(seqdataline);
                    }
                    line = ae2File.ReadLine();
                }
                return new SequenceAlignment(_sequences);
            }
        }

        #region Private Methods and Properties

        private static string HEADER_REGEX = @"#=\s+(?<Rowname>[^\n\s]+)\s+[^\n\s]+\s+(?<Statusin>[^\n\s]+)\s+(?<Statusout>[^\n\s]+)(\s+[^\n\s]+){6}";
        private static string SEQDATA_ROW_REGEX = @"(?<Rowname>[^\n\s]+)\s+(?<Startindex>\d+)\s+(?<Seqdata>[^\n\s]+)";
        private List<ISequence> _sequences;
        private Dictionary<string, SequenceMetadata> _alignmentHeaderIndex;
        private Dictionary<string, ISequence> _alignmentSequenceIndex;

        private void LoadRowMetadata(Match headerline)
        {
            if (headerline.Groups["Statusin"].Value.Equals("in"))
            {
                SequenceMetadata metadata = new SequenceMetadata();
                metadata.AlignmentRowName = headerline.Groups["Rowname"].Value;
                _alignmentHeaderIndex.Add(metadata.AlignmentRowName, metadata);
            }
        }

        private void LoadSequenceData(Match seqdataline)
        {
            if (_alignmentHeaderIndex.ContainsKey(seqdataline.Groups["Rowname"].Value))
            {
                SequenceMetadata metadata = _alignmentHeaderIndex[seqdataline.Groups["Rowname"].Value];
                ISequence sequence;
                int startIndex = Int32.Parse(seqdataline.Groups["Startindex"].Value);
                if (startIndex == 0)
                {
                    sequence = new Sequence(RnaAlphabet.Instance)
                    {
                        ID = metadata.AlignmentRowName,
                        DisplayID = metadata.AlignmentRowName
                    };
                    sequence.Metadata.Add(SequenceMetadata.SequenceMetadataLabel, metadata);
                    _alignmentSequenceIndex.Add(metadata.AlignmentRowName, sequence);
                   _sequences.Add(sequence);
                }
                else
                {
                    sequence = _alignmentSequenceIndex[seqdataline.Groups["Rowname"].Value];
                }

                if (sequence.Count < startIndex)
                {
                    //Becuase AE2 format has blanks, we might have to pad
                    for (int i = 0; i < (startIndex - sequence.Count); i++) sequence.Add(RnaAlphabet.Instance.Gap);
                }

                int lineidx = 0;
                ISequenceItem nextElement;
                char[] octalholder = new char[3];
                string seqdata = seqdataline.Groups["Seqdata"].Value;
                while (lineidx < seqdata.Length)
                {
                    if (seqdata[lineidx] == '\\')
                    {
                        octalholder[0] = seqdata[lineidx + 1];
                        octalholder[1] = seqdata[lineidx + 2];
                        octalholder[2] = seqdata[lineidx + 3];
                        nextElement = sequence.Alphabet.LookupBySymbol(ConvertOctal(octalholder));
                        lineidx = lineidx + 4;
                    }
                    else
                    {
                        nextElement = sequence.Alphabet.LookupBySymbol(seqdata[lineidx]);
                        lineidx = lineidx + 1;
                    }

                    if (nextElement == null)
                        sequence.Add(RnaAlphabet.Instance.Gap);
                    else
                    {
                        if (!nextElement.IsGap) metadata.SequenceLength++;
                        sequence.Add(nextElement);
                    }
                }
            }
        }

        private char ConvertOctal(char[] octal)
        {
            int one = (Int32.Parse(Char.ToString(octal[0])));
            int two = (Int32.Parse(Char.ToString(octal[1])));
            int three = (Int32.Parse(Char.ToString(octal[2])));
            int val = 64 * one + 8 * two + three;
            return Convert.ToChar(val - 128);
        }

        #endregion
    }
}
