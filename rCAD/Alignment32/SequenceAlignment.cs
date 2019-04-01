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
using System.IO;
using System.Xml.Linq;
using Bio.IO;

namespace Alignment
{
    public class SequenceAlignment
    {
        public SequenceAlignment()
        {
            Sequences = new List<ISequence>();
        }

        public SequenceAlignment(List<ISequence> sequences)
        {
            Sequences = sequences;
        }

        #region Properties

        public string MoleculeType { get; set; }
        public string GeneType { get; set; }
        public string GeneName { get; set; }
        public string LogicalName { get; set; }

        private List<ISequence> _sequences;
        public List<ISequence> Sequences
        {
            get { return _sequences; }
            private set 
            { 
                _sequences = value;
                SetColumns();
                SetRows();
            }
        }

        private int _columns;
        public int Columns
        {
            get { return _columns; }
            private set { _columns = value; }
        }

        private int _rows;
        public int Rows
        {
            get { return _rows; }
            private set { _rows = value; }
        }

        #endregion

        public Dictionary<string, ISequenceItem> Column(int index)
        {
            if (index < 0 || index >= Columns) return new Dictionary<string, ISequenceItem>();
            return (from seq in _sequences
                    where index < seq.Count
                    select seq).ToDictionary(a => a.ID, a => a[index]);
        }

        public void DeleteColumn(int index)
        {
            var seqs = from seq in Sequences
                       where index < seq.Count
                       select seq;
            foreach (var s in seqs) { s.RemoveAt(index); }
            SetColumns();
        }

        public void DeleteRow(ISequence row)
        {
            Sequences.Remove(row);
            SetColumns();
            SetRows();
        }

        public void CompressColumns()
        {
            List<int> columnsToCompress = new List<int>();
            for (int i = 0; i < Columns; i++)
            {
                var colValues = Column(i);
                var colFilteredValues = colValues.Values.Where(s => !s.IsGap);
                if (colFilteredValues.Count() <= 0) columnsToCompress.Add(i);
            }

            int offset = 0;
            for (int j = 0; j < columnsToCompress.Count; j++)
            {
                DeleteColumn(columnsToCompress[j] - offset);
                offset++;
            }
        }

        #region Private Methods and Properties

        private void SetColumns()
        {
            if (_sequences == null) Columns = 0;
            else Columns = _sequences.Max(p => p.Count);
        }

        private void SetRows()
        {
            if (_sequences == null) Rows = 0;
            else Rows = _sequences.Count;
        }

        #endregion
    }
}
