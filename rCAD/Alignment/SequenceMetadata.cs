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
using Bio.IO.GenBank;
using System.Xml.Linq;
using Bio;

namespace Alignment
{
    public class SequenceMetadata
    {
        public static string LineageLabel = "Lineage";
        public static string ScientificNameLabel = "ScientificName";
        public static string TaxIDLabel = "TaxID";
        public static string SequenceLengthLabel = "SequenceLength";
        public static string AlignmentRowNameLabel = "AlignmentRowName";
        public static string LocationDescriptionLabel = "LocationDescription";
        public static string SequenceMetadataLabel = "SequenceMetadata";
        public static string AccessionsLabel = "Accessions";
        public static string GenbankAccessionLabel = "GBAccession";
        public static string GenbankAccessionIDLabel = "GBAccessionID";
        public static string GenbankAccessionVersionLabel = "GBAccessionVersion";
        public static string StructureModelsLabel = "StructureModels";
        public static string StructureModelLabel = "StructureModel";
        public static string StructureModelPairLabel = "Pair";
        public static string StructureModelPairFivePrimeIndexLabel = "FivePrimeIndex";
        public static string StructureModelPairThreePrimeIndexLabel = "ThreePrimeIndex";

        public SequenceMetadata()
        {
            Accessions = new List<GenBankVersion>();
        }

        public List<GenBankVersion> Accessions { get; private set; }
        public StructureModel StructureModel { get; set; }
        public int SeqID { get; set; }
        public string ScientificName { get; set; }
        public int TaxID { get; set; }
        public string Lineage { get; set; }
        public string LocationDescription { get; set; }
        public byte LocationID { get; set; }
        public byte SeqTypeID { get; set; }
        public int SequenceLength { get; set; }
        public string AlignmentRowName { get; set; }
    }
}
