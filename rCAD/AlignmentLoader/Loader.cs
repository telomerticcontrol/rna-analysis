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
using System.IO;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.Win32;

namespace AlignmentLoader
{
    public class Loader
    {
        public Loader(Mapper alnMap)
        {
            _data = alnMap;            
             
        }

        public bool Load()
        {
            if (_data == null) return false;
            if (!_data.MappedSuccessfully) return false;

            CreateTempWorkingDirectory();
            if (_tempDirectory == null) return false;

            _alignmentColumnFile = _tempDirectory + "AlignmentColumn.dat";
            _alignmentDataFile = _tempDirectory + "AlignmentData.dat";
            _alignmentSequenceDataFile = _tempDirectory + "AlignmentSequence.dat";
            _alignmentFile = _tempDirectory + "Alignment.dat";
            _sequenceMainFile = _tempDirectory + "SequenceMain.dat";
            _sequenceAccessionFile = _tempDirectory + "SequenceAccession.dat";
            _secondaryStructureBasePairsFile = _tempDirectory + "SecondaryStructureBasePairs.dat";
            _secondaryStructureExtentsFile = _tempDirectory + "SecondaryStructureExtents.dat";

            CreateAlignmentImportFiles();
            bool retValue = LoadIntoRCAD();
            DeleteTempWorkingDirectory();

            return retValue;
        }

