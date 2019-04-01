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
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Bio;
using Bio.IO;
using Bio.IO.GenBank;

namespace Alignment
{
    public class CRWSequenceAlignmentLoader : ISequenceAlignmentLoader
    {
        public CRWSequenceAlignmentLoader()
        {
        }

        public SequenceAlignment Load(string filename)
        {
            CreateTempWorkingDirectory();
            if (_tempDirectory == null) return new SequenceAlignment(); //Error condition, failed creating a temporary working directory
            if (filename == null) return new SequenceAlignment(); //Error condition, passed invalid file name

            using (_crwFile = ZipPackage.Open(filename, FileMode.Open))
            {
                if (ExtractManifest())
                {
                    if (LoadAlignmentMetadata())
                    {
                        if (LoadAlignment())
                        {
                            LoadSequenceMetadata();
                            DeleteTempWorkingDirectory();
                        }
                        else { return new SequenceAlignment(); } //Error condition, failed loading the sequence alignment into memory.
                    }
                    else { return new SequenceAlignment(); } //Error condition, missing alignment data in manifest, potential format issues
                }
                else { return new SequenceAlignment(); }  //Error condition, missing manifest file.
                return _alignment;   
            }       
        }

        #region Private Methods and Properties

        private string _tempDirectory;
        private SequenceAlignment _alignment;
        private string _logicalAlignmentName;
        private string _alignmentFileName;
        private string _moleculeType;
        private string _geneType;
        private string _geneName;
        private XElement _manifestRoot;
        //private ZipFile _crwFile;
        private Package _crwFile;


        private void DeleteTempWorkingDirectory()
        {
            try
            {
                if (_tempDirectory != null)
                    Directory.Delete(_tempDirectory, true);
            }
            catch { } //Error condition, failed to cleanup temp working directory
        }

        private void CreateTempWorkingDirectory()
        {
            try
            {
                string windowsTempDirectory = Environment.GetEnvironmentVariable("TEMP");
                string tempDirectoryLabel = Guid.NewGuid().ToString();
                _tempDirectory = windowsTempDirectory + "\\" + tempDirectoryLabel + "\\";
                Directory.CreateDirectory(_tempDirectory);
            }
            catch { _tempDirectory = null; } //When the temp directory is null or we fail to delete an existing directory, we can fail to load.
        }

        private void LoadSequenceMetadata()
        {
            //Associate available metadata with Sequence objects.
            var seqmetadata = from e in _manifestRoot.Descendants(SequenceMetadata.SequenceMetadataLabel)
                              join s in _alignment.Sequences
                              on e.Element(SequenceMetadata.AlignmentRowNameLabel).Value equals s.ID
                              select new { e, s };

            //Load the metadata
            foreach (var sm in seqmetadata)
            {
                sm.s.Metadata.Add(SequenceMetadata.SequenceMetadataLabel, new SequenceMetadata());
                SequenceMetadata metadata = (SequenceMetadata)sm.s.Metadata[SequenceMetadata.SequenceMetadataLabel];

                if (sm.e.Element(SequenceMetadata.ScientificNameLabel) != null)
                {
                    metadata.ScientificName = sm.e.Element(SequenceMetadata.ScientificNameLabel).Value;
                }

                if (sm.e.Element(SequenceMetadata.TaxIDLabel) != null)
                {
                    int taxID = 0;
                    Int32.TryParse(sm.e.Element(SequenceMetadata.TaxIDLabel).Value, out taxID);
                    metadata.TaxID = taxID;
                }

                if (sm.e.Element(SequenceMetadata.LineageLabel) != null)
                {
                    metadata.Lineage = sm.e.Element(SequenceMetadata.LineageLabel).Value;
                }

                if (sm.e.Element(SequenceMetadata.SequenceLengthLabel) != null)
                {
                    int seqLength = 0;
                    Int32.TryParse(sm.e.Element(SequenceMetadata.SequenceLengthLabel).Value, out seqLength);
                    metadata.SequenceLength = seqLength;
                }

                if (sm.e.Element(SequenceMetadata.LocationDescriptionLabel) != null)
                {
                    metadata.LocationDescription = sm.e.Element(SequenceMetadata.LocationDescriptionLabel).Value;
                }

                if (sm.e.Element(SequenceMetadata.AlignmentRowNameLabel) != null)
                {
                    metadata.AlignmentRowName = sm.e.Element(SequenceMetadata.AlignmentRowNameLabel).Value;
                }

                if (sm.e.Element(SequenceMetadata.AccessionsLabel) != null)
                {
                    var accessions = from accession in sm.e.Element(SequenceMetadata.AccessionsLabel).Descendants(SequenceMetadata.GenbankAccessionLabel)
                                     select new GenBankVersion
                                     {
                                         Accession = accession.Element(SequenceMetadata.GenbankAccessionIDLabel).Value,
                                         Version = accession.Element(SequenceMetadata.GenbankAccessionVersionLabel).Value
                                     };
                    foreach (var accession in accessions)
                    {
                        metadata.Accessions.Add(accession);
                    }

                }

                if (sm.e.Element(SequenceMetadata.StructureModelLabel) != null)
                {
                    StructureModel strmodel = new StructureModel(metadata.SequenceLength);
                    var pairs = from pair in sm.e.Element(SequenceMetadata.StructureModelLabel).Descendants(SequenceMetadata.StructureModelPairLabel)
                                select pair;
                    foreach (var pair in pairs)
                    {
                        int fivePrimeIndex, threePrimeIndex;
                        if (Int32.TryParse(pair.Element(SequenceMetadata.StructureModelPairFivePrimeIndexLabel).Value, out fivePrimeIndex) &&
                            Int32.TryParse(pair.Element(SequenceMetadata.StructureModelPairThreePrimeIndexLabel).Value, out threePrimeIndex))
                        {
                            strmodel.Pairs.Add(fivePrimeIndex, threePrimeIndex);
                        }
                    }
                    strmodel.DecomposeStructure();
                    metadata.StructureModel = strmodel;
                }
            }
        }

