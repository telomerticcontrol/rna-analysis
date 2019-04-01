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
using Bio.Data;
using Bio.Data.Interfaces;

namespace Bio.Views.Alignment.Internal
{
    /// <summary>
    /// This class is used to provide a group heading when alignments are being grouped/sorted within the alignment viewer.
    /// To make it work within the viewer which is showing a sequence of IBioEntity objects, we implement the two requisite
    /// interfaces (IAlignedBioEntity and IBioEntity).
    /// </summary>
    public class GroupHeader : BioExtensiblePropertyBase, IAlignedBioEntity, IBioEntity
    {
        /// <summary>
        /// The entity being represented
        /// </summary>
        public IBioEntity Entity { get { return this; } }

        /// <summary>
        /// The first column in the data stream with data
        /// </summary>
        public int FirstDataColumn { get { return Int32.MaxValue; } }

        /// <summary>
        /// The common name for this alignment
        /// </summary>
        public string CommonName { get; set;}

        /// <summary>
        /// The taxonomy level (for display)
        /// </summary>
        public int TaxonomyLevel { get; set; }

        /// <summary>
        /// This returns the biological symbol information in aligned format.
        /// </summary>
        public IList<IBioSymbol> AlignedData { get { return null; } }

        /// <summary>
        /// Scientific name - this is displayed as the heading
        /// </summary>
        public string  ScientificName { get; set;}

        /// <summary>
        /// Taxonomy identifier - this is displayed as the ToolTip
        /// </summary>
        public string  TaxonomyId { get; set; }

        /// <summary>
        /// This represents the validator used to validate the entity
        /// </summary>
        public IBioValidator  Validator { get; private set;}

        /// <summary>
        /// Unaligned sequence of data - no data for a group heading.
        /// </summary>
        public IList<IBioSymbol>  RawData { get { return null; }}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="fullTaxonomyPath"></param>
        public GroupHeader(string headerText, string fullTaxonomyPath)
        {
            // We always display scientific name
            ScientificName = CommonName = headerText;
            TaxonomyId = fullTaxonomyPath;
        }
    }
}