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
using System.Linq;
using Bio.Data.Providers.Virtualization;
using System.Diagnostics;
using Bio.Data.Interfaces;

namespace Bio.Data.Providers.rCAD.RI.Models
{
    internal class DbAlignmentProvider : IVirtualizingListDataProvider<IBioSymbol>
    {
        private readonly int _id;
        private readonly int _aid;
        private readonly int _startingSequenceColumn;
        private readonly int _length;
        private readonly string _connectionString;

        public DbAlignmentProvider(rcadDataContext dc, string connectionString, int seqid, int alignmentid, int length)
        {
            //_connectionString = dc.Connection.ConnectionString;
            _connectionString = connectionString;
            _id = seqid;
            _aid = alignmentid;
            _length = length;
            //KJD (10/21/2009) - Change query to use vAlignmentGridUngapped which takes into account column indirection supported by the rCAD schema
            //instead of querying the Sequence table directly.
            _startingSequenceColumn = dc.vAlignmentGridUngappeds.Where(seq => ((seq.SeqID == _id) && (seq.AlnID == alignmentid))).Min(seq => seq.LogicalColumnNumber);
            Debug.Assert(_startingSequenceColumn > 0);
        }

        public int GetCount()
        {
            return _length;
        }

        public override string ToString()
        {
            return string.Format("RCAD Seq={0}, AId={1}", _id, _aid);
        }

        public IEnumerable<IBioSymbol> LoadRange(int startIndex, int count)
        {
            using (var dc = RcadSequenceProvider.CreateDbContext(_connectionString))
            {
                startIndex++;

                // select ColumnNumber, Base from sequence
                //    where seqid = @id and ColumnNumber >= @startIndex
                //    order by columnNumber
                //KJD (10/21/2009) - Change query to use vAlignmentGridUngapped which takes into account column indirection supported by the rCAD schema
                //we use the ungapped view to avoid transmitting gap characters from the db to the view (which would happen with vAlignmentGrid).
                var data = (from seq in dc.vAlignmentGridUngappeds
                            where seq.SeqID == _id && seq.AlnID == _aid && seq.LogicalColumnNumber >= startIndex
                            orderby seq.LogicalColumnNumber
                            select new {seq.LogicalColumnNumber, seq.BioSymbol}).Take(count);

                // Push back the requested count, fill in gaps in the sequence
                int i = 0;
                var ie = data.GetEnumerator();
                if (ie.MoveNext())
                {
                    var entry = ie.Current;

                    for (; i < count; i++)
                    {
                        int currentIndex = i + startIndex;

                        Debug.Assert(entry.LogicalColumnNumber >= currentIndex);
                        if (entry.LogicalColumnNumber == currentIndex)
                        {
                            Debug.Assert(_startingSequenceColumn <= currentIndex);

                            yield return new BioSymbol(BioSymbolType.Nucleotide, entry.BioSymbol);

                            if (!ie.MoveNext())
                                break;

                            entry = ie.Current;
                        }
                        else
                        {
                            if (_startingSequenceColumn > currentIndex)
                                yield return BioSymbol.None;
                            else
                                yield return BioSymbol.Gap;
                        }
                    }
                }

                // Pad to end
                for (; i < count; i++)
                    yield return BioSymbol.None;
            }
        }
    }
}