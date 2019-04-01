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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Interfaces;

namespace Bio.Data.Providers.AE2
{
    /// <summary>
    /// This class is used to load a set of RNA sequences from an AE2 data file.
    /// It loads the entire sequence into memory - assuming that the file is not too large to manage due
    /// to AE2 restrictions.
    /// </summary>
    public class AE2File : IBioDataLoader<IAlignedBioEntity>
    {
        private const string HeaderRegex = @"#=\s+(?<Rowname>[^\n\s]+)\s+[^\n\s]+\s+(?<Statusin>[^\n\s]+)\s+(?<Statusout>[^\n\s]+)(\s+[^\n\s]+){6}";
        private const string SeqdataRowRegex = @"(?<Rowname>[^\n\s]+)\s+(?<Startindex>\d+)\s+(?<Seqdata>[^\n\s]+)";

        public string Filename { get; private set; }

        private readonly List<IAlignedBioEntity> _sequences = new List<IAlignedBioEntity>();
        public int MaxSequenceLength { get; private set; }

        /// <summary>
        /// This is used to initialize the BioDataLoader when it is first created.
        /// </summary>
        /// <param name="filename">String data</param>
        /// <returns>True/False success</returns>
        public bool Initialize(string filename)
        {
            Filename = filename;
            return true;
        }

        /// <summary>
        /// This provides access to any initialization data used to create this loader.
        /// </summary>
        public string InitializationData
        {
            get { return Path.GetFileName(Filename); }
        }

        /// <summary>
        /// The list of entities
        /// </summary>
        IList IBioDataLoader.Entities
        {
            get { return _sequences; }
        }

        /// <summary>
        /// Type-safe version of entity list
        /// </summary>
        public IList<IAlignedBioEntity> Entities
        {
            get { return _sequences; }
        }

        /// <summary>
        /// This method is used to prepare the loader to access the
        /// Entities collection.
        /// </summary>
        /// <returns>Count of loaded records</returns>
        public int Load()
        {
            Debug.Assert(Filename != string.Empty);
            return LoadSequences(BioValidator.rRNAValidator);
        }

        /// <summary>
        /// This loads the sequences from the file
        /// </summary>
        /// <param name="bioValidator"></param>
        /// <returns></returns>
        private int LoadSequences(IBioValidator bioValidator)
        {
            _sequences.Clear();

            using (var fs = File.OpenText(Filename))
            {
                string line = fs.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    // Header metadata?
                    if (Regex.IsMatch(line, HeaderRegex))
                    {
                        // Not loading metadata in this release.
                        //var headerLine = Regex.Match(line, HeaderRegex);
                        //if (headerLine.Groups["Statusin"].Value.Equals("in"))
                        //{
                        //    string rowName = headerLine.Groups["Rowname"].Value;
                        //}
                    }
                    else if (!line.StartsWith("#-") && !line.StartsWith("#:") 
                        && Regex.IsMatch(line, SeqdataRowRegex))
                    {
                        LoadSequenceData(bioValidator, Regex.Match(line, SeqdataRowRegex));
                    }

                    line = fs.ReadLine();
                }
            }

            // Calculate the first data column in each found sequence
            Parallel.ForEach(_sequences.Cast<AE2Sequence>(), seq =>
             {
                 seq.FirstDataColumn =
                     Enumerable.Range(0, seq.AlignedData.Count).FirstOrDefault(
                         i => seq.AlignedData[i].Type == BioSymbolType.Nucleotide);
             });


            return _sequences.Count;
        }

        /// <summary>
        /// This parses a single row of data and adds it to a sequence.
        /// </summary>
        /// <param name="bioValidator"></param>
        /// <param name="seqdataline"></param>
        private void LoadSequenceData(IBioValidator bioValidator, Match seqdataline)
        {
            string rowName = seqdataline.Groups["Rowname"].Value;
            Debug.Assert(!string.IsNullOrEmpty(rowName));

            AE2Sequence sequence = (AE2Sequence) _sequences.FirstOrDefault(seq => string.Compare(seq.CommonName, rowName) == 0);
            if (sequence == null)
            {
                sequence = new AE2Sequence(bioValidator) {CommonName = rowName, ScientificName = rowName};
                _sequences.Add(sequence);
            }

            // Get the data this represents.
            int startIndex = Int32.Parse(seqdataline.Groups["Startindex"].Value);

            // AE2 omits the blanks so skip anything we haven't seen with a gap.
            if (sequence.AlignedData.Count < startIndex)
            {
                for (int i = 0; i < (startIndex - sequence.AlignedData.Count); i++) 
                    sequence.AlignedData.Add(BioSymbol.Gap);
            }

            string seqdata = seqdataline.Groups["Seqdata"].Value;
            for (int index = 0; index < seqdata.Length; )
            {
                IBioSymbol symbol;
                if (seqdata[index] == '\\')
                {
                    symbol = CharacterToBioSymbol(bioValidator, ConvertOctal(new[] { seqdata[index + 1], seqdata[index + 2], seqdata[index + 3] }));
                    index += 4;
                }
                else
                {
                    symbol = CharacterToBioSymbol(bioValidator, seqdata[index]);
                    index++;
                }

                sequence.AlignedData.Add(symbol ?? BioSymbol.Gap);
            }
        }

        private static IBioSymbol CharacterToBioSymbol(IBioValidator bioValidator, char ch)
        {
            IBioSymbol symbol = null;
            switch (ch)
            {
                case '~':
                case '|':
                    symbol = BioSymbol.None;
                    break;
                case '-':
                    symbol = BioSymbol.Gap;
                    break;
                default:
                    if (bioValidator.IsValid(ch))
                        symbol = new BioSymbol(BioSymbolType.Nucleotide, ch);
                    break;
            }
            return symbol;
        }

        /// <summary>
        /// Converts an Octal (base8) number to base10 character.
        /// </summary>
        /// <param name="octal"></param>
        /// <returns></returns>
        private static char ConvertOctal(char[] octal)
        {
            int one = (Int32.Parse(Char.ToString(octal[0])));
            int two = (Int32.Parse(Char.ToString(octal[1])));
            int three = (Int32.Parse(Char.ToString(octal[2])));
            int val = 64 * one + 8 * two + three;
            return Convert.ToChar(val - 128);
        }
    }
}