        public bool CreateBatchLoad(string targetDirectory)
        {
            if (targetDirectory == null) return false;

            if (_data == null) return false;
            if (_data.DatabaseName == null) return false;
            if (!_data.MappedSuccessfully) return false;

            try
            {
                if(!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                _alignmentColumnFile = targetDirectory + "\\AlignmentColumn.dat";
                _alignmentDataFile = targetDirectory + "\\AlignmentData.dat";
                _alignmentSequenceDataFile = targetDirectory + "\\AlignmentSequence.dat";
                _alignmentFile = targetDirectory + "\\Alignment.dat";
                _sequenceMainFile = targetDirectory + "\\SequenceMain.dat";
                _sequenceAccessionFile = targetDirectory + "\\SequenceAccession.dat";
                _secondaryStructureBasePairsFile = targetDirectory + "\\SecondaryStructureBasePairs.dat";
                _secondaryStructureExtentsFile = targetDirectory + "\\SecondaryStructureExtents.dat";

                string scriptFile = targetDirectory + "\\rCADAlignmentLoader.sql";

                CreateAlignmentImportFiles();
                CreateAlignmentLoaderScriptFile(scriptFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Private Methods and Properties

        private Mapper _data;
        private string _tempDirectory;
        private string _alignmentColumnFile;
        private string _alignmentDataFile;
        private string _alignmentSequenceDataFile;
        private string _alignmentFile;
        private string _sequenceMainFile;
        private string _sequenceAccessionFile;
        private string _secondaryStructureBasePairsFile;
        private string _secondaryStructureExtentsFile;

        private readonly static string ALIGNMENTLOADER_KEY = @"SOFTWARE\Gutell Lab\rCAD\AlignmentLoader";
        private readonly static string SSISPACKAGE_KEY = @"SSISPackage";

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

        private bool LoadIntoRCAD()
        {
            Application ssisApp = new Application();
            SSISEventListener eventListener = new SSISEventListener();

            string packagePath = null;

            RegistryKey alignmentLoaderKey = Registry.LocalMachine.OpenSubKey(ALIGNMENTLOADER_KEY);
            if (alignmentLoaderKey != null)
            {
                if (alignmentLoaderKey.GetValue(SSISPACKAGE_KEY, null) != null && alignmentLoaderKey.GetValueKind(SSISPACKAGE_KEY) == RegistryValueKind.String)
                {
                    packagePath = alignmentLoaderKey.GetValue(SSISPACKAGE_KEY).ToString();
                }
            }

            if (packagePath == null) return false;

            Package rCADLoadNewAlignment = null;
            try
            {
                //Should we read the location of the package out of the environment or registry? 
                rCADLoadNewAlignment = ssisApp.LoadPackage(packagePath, eventListener);
                rCADLoadNewAlignment.Variables["AlignmentDataFile"].Value = _alignmentFile;
                rCADLoadNewAlignment.Variables["AlignmentColumnDataFile"].Value = _alignmentColumnFile;
                rCADLoadNewAlignment.Variables["AlignmentDataDataFile"].Value = _alignmentDataFile;
                rCADLoadNewAlignment.Variables["AlignmentSequenceDataFile"].Value = _alignmentSequenceDataFile;
                rCADLoadNewAlignment.Variables["SequenceMainDataFile"].Value = _sequenceMainFile;
                rCADLoadNewAlignment.Variables["SequenceAccessionDataFile"].Value = _sequenceAccessionFile;
                rCADLoadNewAlignment.Variables["rCADConnectionString"].Value = _data.ConnectionString;
                rCADLoadNewAlignment.Variables["NextAlnID"].Value = _data.GetNextAlignmentID();
                rCADLoadNewAlignment.Variables["NextSeqID"].Value = _data.GetNextSequenceID();
                rCADLoadNewAlignment.Variables["SecondaryStructureBasePairsDataFile"].Value = _secondaryStructureBasePairsFile;
                rCADLoadNewAlignment.Variables["SecondaryStructureExtentsDataFile"].Value = _secondaryStructureExtentsFile;
                DTSExecResult result = rCADLoadNewAlignment.Execute(null, null, eventListener, null, null);
                if (result == DTSExecResult.Success)
                {
                    return true;
                }
                else
                {
                    using(StreamWriter output = new StreamWriter(new FileStream(Path.GetTempPath() + "AlignmentLoader.ssis_errors.out", FileMode.Create)))
                    {
                        output.Write(eventListener.ErrorLog);
                        output.Flush();
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void CreateAlignmentLoaderScriptFile(string scriptFile)
        {
            using (var targetScriptFile = File.CreateText(scriptFile))
            {
                targetScriptFile.WriteLine(@"USE {0};", _data.DatabaseName);
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                //1. Bulk Load Tables
                targetScriptFile.WriteLine(@"-- Loading Alignment");
                targetScriptFile.WriteLine(@"PRINT '*** Loading Alignment';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT Alignment FROM N'{0}'", _alignmentFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading SequenceMain");
                targetScriptFile.WriteLine(@"PRINT '*** Loading SequenceMain';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT SequenceMain FROM N'{0}'", _sequenceMainFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading AlignmentSequence");
                targetScriptFile.WriteLine(@"PRINT '*** Loading AlignmentSequence';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT AlignmentSequence FROM N'{0}'", _alignmentSequenceDataFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading AlignmentColumn");
                targetScriptFile.WriteLine(@"PRINT '*** Loading AlignmentColumn';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT AlignmentColumn FROM N'{0}'", _alignmentColumnFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading AlignmentData");
                targetScriptFile.WriteLine(@"PRINT '*** Loading AlignmentData';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT AlignmentData FROM N'{0}'", _alignmentDataFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading SequenceAccession");
                targetScriptFile.WriteLine(@"PRINT '*** Loading SequenceAccession';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT SequenceAccession FROM N'{0}'", _sequenceAccessionFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading SecondaryStructureBasePairs");
                targetScriptFile.WriteLine(@"PRINT '*** Loading SecondaryStructureBasePairs';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT SecondaryStructureBasePairs FROM N'{0}'", _secondaryStructureBasePairsFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                targetScriptFile.WriteLine(@"-- Loading SecondaryStructureExtents");
                targetScriptFile.WriteLine(@"PRINT '*** Loading SecondaryStructureExtents';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"BULK INSERT SecondaryStructureExtents FROM N'{0}'", _secondaryStructureExtentsFile);
                targetScriptFile.WriteLine(@"WITH ( CHECK_CONSTRAINTS, CODEPAGE='ACP', DATAFILETYPE='char', FIELDTERMINATOR='\t|\t');");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(targetScriptFile.NewLine);

                //2. Update AlignmentID
                targetScriptFile.WriteLine(@"-- Updating AlnID in NextAlnID");
                targetScriptFile.WriteLine(@"PRINT '*** Updating AlnID in NextAlnID';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"UPDATE NextAlnID SET AlnID = {0}", _data.GetNextAlignmentID());

                //3. Update SeqID
                targetScriptFile.WriteLine(@"-- Updating SeqID in NextSeqID");
                targetScriptFile.WriteLine(@"PRINT '*** Updating SeqID in NextSeqID';");
                targetScriptFile.WriteLine(@"GO");
                targetScriptFile.WriteLine(@"UPDATE NextSeqID SET SeqID = {0}", _data.GetNextSequenceID());

                targetScriptFile.Flush();
            }
        }

        private void CreateAlignmentImportFiles()
        {
            StreamWriter alignmentcolumn = File.CreateText(_alignmentColumnFile);
            StreamWriter alignment = File.CreateText(_alignmentFile);
            StreamWriter alignmentsequence = File.CreateText(_alignmentSequenceDataFile);
            StreamWriter alignmentdata = File.CreateText(_alignmentDataFile);
            StreamWriter sequencemain = File.CreateText(_sequenceMainFile);
            StreamWriter sequenceaccession = File.CreateText(_sequenceAccessionFile);
            StreamWriter secondarystructurebasepairs = File.CreateText(_secondaryStructureBasePairsFile);
            StreamWriter secondarystructureextents = File.CreateText(_secondaryStructureExtentsFile);

            //rCAD.Alignment: Entry in the alignment table for the new alignment
            alignment.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}", _data.AlignmentID, _data.AlignmentSeqTypeID, _data.MappedAlignment.LogicalName, _data.MappedAlignment.Columns + 1);
            alignment.Flush();
            alignment.Close();

            //rCAD.AlignmentColumn: We have a 1 to 1 mapping of the logical and physical column numbers at the start
            for (int i = 0; i < _data.MappedAlignment.Columns; i++)
            {
                alignmentcolumn.WriteLine("{0}\t|\t{1}\t|\t{2}", _data.AlignmentID, i + 1, i + 1);
            }
            alignmentcolumn.Flush();
            alignmentcolumn.Close();

            //rCAD.AlignmentSequence, rCAD.AlignmentData, rCAD.SequenceMain, rCAD.SequenceAccession written on a per-sequence basis
            //We will do duplicate checking inside the database.
            foreach (var sequence in _data.MappedAlignment.Sequences)
            {
                SequenceMetadata metadata = (SequenceMetadata)sequence.Metadata[SequenceMetadata.SequenceMetadataLabel];
                SequenceMappingData mappingMetadata = (SequenceMappingData)sequence.Metadata[Mapper.rCADMappingData];
                int seqLengthMetadata = metadata.SequenceLength;
                int firstNtColNum = -1;
                int lastNtColNumber = -1;
                int sequenceIndex = 1;

                for (int i = 0; i < sequence.Count; i++)
                {
                    if (!sequence[i].IsGap) //We are only actually writing the non-gap positions.
                    {
                        alignmentdata.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}", mappingMetadata.SeqID, _data.AlignmentID, i + 1, sequence[i].Symbol, sequenceIndex);
                        sequenceIndex++;
                        if (firstNtColNum < 0)
                            firstNtColNum = i + 1;

                        //KJD, 1/21/2010 - A nasty little bug right here where lastNtColNumber = i + 0 SHOULD be lastNtColNumber = i + 1!
                        if (firstNtColNum > 0)
                            lastNtColNumber = i + 1; //We just set the last col num value to the last column with data we've seen.
                        //lastNtColNumber = i + 0;
                    }
                }

                if ((sequenceIndex - 1) != seqLengthMetadata)
                {
                    Console.WriteLine("Warning: Existing metadata for SeqLength ({0}) does not match number of observed nt ({1}) for {2}", seqLengthMetadata, sequenceIndex - 1, sequence.ID);
                }

                sequencemain.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t\t|\t",
                    mappingMetadata.SeqID, mappingMetadata.TaxID, mappingMetadata.LocationID, _data.AlignmentSeqTypeID, seqLengthMetadata);
                alignmentsequence.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}", mappingMetadata.SeqID, _data.AlignmentID, metadata.AlignmentRowName, firstNtColNum, lastNtColNumber);

                foreach (var gbentry in metadata.Accessions)
                {
                    sequenceaccession.WriteLine("{0}\t|\t{1}\t|\t{2}", mappingMetadata.SeqID, gbentry.Accession, gbentry.Version);
                }

                if (metadata.StructureModel != null && metadata.StructureModel.Pairs.Count() > 0)
                {
                    foreach (int fivePrime in metadata.StructureModel.Pairs.Keys)
                    {
                        secondarystructurebasepairs.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}", mappingMetadata.SeqID, _data.AlignmentID, fivePrime,
                            metadata.StructureModel.Pairs[fivePrime]);
                    }

                    int extentID = 1;
                    foreach (var helix in metadata.StructureModel.Helices)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                            extentID, 1, helix.FivePrimeStart, helix.FivePrimeEnd, _data.ExtentTypeIDs["Helix"]);
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                            extentID, 2, helix.ThreePrimeStart, helix.ThreePrimeEnd, _data.ExtentTypeIDs["Helix"]);
                        extentID++;
                    }

                    foreach (var hairpinloop in metadata.StructureModel.Hairpins)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                            extentID, 1, hairpinloop.Loop.LoopStart, hairpinloop.Loop.LoopEnd, _data.ExtentTypeIDs["Hairpin Loop"]);
                        extentID++;
                    }

                    foreach (var internalloop in metadata.StructureModel.Internals)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                            extentID, 1, internalloop.FivePrimeLoop.LoopStart, internalloop.FivePrimeLoop.LoopEnd, _data.ExtentTypeIDs["Internal Loop"]);
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                            extentID, 2, internalloop.ThreePrimeLoop.LoopStart, internalloop.ThreePrimeLoop.LoopEnd, _data.ExtentTypeIDs["Internal Loop"]);
                        extentID++;
                    }

                    foreach (var bulgeloop in metadata.StructureModel.Bulges)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                            extentID, 1, bulgeloop.Bulge.LoopStart, bulgeloop.Bulge.LoopEnd, _data.ExtentTypeIDs["Bulge Loop"]);
                        extentID++;
                    }

                    foreach (var multistemloop in metadata.StructureModel.Stems)
                    {
                        int stemordinal = 1;
                        foreach (var stem in multistemloop.Segments)
                        {
                            secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                                extentID, stemordinal, stem.LoopStart, stem.LoopEnd, _data.ExtentTypeIDs["Multistem Loop"]);
                            stemordinal++;
                        }
                        extentID++;
                    }

                    foreach (var free in metadata.StructureModel.Strands)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                                extentID, 1, free.LoopStart, free.LoopEnd, _data.ExtentTypeIDs["Free"]);
                        extentID++;
                    }

                    foreach (var tail in metadata.StructureModel.Tails)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                                extentID, 1, tail.LoopStart, tail.LoopEnd, _data.ExtentTypeIDs["Tail"]);
                        extentID++;
                    }

