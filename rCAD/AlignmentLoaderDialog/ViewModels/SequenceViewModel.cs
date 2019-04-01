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
using Bio;
using Alignment;
using Bio.IO.GenBank;
using System.Windows.Input;
using JulMar.Windows.Mvvm;
using AlignmentLoader;

namespace AlignmentLoaderDialog.ViewModels
{
    public class SequenceViewModel : ViewModel
    {
        public string ScientificName
        {
            get { return _metadata.ScientificName; }
        }

        public string RowLabel
        {
            get { return _metadata.AlignmentRowName; }
        }

        public int SequenceLength
        {
            get { return _metadata.SequenceLength; }
        }

        public string CellLocationDescription
        {
            get { return _metadata.LocationDescription; }
        }

        public List<GenBankVersion> GenBankIDs
        {
            get { return _metadata.Accessions; }
        }

        public int BasePairs
        {
            get { return (_metadata.StructureModel == null) ? 0 : _metadata.StructureModel.Pairs.Count(); }
        }

        public bool IsMappedToRCAD
        {
            get { return (_rcadMappingData == null) ? false : true; }
        }

        public int rCADSeqID
        {
            get { return _rcadMappingData.SeqID; }
        }

        public int rCADTaxID
        {
            get { return _rcadMappingData.TaxID; }
        }

        public int rCADLocationID
        {
            get { return _rcadMappingData.LocationID; }
        }

        public SequenceViewModel(ISequence sequence)
        {
            _sequence = sequence;
            Initialize();
        }

        private ISequence _sequence;
        private SequenceMetadata _metadata;
        private SequenceMappingData _rcadMappingData;

        private void Initialize()
        {
            bool retValue = RegisterWithMessageMediator();
            if (_sequence.Metadata.ContainsKey(SequenceMetadata.SequenceMetadataLabel))
            {
                _metadata = (SequenceMetadata)_sequence.Metadata[SequenceMetadata.SequenceMetadataLabel];
            }
            else //We'll add metadata to the sequence.
            {
                _metadata = new SequenceMetadata();
                _sequence.Metadata.Add(SequenceMetadata.SequenceMetadataLabel, _metadata);
            }
        }

        [MessageMediatorTarget(ViewMessages.MappedToRCAD)]
        private void OnMappedToRCAD(object parameter)
        {
            bool? mappedSuccessfully = parameter as bool?;
            if (mappedSuccessfully == null)
            {
                return;
            }

            if (_sequence.Metadata.ContainsKey(Mapper.rCADMappingData))
            {
                _rcadMappingData = (SequenceMappingData)_sequence.Metadata[Mapper.rCADMappingData];
                OnPropertiesChanged("IsMappedToRCAD", "rCADSeqID", "rCADTaxID", "rCADLocationID");
            }
        }

        [MessageMediatorTarget(ViewMessages.ClearRCADMapping)]
        private void OnClearMappingToRCAD(object parameter)
        {
            if (IsMappedToRCAD)
            {
                _rcadMappingData = null;
                _sequence.Metadata.Remove(Mapper.rCADMappingData);
                OnPropertiesChanged("IsMappedToRCAD", "rCADSeqID", "rCADTaxID", "rCADLocationID");
            }
        }
    }
}
