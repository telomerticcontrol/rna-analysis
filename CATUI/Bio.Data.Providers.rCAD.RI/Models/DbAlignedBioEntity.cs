using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bio.Data.Interfaces;
using Bio.Data.Providers.Virtualization;

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

namespace Bio.Data.Providers.rCAD.RI.Models
{
    /// <summary>
    /// This represents our alignment within the database.
    /// </summary>
    [DebuggerDisplay("{ScientificName}")]
    internal class DbAlignedBioEntity : BioExtensiblePropertyBase, IAlignedBioEntity, IBioEntity
    {
        private readonly VirtualizingList<IBioSymbol> _bioList;

        #region IBioEntity support

        public string ScientificName { get; set; }
        public string TaxonomyId { get; set; }
        public IBioValidator Validator { get; set; }
        public IList<IBioSymbol> RawData { get { return _bioList; } }

        #endregion

        #region IAlignedBioEntity support

        public IBioEntity Entity { get { return this; } }
        public string CommonName { get; set; }
        public int FirstDataColumn { get; private set; }

        public IList<IBioSymbol> AlignedData
        {
            get { return _bioList; }
        }

        #endregion

        /// <summary>
        /// Constructor for the rCAD BioEntity
        /// </summary>
        /// <param name="dc">Linq2Sql DataContext</param>
        /// <param name="seqid">Sequence ID (key)</param>
        /// <param name="alignmentid">Alignment ID (key)</param>
        /// <param name="length">Total length in bytes of the sequence</param>
        public DbAlignedBioEntity(rcadDataContext dc, string connectionString, int seqid, int alignmentid, int length)
        {
            // Create a virtualized list of the data
            _bioList = new VirtualizingList<IBioSymbol>(
                            new DbAlignmentProvider(dc, connectionString, seqid, alignmentid, length), 
                                Math.Min(512, length), 60);

            // Determine the first column with valid data
            //KJD (10/21/2009) - Change query to use vAlignmentGridUngapped which takes into account column indirection supported by the rCAD schema
            //instead of querying the Sequence table directly
            FirstDataColumn = dc.vAlignmentGridUngappeds.Where(s => ((s.SeqID == seqid) && (s.AlnID == alignmentid))).Min(s => s.LogicalColumnNumber) - 1;
        }
    }
}