        private bool LoadAlignment()
        {
            Uri alignmentURI = new Uri("/" + _alignmentFileName, UriKind.Relative);
            var alnfileentry = from entry in _crwFile.GetParts()
                               where entry.Uri.Equals(alignmentURI)
                               select entry;
            PackagePart alnEntry = alnfileentry.First();
            if (alnEntry != null)
            {
                //Extract the alignment file from the zip archive.
                using (var reader = new BufferedStream(alnEntry.GetStream(FileMode.Open)))
                {
                    using (var writer = new BufferedStream(File.Create(_tempDirectory + _alignmentFileName)))
                    {
                        byte[] buf = new byte[1024];
                        int readCount = 0;
                        while ((readCount = reader.Read(buf, 0, buf.Length)) > 0)
                        {
                            writer.Write(buf, 0, readCount);
                        }
                    }
                }
                   
                ISequenceParser parser = SequenceParsers.FindParserByFile(_tempDirectory + _alignmentFileName);
                _alignment = new SequenceAlignment(parser.Parse(_tempDirectory + _alignmentFileName));
                _alignment.MoleculeType = _moleculeType;
                _alignment.GeneType = _geneType;
                _alignment.GeneName = _geneName;
                _alignment.LogicalName = _logicalAlignmentName;
                foreach (var seq in _alignment.Sequences)
                {
                    //Workaround for MBF Framework, have to cast Sequence objects as writeable, opened feature request.
                    Sequence s = seq as Sequence;
                    if (s != null)
                    {
                        s.IsReadOnly = false;
                    }
                }
                return true;
            }
            else { return false; } //Error condition, missing alignment file.
        }

        private bool LoadAlignmentMetadata()
        {
            var alnfile = from aln in _manifestRoot.Descendants(CRWSequenceAlignmentFormatTags.AlignmentLabel)
                          select new
                          {
                              File = aln.Element(CRWSequenceAlignmentFormatTags.AlignmentFileNameLabel),
                              SequenceType = aln.Element(CRWSequenceAlignmentFormatTags.SequenceTypeLabel),
                              LogicalName = aln.Element(CRWSequenceAlignmentFormatTags.AlignmentLogicalNameLabel)
                          };
            if (alnfile.Count() == 1)
            {
                _moleculeType = (alnfile.First().SequenceType.Element(CRWSequenceAlignmentFormatTags.MoleculeTypeLabel).Value != null) ? alnfile.First().SequenceType.Element(CRWSequenceAlignmentFormatTags.MoleculeTypeLabel).Value : null;
                _geneType = (alnfile.First().SequenceType.Element(CRWSequenceAlignmentFormatTags.GeneTypeLabel).Value != null) ? alnfile.First().SequenceType.Element(CRWSequenceAlignmentFormatTags.GeneTypeLabel).Value : null;
                _geneName = (alnfile.First().SequenceType.Element(CRWSequenceAlignmentFormatTags.GeneNameLabel).Value != null) ? alnfile.First().SequenceType.Element(CRWSequenceAlignmentFormatTags.GeneNameLabel).Value : null;
                if (_moleculeType == null ||
                    _geneName == null ||
                    _geneType == null)
                {
                    return false; //Error condition, sequence type information missing
                }

                _alignmentFileName = alnfile.First().File.Value;
                _logicalAlignmentName = alnfile.First().LogicalName.Value;

                if (_alignmentFileName == null)
                {
                    return false; //Error condition, missing the actual alignment file name pointer in the manifest.
                }

                if (_logicalAlignmentName == null)
                    _logicalAlignmentName = _alignmentFileName;

                return true;
            }
            else { return false; } //Error condition, multiple alignment metadata blocks in the manifest.
        }

        private bool ExtractManifest()
        {
            Uri manifestUri = new Uri("/" + CRWSequenceAlignmentFormatTags.ManifestFileName, UriKind.Relative);
            var manifest = from entry in _crwFile.GetParts()
                           where entry.Uri.Equals(manifestUri)
                           select entry;

            PackagePart manifestEntry = manifest.First();
            if (manifestEntry != null)
            {
                //Extract the manifest file from the zip archive.
                using (var reader = new BufferedStream(manifestEntry.GetStream(FileMode.Open)))
                {
                    using (var writer = new BufferedStream(File.Create(_tempDirectory + CRWSequenceAlignmentFormatTags.ManifestFileName)))
                    {
                        byte[] buf = new byte[1024];
                        int readCount = 0;
                        while ((readCount = reader.Read(buf, 0, buf.Length)) > 0)
                        {
                            writer.Write(buf, 0, readCount);
                        }
                    }
                }
                
                _manifestRoot = XElement.Load(_tempDirectory + CRWSequenceAlignmentFormatTags.ManifestFileName);
                return true;
            }
            return false; //Error condition, missing manifest file.
        }

        #endregion
    }
}
