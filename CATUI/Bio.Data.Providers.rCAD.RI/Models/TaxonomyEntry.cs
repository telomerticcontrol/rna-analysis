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
using System.Linq;
using JulMar.Windows;
using System.Data.SqlClient;

namespace Bio.Data.Providers.rCAD.RI.Models
{
    /// <summary>
    /// This class represents a single Taxonomy Entry from the RCAD database.
    /// </summary>
    public class TaxonomyEntry : KeyNameData, IDisposable
    {
        private static readonly TaxonomyEntry EmptyMarker = new TaxonomyEntry {Name = "(loading)", Id = 0};
        private bool _isExpanded, _isSelected;
        private readonly rcadDataContext _dc;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged("IsExpanded"); OnLoadChildren(); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value; 
                OnPropertyChanged("IsSelected");
            }
        }

        public MTObservableCollection<TaxonomyEntry> Children { get; private set; }

        private TaxonomyEntry()
        {
            Children = new MTObservableCollection<TaxonomyEntry> { EmptyMarker };
        }

        public TaxonomyEntry(rcadDataContext dc, bool isRoot) : this()
        {
            _dc = dc;

            if (isRoot)
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        LoadRootInfo();
                        IsExpanded = true;
                        break;
                    }
                    catch (SqlException)
                    {
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }

        private void LoadRootInfo()
        {
            Id = 0;
            Name = "Taxonomy Data";

            Children.Clear();
            var roots = from ti in _dc.TaxonomyNamesOrdereds
                        where ti.TaxID > 0 && ti.Level == 1
                        orderby ti.ScientificName
                        select new TaxonomyEntry(_dc, false)
                           {
                                Id = ti.TaxID,
                                Name = ti.ScientificName,
                           };
            foreach (var ti in roots)
                Children.Add(ti);
        }

        private void OnLoadChildren()
        {
            try
            {
                if (IsExpanded && Children.Count == 1 && Children[0] == EmptyMarker)
                {
                    Children.Clear();
                    var roots = from ti in _dc.TaxonomyNamesOrdereds
                                from tn in _dc.Taxonomies
                                where tn.ParentTaxID == Id && tn.TaxID > 0 &&
                                      ti.TaxID == tn.TaxID
                                orderby ti.ScientificName
                                select new TaxonomyEntry(_dc, false)
                                {
                                    Id = ti.TaxID,
                                    Name = ti.ScientificName,
                                };
                    foreach (var ti in roots)
                        Children.Add(ti);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        public void Dispose()
        {
            if (_dc != null && Id == 0)
                _dc.Dispose();
        }
    }
}
