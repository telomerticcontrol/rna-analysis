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
using JulMar.Windows.Mvvm;
using Alignment;
using System.Collections.ObjectModel;
using JulMar.Windows;
using AlignmentLoader;
using System.Windows.Input;
using System.ComponentModel;
using Utilities.Data;

namespace AlignmentLoaderDialog.ViewModels
{
    public class AlignmentViewModel : ViewModel
    {
        public int AlignmentID 
        {
            get { return _alignmentID; }
            private set
            {
                _alignmentID = value;
                OnPropertyChanged("AlignmentID");
            }
        }
        
        public byte AlignmentSeqTypeID 
        {
            get { return _alignmentSeqTypeID; }
            private set
            {
                _alignmentSeqTypeID = value;
                OnPropertyChanged("AlignmentSeqTypeID");
            }
        }

        public int StructureModelCount
        {
            get { return _alignmentStructureModels; }
            private set
            {
                _alignmentStructureModels = value;
                OnPropertyChanged("StructureModelCount");
            }
        }

        public int SequenceCount
        {
            get { return _alignment.Rows; }
        }

        public string MoleculeType
        {
            get { return _alignment.MoleculeType; }
        }

        public string GeneType
        {
            get { return _alignment.GeneType; }
        }

        public string GeneName
        {
            get { return _alignment.GeneName; }
        }

        public string LogicalName
        {
            get { return _alignment.LogicalName; }
        }

        public bool IsLoadedToRCAD
        {
            get { return _isLoadedToRCAD; }
            private set
            {
                _isLoadedToRCAD = value;
                OnPropertyChanged("IsLoadedToRCAD");
            }
        }

        public bool IsMappedToRCAD
        {
            get { return _isMappedToRCAD; }
            private set
            {
                _isMappedToRCAD = value;
                OnPropertyChanged("IsMappedToRCAD");
            }
        }

        public string RCADMappingStatus
        {
            get { return _mappingStatus; }
            private set
            {
                _mappingStatus = value;
                OnPropertyChanged("RCADMappingStatus");
            }
        }

        public string RCADLoadingStatus
        {
            get { return _loadingStatus; }
            private set
            {
                _loadingStatus = value;
                OnPropertyChanged("RCADLoadingStatus");
            }
        }

        public ObservableCollection<SequenceViewModel> Sequences
        {
            get { return _sequences; }
        }

        public ICommand MapToRCADCommand { get; private set; }
        public ICommand LoadToRCADCommand { get; private set; }
        public ICommand ClearRCADMappingCommand { get; private set; }

        public void ClearRCADMapping()
        {
            if (IsMappedToRCAD)
            {
                RCADMappingStatus = ALIGNMENT_NOT_MAPPED_MESSAGE;
                _alignmentToRCADMapping = null;
                IsMappedToRCAD = false;
                IsLoadedToRCAD = false;
                AlignmentID = 0;
                AlignmentSeqTypeID = 0;
                CommandManager.InvalidateRequerySuggested();
                SendMessage(ViewMessages.ClearRCADMapping, null); //We notify all sequences to clear their mappings.
            }
        }

        public AlignmentViewModel(SequenceAlignment alignment)
        {
            _alignment = alignment;
            _sequences = new MTObservableCollection<SequenceViewModel>();
            MapToRCADCommand = new DelegatingCommand<rCADConnection>(MapAlignmentToRCAD, (a) => (!MappingToRCAD && !IsMappedToRCAD && a!=null));
            //MapToRCADCommand = new DelegatingCommand<string>(MapAlignmentToRCAD, (a) => (!MappingToRCAD && !IsMappedToRCAD && a != null));
            LoadToRCADCommand = new DelegatingCommand<string>(LoadAlignmentToRCAD, (a) => (!MappingToRCAD && IsMappedToRCAD && _alignmentToRCADMapping!=null && !LoadingToRCADFailed && !LoadingToRCAD && !IsLoadedToRCAD));
            //LoadToRCADCommand = new DelegatingCommand(LoadAlignmentToRCAD, () => (!MappingToRCAD && IsMappedToRCAD && _alignmentToRCADMapping != null && !LoadingToRCADFailed && !LoadingToRCAD && !IsLoadedToRCAD));
            Initialize();
        }

        internal bool MappingToRCAD
        {
            get { return _mappingToRCAD; }
            private set { _mappingToRCAD = value; }
        }

        internal bool LoadingToRCAD
        {
            get { return _loadingToRCAD; }
            private set { _loadingToRCAD = value; }
        }

        internal bool LoadingToRCADFailed
        {
            get { return _loadingToRCADFailed; }
            private set { _loadingToRCADFailed = value; }
        }

        private int _alignmentStructureModels;
        private int _alignmentID;
        private byte _alignmentSeqTypeID;
        private bool _isMappedToRCAD;
        private bool _isLoadedToRCAD;
        private bool _mappingToRCAD;
        private bool _loadingToRCAD;
        private bool _loadingToRCADFailed;
        private string _mappingStatus;
        private string _loadingStatus;
        private SequenceAlignment _alignment;
        private BackgroundWorker _worker;
        private MTObservableCollection<SequenceViewModel> _sequences;
        private Mapper _alignmentToRCADMapping;

