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
using System.Xml.Linq;
using System.IO;
using System.IO.Packaging;
using Bio;
using Bio.IO.Fasta;

namespace Alignment
{
    public class CRWSequenceAlignmentWriter : ISequenceAlignmentWriter
    {
        public CRWSequenceAlignmentWriter()
        {
        }

        public void Write(string filename, SequenceAlignment alignment)
        {
            if (filename == null) return; //Error condition, output filename not specified.
            CreateTempWorkingDirectory();
            if (_tempDirectory != null)
            {
                WriteManifest(alignment);
                WriteAlignment(alignment);

                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch 
                    {
                        DeleteTempWorkingDirectory(); //Error condition, failed to remove an existing copy of the target file    
                        return;
                    }
                }

                Package outputfile = ZipPackage.Open(filename, FileMode.Create);
                PackagePart manifestFilePart = outputfile.CreatePart(new Uri("/" + CRWSequenceAlignmentFormatTags.ManifestFileName, UriKind.Relative), System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Fast);
                PackagePart alignmentFilePart = outputfile.CreatePart(new Uri("/" + CRWSequenceAlignmentFormatTags.AlignmentFileName, UriKind.Relative), System.Net.Mime.MediaTypeNames.Text.Plain, CompressionOption.Maximum);
                Stream manifestFileStream = manifestFilePart.GetStream();
                Stream alignmentFileStream = alignmentFilePart.GetStream();
                using (var manifestFile = new BufferedStream(File.Open((_tempDirectory + CRWSequenceAlignmentFormatTags.ManifestFileName), FileMode.Open)))
                {
                    byte[] buf = new byte[1024]; //We read with 1MB buffer
                    int count = 0;
                    while ((count = manifestFile.Read(buf, 0, buf.Length)) > 0)
                    {
                        manifestFileStream.Write(buf, 0, count); //We write into the zip archive
                    }
                }

                using (var alignmentFile = new BufferedStream(File.Open((_tempDirectory + CRWSequenceAlignmentFormatTags.AlignmentFileName), FileMode.Open)))
                {
                    byte[] buf = new byte[1024]; //We read with 1MB buffer
                    int count = 0;
                    while ((count = alignmentFile.Read(buf, 0, buf.Length)) > 0)
                    {
                        alignmentFileStream.Write(buf, 0, count); //We write into the zip archive
                    }
                }
                outputfile.Close();
                DeleteTempWorkingDirectory();
                return;
            }
            else 
            {
                DeleteTempWorkingDirectory();
                return;
            } //Error condition, failed to create a working directory.
        }

        #region Private Methods and Properties

        private string _tempDirectory;

