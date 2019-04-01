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
using System.Text;
using Alignment;
using Bio;

namespace AlignmentLoader
{
    public class Mapper
    {
        public Mapper(SequenceAlignment alignment, string connectionString, string databaseName)
        {
            DatabaseName = databaseName;
            ConnectionString = connectionString;
            MappedAlignment = alignment;
            MappedSuccessfully = false;
        }

        public bool MappedSuccessfully { get; private set; }
        public byte AlignmentSeqTypeID { get; private set; }
        public int AlignmentID { get; private set; }

        public int GetNextSequenceID() { return NextSeqID; }
        public int GetNextAlignmentID() { return NextAlnID; }

        public bool Map()
        {
            if (MappedAlignment == null) return false;
            try
            {
                rCADDataContext dc = CreateDataContext();
                NextSeqID = dc.NextSeqIDs.Select(row => row.SeqID).First();
                AlignmentID = dc.NextAlnIDs.Select(row => row.AlnID).First();
                NextAlnID = AlignmentID + 1;

                AlignmentSeqTypeID = dc.SequenceTypes.Where(row => row.MoleculeType.Equals(_alignment.MoleculeType) && row.GeneName.Equals(_alignment.GeneName)
                    && row.GeneType.Equals(_alignment.GeneType)).First().SeqTypeID;

                var seqToTaxID = (from seq in _alignment.Sequences
                                  join taxonomyNameRow in dc.TaxonomyNames
                                  on ((SequenceMetadata)seq.Metadata[SequenceMetadata.SequenceMetadataLabel]).ScientificName equals taxonomyNameRow.ScientificName
                                  select new { seq.ID, taxonomyNameRow.TaxID }).ToDictionary(match => match.ID, match => match.TaxID);

                int rootTaxID = (from taxname in dc.TaxonomyNames
                                 where taxname.ScientificName.Equals("root")
                                 select taxname.TaxID).First();

                ExtentTypeIDs = (from bar in dc.SecondaryStructureExtentTypes
                                 select new { bar.ExtentTypeID, bar.ExtentType }).ToDictionary(match => match.ExtentType, match => match.ExtentTypeID);

                foreach (var sequence in _alignment.Sequences)
                {
                    
                    SequenceMetadata metadata = (SequenceMetadata)sequence.Metadata[SequenceMetadata.SequenceMetadataLabel];
                    SequenceMappingData data = new SequenceMappingData();
                    data.SeqID = NextSeqID;
                    data.TaxID = seqToTaxID.ContainsKey(sequence.ID) ? seqToTaxID[sequence.ID] : rootTaxID; //The sequence is mapped to the root of the Taxonomy tree if we don't have mapping info.
                    data.LocationID = dc.CellLocationInfos.Where(row => row.Description.Equals(metadata.LocationDescription)).First().LocationID;
                    sequence.Metadata.Add(rCADMappingData, data);
                    NextSeqID++;
                }
                dc.Connection.Close();
                MappedSuccessfully = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal rCADDataContext CreateDataContext()
        {
            return new rCADDataContext(ConnectionString);
        }

        internal string DatabaseName { get; private set; }
        internal string ConnectionString { get; private set; }
        internal int NextSeqID { get; private set; }
        internal int NextAlnID { get; private set; }
        internal Dictionary<string, byte> ExtentTypeIDs { get; private set; }
        internal SequenceAlignment MappedAlignment
        {
            get { return _alignment; }
            private set { _alignment = value; }
        }

        private SequenceAlignment _alignment;
        public const string rCADMappingData = "rCADMappingData";
    }
}