        private static string ALIGNMENT_NOT_MAPPED_MESSAGE = "Alignment Not Mapped to rCAD...";
        private static string MAPPING_ALIGNMENT_MESSAGE = "Mapping Alignment to rCAD...";
        private static string FINISHED_MAPPING_ALIGNMENT_MESSAGE = "Finished Mapping Alignment to rCAD...";
        private static string MAPPING_ALIGNMENT_FAILED_MESSAGE = "Failed Mapping Alignment to rCAD...";
        private static string LOADING_ALIGNMENT_MESSAGE = "Loading Alignment to rCAD...";
        private static string FINISHED_LOADING_ALIGNMENT_MESSAGE = "Finished Loading Alignment to rCAD...";
        private static string ALIGNMENT_NOT_LOADED_MESSAGE = "Alignment Not Loaded to rCAD...";
        private static string LOADING_ALIGNMENT_FAILED_MESSAGE = "Failed Loading Alignment to rCAD...";

        private void Initialize()
        {
            IsMappedToRCAD = false;
            IsLoadedToRCAD = false;
            MappingToRCAD = false;
            LoadingToRCAD = false;
            LoadingToRCADFailed = false;
            AlignmentID = 0;
            AlignmentSeqTypeID = 0;
            StructureModelCount = _alignment.Sequences.Count(s => s.Metadata.ContainsKey(SequenceMetadata.SequenceMetadataLabel) &&
                                                  ((SequenceMetadata)s.Metadata[SequenceMetadata.SequenceMetadataLabel]).StructureModel != null); ;
            RCADMappingStatus = ALIGNMENT_NOT_MAPPED_MESSAGE;
            RCADLoadingStatus = ALIGNMENT_NOT_LOADED_MESSAGE;
            foreach (var seq in _alignment.Sequences)
            {
                _sequences.Add(new SequenceViewModel(seq));
            }
            OnPropertiesChanged("SequenceCount", "MoleculeType", "GeneType", "GeneName");
        }

        private void MapAlignmentToRCAD(rCADConnection connection)
        {
            string connectionString = connection.BuildConnectionString();
            MappingToRCAD = true;
            RCADMappingStatus = MAPPING_ALIGNMENT_MESSAGE;
            CommandManager.InvalidateRequerySuggested();
            RCADMappingArgs args = new RCADMappingArgs() { ConnectionString = connectionString, Database = connection.Database };

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(MapAlignmentToRCADWorker);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MapAlignmentToRCADWorkerCompleted);
            _worker.RunWorkerAsync(args);
        }

        private void LoadAlignmentToRCAD(string targetDirectory)
        {
            LoadingToRCAD = true;
            RCADLoadingStatus = LOADING_ALIGNMENT_MESSAGE;
            CommandManager.InvalidateRequerySuggested();
            RCADLoadingArgs args = new RCADLoadingArgs() { Mapper = _alignmentToRCADMapping, TargetDirectory = targetDirectory };

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(LoadAlignmentToRCADWorker);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadAlignmentToRCADWorkerCompleted);
            _worker.RunWorkerAsync(args);
        }

        private void MapAlignmentToRCADWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _alignmentToRCADMapping = e.Result as Mapper;
            if (_alignmentToRCADMapping != null && _alignmentToRCADMapping.MappedSuccessfully)
            {
                IsMappedToRCAD = true;
                MappingToRCAD = false;
                RCADMappingStatus = FINISHED_MAPPING_ALIGNMENT_MESSAGE;
                AlignmentID = _alignmentToRCADMapping.AlignmentID;
                AlignmentSeqTypeID = _alignmentToRCADMapping.AlignmentSeqTypeID;
                CommandManager.InvalidateRequerySuggested();
                SendMessage(ViewMessages.MappedToRCAD, true); //We notify every sequence to pickup their rCAD Mapping info.
            }
            else
            {
                IsMappedToRCAD = false;
                MappingToRCAD = false;
                RCADMappingStatus = MAPPING_ALIGNMENT_FAILED_MESSAGE;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void MapAlignmentToRCADWorker(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null) return;
            RCADMappingArgs connArgs = e.Argument as RCADMappingArgs;
            if (connArgs != null)
            {
                Mapper m = new Mapper(_alignment, connArgs.ConnectionString, connArgs.Database);
                m.Map();
                e.Result = m;
            }
        }

        private void LoadAlignmentToRCADWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool? result = e.Result as bool?;
            if (result != null && result.Value)
            {
                IsLoadedToRCAD = true;
                LoadingToRCAD = false;
                RCADLoadingStatus = FINISHED_LOADING_ALIGNMENT_MESSAGE;
                CommandManager.InvalidateRequerySuggested();
            }
            else
            {
                IsLoadedToRCAD = false;
                LoadingToRCAD = false;
                LoadingToRCADFailed = true;
                RCADLoadingStatus = LOADING_ALIGNMENT_FAILED_MESSAGE;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void LoadAlignmentToRCADWorker(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null) return;
            RCADLoadingArgs args = e.Argument as RCADLoadingArgs;
            if (args.Mapper != null)
            {
                Loader l = new Loader(args.Mapper);
                if (!string.IsNullOrEmpty(args.TargetDirectory))
                {
                    e.Result = l.CreateBatchLoad(args.TargetDirectory);
                }
                else
                {
                    e.Result = l.Load();
                }
            }
        }
    }

    internal class RCADMappingArgs
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
    }

    internal class RCADLoadingArgs
    {
        public Mapper Mapper { get; set; }
        public string TargetDirectory { get; set; }
    }
}