                    foreach (var knot in metadata.StructureModel.KnottedHelices)
                    {
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                                extentID, 1, knot.FivePrimeStart, knot.FivePrimeEnd, _data.ExtentTypeIDs["Pseudoknot Helix"]);
                        secondarystructureextents.WriteLine("{0}\t|\t{1}\t|\t{2}\t|\t{3}\t|\t{4}\t|\t{5}\t|\t{6}", mappingMetadata.SeqID, _data.AlignmentID,
                                extentID, 2, knot.ThreePrimeStart, knot.ThreePrimeEnd, _data.ExtentTypeIDs["Pseudoknot Helix"]);
                        extentID++;
                    }
                }
            }
            sequencemain.Flush();
            sequencemain.Close();

            alignmentsequence.Flush();
            alignmentsequence.Close();

            alignmentdata.Flush();
            alignmentdata.Close();

            sequenceaccession.Flush();
            sequenceaccession.Close();

            secondarystructurebasepairs.Flush();
            secondarystructurebasepairs.Close();

            secondarystructureextents.Flush();
            secondarystructureextents.Close();
        }

        #endregion
    }

    internal class SSISEventListener : DefaultEvents
    {
        internal SSISEventListener()
        {
            _errorStream = new StringWriter();
        }

        public String ErrorLog
        {
            get { return _errorStream.ToString(); }
        }

        public override bool OnError(DtsObject source, int errorCode, string subComponent, string description, string helpFile, int helpContext, string idofInterfaceWithError)
        {
            _errorStream.WriteLine("SSIS Error in {0}/{1} : {2}", source, subComponent, description);
            return false;
        }

        private StringWriter _errorStream;
    }
}