        private void DeleteTempWorkingDirectory()
        {
            try
            {
                if(_tempDirectory!=null)
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

        private void WriteAlignment(SequenceAlignment alignment)
        {
            FastaFormatter formatter = new FastaFormatter();
            StreamWriter alnoutput = new StreamWriter(_tempDirectory + CRWSequenceAlignmentFormatTags.AlignmentFileName);
            alnoutput.AutoFlush = true;
            foreach (ISequence sequence in alignment.Sequences)
            {
                alnoutput.Write(formatter.FormatString(sequence));
            }
            alnoutput.Close();
        }

        private void WriteManifest(SequenceAlignment alignment)
        {
            XElement manifest = new XElement(CRWSequenceAlignmentFormatTags.ManifestLabel);

            XElement alnMetadata = WriteAlignmentMetadata(alignment);
            manifest.Add(WriteAlignmentMetadata(alignment));
            
            foreach (var sequence in alignment.Sequences)
            {
                XElement seqmetdata = WriteSequenceMetadata(sequence);
                if (seqmetdata != null) manifest.Add(seqmetdata); 
            }

            manifest.Save(_tempDirectory + CRWSequenceAlignmentFormatTags.ManifestFileName);
        }

        private XElement WriteAlignmentMetadata(SequenceAlignment alignment)
        {
            XElement output = new XElement(CRWSequenceAlignmentFormatTags.AlignmentLabel);
            output.Add(new XElement(CRWSequenceAlignmentFormatTags.AlignmentFileNameLabel, CRWSequenceAlignmentFormatTags.AlignmentFileName));
            output.Add(new XElement(CRWSequenceAlignmentFormatTags.AlignmentLogicalNameLabel, alignment.LogicalName));
            output.Add(new XElement(CRWSequenceAlignmentFormatTags.SequenceTypeLabel,
                                new XElement(CRWSequenceAlignmentFormatTags.MoleculeTypeLabel, alignment.MoleculeType),
                                new XElement(CRWSequenceAlignmentFormatTags.GeneTypeLabel, alignment.GeneType),
                                new XElement(CRWSequenceAlignmentFormatTags.GeneNameLabel, alignment.GeneName)));
            return output;
        }

        private XElement WriteSequenceMetadata(ISequence sequence)
        {
            XElement output = new XElement(SequenceMetadata.SequenceMetadataLabel);
            if(sequence.Metadata.ContainsKey(SequenceMetadata.SequenceMetadataLabel))
            {
                SequenceMetadata seqMetadata = (SequenceMetadata)sequence.Metadata[SequenceMetadata.SequenceMetadataLabel];
                if (seqMetadata.ScientificName != null) output.Add(new XElement(SequenceMetadata.ScientificNameLabel, seqMetadata.ScientificName));
                output.Add(new XElement(SequenceMetadata.SequenceLengthLabel, seqMetadata.SequenceLength));
                output.Add(new XElement(SequenceMetadata.LineageLabel, seqMetadata.Lineage));
                if (seqMetadata.AlignmentRowName != null) output.Add(new XElement(SequenceMetadata.AlignmentRowNameLabel, seqMetadata.AlignmentRowName));
                if (seqMetadata.LocationDescription != null) output.Add(new XElement(SequenceMetadata.LocationDescriptionLabel, seqMetadata.LocationDescription));
                if (seqMetadata.Accessions.Count() > 0)
                {
                    XElement accession = new XElement(SequenceMetadata.AccessionsLabel);
                    foreach (var acc in seqMetadata.Accessions)
                    {
                        XElement gbacc = new XElement(SequenceMetadata.GenbankAccessionLabel);
                        gbacc.Add(new XElement(SequenceMetadata.GenbankAccessionIDLabel, acc.Accession));
                        gbacc.Add(new XElement(SequenceMetadata.GenbankAccessionVersionLabel, acc.Version));
                        accession.Add(gbacc);
                    }
                    output.Add(accession);
                }

                if (seqMetadata.StructureModel != null)
                {
                    var model = from pair in seqMetadata.StructureModel.Pairs
                                select new XElement(SequenceMetadata.StructureModelPairLabel,
                                            new XElement(SequenceMetadata.StructureModelPairFivePrimeIndexLabel, pair.Key),
                                            new XElement(SequenceMetadata.StructureModelPairThreePrimeIndexLabel, pair.Value));
                    XElement strmodelxml = new XElement(SequenceMetadata.StructureModelLabel);
                    strmodelxml.Add(model.ToArray());
                    output.Add(strmodelxml);
                }

                /*if (seqMetadata.StructureModels.Count() > 0)
                {
                    var models = from strmodel in seqMetadata.StructureModels
                                 select new XElement(SequenceMetadata.StructureModelLabel,
                                            from pair in strmodel.Pairs
                                            select new XElement(SequenceMetadata.StructureModelPairLabel,
                                                        new XElement(SequenceMetadata.StructureModelPairFivePrimeIndexLabel, pair.Key),
                                                        new XElement(SequenceMetadata.StructureModelPairThreePrimeIndexLabel, pair.Value)));

                    XElement strmodels = new XElement(SequenceMetadata.StructureModelsLabel);
                    strmodels.Add(models.ToArray());
                    output.Add(strmodels);
                }*/
                return output;
            }
            else { return null; }
        }

        #endregion
    }
}
