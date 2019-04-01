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
using System.IO.MemoryMappedFiles;
using System.Text;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Interfaces;
using System.Configuration;

namespace Bio.Data.Providers.FastA
{
    /// <summary>
    /// This class is used to load a set of RNA sequences from a FASTA file.
    /// </summary>
    public class FastAFile : IBioDataLoader<IAlignedBioEntity>, IDisposable
    {
        private const int SmallFileSize = 5*1024*1024; // 5M

        public string Filename { get; private set; }

        internal MemoryMappedFile MmFile { get; private set; }
        private bool _loadAllIntoMemory;
        private readonly List<IAlignedBioEntity> _sequences = new List<IAlignedBioEntity>();
        public int MaxSequenceLength { get; private set; }

        /// <summary>
        /// This is used to initialize the BioDataLoader when it is first created.
        /// </summary>
        /// <param name="filename">String data</param>
        /// <returns>True/False success</returns>
        public bool Initialize(string filename)
        {
            // Debug: special flag to load entire file into memory.  Place into 
            // <appSettings /> in the app.config file of controlling application.
            string value = ConfigurationManager.AppSettings["FastAFile:LoadIntoMemory"];
            if (!string.IsNullOrEmpty(value))
                Boolean.TryParse(value, out _loadAllIntoMemory);

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
        IList IBioDataLoader.Entities { get { return _sequences; } }

        /// <summary>
        /// Type-safe version of entity list
        /// </summary>
        public IList<IAlignedBioEntity> Entities
        {
            get { return _sequences;  }   
        }

        /// <summary>
        /// This method is used to prepare the loader to access the
        /// Entities collection.
        /// </summary>
        /// <returns>Count of loaded records</returns>
        public int Load()
        {
            Debug.Assert(Filename != string.Empty);
            _sequences.Clear();
            return LoadSequences(BioValidator.rRNAValidator);
        }

        /// <summary>
        /// This loads the sequences from the file
        /// </summary>
        /// <param name="bioValidator"></param>
        /// <returns></returns>
        private int LoadSequences(IBioValidator bioValidator)
        {
            var headerBuffer = new byte[512];
            string currHeader = string.Empty;
            long startPos = -1;
            int totalSequenceCount = 0, startingNonGap = -1;
            bool loadIntoMemory = _loadAllIntoMemory || (new FileInfo(Filename).Length < SmallFileSize);

            MmFile = MemoryMappedFile.CreateFromFile(Filename, FileMode.Open);
            using (var fs = MmFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
            {
                for (; fs != null ;)
                {
                    int db = fs.ReadByte();
                    if (db <= 0)
                        break;

                    if ((char)db == '>') // Header for species?
                    {
                        // Just finished a sequence?
                        if (currHeader != string.Empty)
                        {
                            if (MaxSequenceLength < totalSequenceCount)
                                MaxSequenceLength = totalSequenceCount;
                            if (startingNonGap == -1)
                                startingNonGap = totalSequenceCount;
                            _sequences.Add(
                                new FastASequence(this, loadIntoMemory, currHeader, startPos,
                                                  (int)(fs.Position - startPos), startingNonGap, totalSequenceCount, bioValidator));
                        }

                        // Start new sequence
                        currHeader = string.Empty;
                        totalSequenceCount = 0;
                        startingNonGap = -1;

                        for (int i = 0; currHeader == string.Empty; )
                        {
                            db = fs.ReadByte();
                            if (db == -1 || db == 0x0a || db == 0x0d)
                                currHeader = Encoding.ASCII.GetString(headerBuffer, 0, i);
                            else
                                headerBuffer[i++] = (byte)db;
                        }

                        // This is the start of the nucleotide chain
                        startPos = fs.Position;
                    }
                    else if (db != 0x0a && db != 0x0d)
                    {
                        if (startingNonGap == -1 && bioValidator.IsValid((char)db))
                            startingNonGap = totalSequenceCount;
                        totalSequenceCount++;
                    }
                }

                // Handle the final sequence in the file.
                Debug.Assert(currHeader != string.Empty);
                Debug.Assert(totalSequenceCount > 0);
                Debug.Assert(startPos > 0);

                if (MaxSequenceLength < totalSequenceCount)
                    MaxSequenceLength = totalSequenceCount;
                if (startingNonGap == -1)
                    startingNonGap = totalSequenceCount;
                
                _sequences.Add(
                    new FastASequence(this, loadIntoMemory, currHeader, startPos,
                                      (int)(fs.Position - startPos)-1, startingNonGap, totalSequenceCount, bioValidator));
            }

            // Force all sequences to have the same "virtual" length
            _sequences.ForEach(ns => ((FastASequence)ns).ForceLength(MaxSequenceLength));

            if (loadIntoMemory)
            {
                MmFile.Dispose();
                MmFile = null;
            }

            return _sequences.Count;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sequences.ForEach(seq => ((IDisposable) seq).Dispose());

            if (MmFile != null)
            {
                MmFile.Dispose();
                MmFile = null;
            }
        }

        #endregion
    }
}