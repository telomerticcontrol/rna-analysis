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

using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Internal;
using JulMar.Windows.Mvvm;

namespace Bio.Views.Alignment.ViewModels
{
    /// <summary>
    /// This class provides a ViewModel abstraction for a single alignment within the view.
    /// </summary>
    [DebuggerDisplay("{Position}: {Header} IsLocked={IsLocked}, IsSelected={IsSelected}")]
    public class AlignmentEntityViewModel : SimpleViewModel
    {
        #region Private Data

        private readonly IAlignedBioEntity _entity;
        private int _displayIndex;
        private bool _isSelected, _isLocked, _isGroupHeader, _isSeparator, _isReference, _isFocused;
        private Brush _referenceSequenceColor, _referenceSequenceBorder;

        #endregion

        /// <summary>
        /// The parent (single) view model
        /// </summary>
        public AlignmentViewModel Parent { get; private set; }

        /// <summary>
        /// Position of the row in the parent collection
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Returns the reference sequence ID
        /// </summary>
        public int FirstDataIndex
        {
            get { return _entity.FirstDataColumn;  }    
        }

        /// <summary>
        /// Returns the taxonomy grouping level for a group header. -1 if this is not a group header.
        /// </summary>
        public int TaxonomyGroupLevel
        {
            get
            {
                return (IsGroupHeader) ? ((GroupHeader) _entity).TaxonomyLevel : -1;
            }
        }

        /// <summary>
        /// The display index (different from the Position if grouping is turned on.)
        /// </summary>
        public int DisplayIndex
        {
            get { return _displayIndex; }
            set 
            {
                if (_displayIndex != value)
                {
                    _displayIndex = value;
                    OnPropertyChanged("DisplayIndex", "DisplayIndexText");
                }
            }
        }

        /// <summary>
        /// Returns the taxonomy ID for the entity
        /// </summary>
        public string TaxonomyId
        {
            get { return _entity.Entity.TaxonomyId;  }
        }

        /// <summary>
        /// Returns the display index
        /// </summary>
        public string DisplayIndexText
        {
            get { return IsGroupHeader || IsSeparator ? string.Empty : _displayIndex.ToString(); }
        }

        /// <summary>
        /// Constructor for the separator version
        /// </summary>
        /// <param name="parent"></param>
        public AlignmentEntityViewModel(AlignmentViewModel parent)
        {
            Parent = parent;
            IsSeparator = true;
            Position = -1;
        }

        /// <summary>
        /// Constructor for a real BioEntity or Grouping row
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        /// <param name="pos"></param>
        public AlignmentEntityViewModel(AlignmentViewModel parent, IAlignedBioEntity entity, int pos)
        {
            Parent = parent;
            _entity = entity;
            Position = pos;
            DisplayIndex = -1;

            if (entity is GroupHeader)
                IsGroupHeader = true;
        }

        /// <summary>
        /// Header (name) displayed in UI
        /// </summary>
        public string Header 
        { 
            get
            {
                return _entity == null ? string.Empty : _entity.Entity.ScientificName;
            }
        }
        
        /// <summary>
        /// The actual nucelotide data
        /// </summary>
        public IList<IBioSymbol> AlignedData { get { return _entity.AlignedData; } }

        /// <summary>
        /// True if this row is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        /// <summary>
        /// True if this is the "focused" row
        /// </summary>
        public bool IsFocused
        {
            get { return _isFocused;  }
            set
            {
                _isFocused = value; 
                Debug.Assert(!_isFocused || Parent.FocusedRow == this);
                OnPropertyChanged("IsFocused");
            }
        }

        /// <summary>
        /// True if this item is row locked
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked || _isSeparator; }
            set { _isLocked = value; OnPropertyChanged("IsLocked", "DisplayIndexText"); }
        }

        /// <summary>
        /// True if this VM represents a grouping header
        /// </summary>
        public bool IsGroupHeader
        {
            get { return _isGroupHeader;  }
            set { _isGroupHeader = value; OnPropertyChanged("IsGroupHeader", "DisplayIndexText"); }
        }

        /// <summary>
        /// True if this VM represents a separator row
        /// </summary>
        public bool IsSeparator
        {
            get { return _isSeparator; }
            set { _isSeparator = value; OnPropertyChanged("IsSeparator", "DisplayIndexText"); }
        }

        /// <summary>
        /// True if this row is a reference sequence
        /// </summary>
        public bool IsReferenceSequence
        {
            get { return _isReference; }
            set { _isReference = value; OnPropertyChanged("IsReferenceSequence"); }
        }

        /// <summary>
        /// Reference sequence background color
        /// </summary>
        public Brush ReferenceSequenceColor
        {
            get { return _referenceSequenceColor; }
            set { _referenceSequenceColor = value; OnPropertyChanged("ReferenceSequenceColor"); }
        }

        /// <summary>
        /// Reference sequence border color
        /// </summary>
        public Brush ReferenceSequenceBorder
        {
            get { return _referenceSequenceBorder; }
            set { _referenceSequenceBorder = value; OnPropertyChanged("ReferenceSequenceBorder"); }
        }

        /// <summary>
        /// Simple property to return true when element represents a sequence
        /// </summary>
        public bool IsSequence
        {
            get { return !IsGroupHeader && !IsSeparator; }
        }
    }
}