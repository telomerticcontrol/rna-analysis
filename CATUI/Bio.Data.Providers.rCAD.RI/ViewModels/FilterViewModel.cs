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
using System.Collections.ObjectModel;
using JulMar.Windows;
using JulMar.Windows.Extensions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JulMar.Windows.Mvvm;
using Bio.Data.Providers.rCAD.RI.Models;
using JulMar.Windows.Validations;
using JulMar.Windows.Behaviors;
using System.Threading;
using System;
using System.Data.SqlClient;

namespace Bio.Data.Providers.rCAD.RI.ViewModels
{
    public class FilterViewModel : ValidatingViewModel
    {
        private int _step, _matchingSequenceCount = -1;
        private TaxonomyEntry _rootTaxonomy;
        private readonly AlignmentFilter _filter;

        /// <summary>
        /// User friendly name for the filter
        /// </summary>
        [Required]
        public string Name
        {
            get { return _filter.Name; }
            set { _filter.Name = value; OnPropertyChanged("Name"); }
        }

        /// <summary>
        /// Root taxononmy ID used for filter.  Only sequences under this ID will be retrieved.
        /// </summary>
        public int ParentTaxId
        {
            get { return _filter.ParentTaxId; }
            set
            {
                _filter.ParentTaxId = value;
                _matchingSequenceCount = -1;
                OnPropertyChanged("ParentTaxId", "IsValid", "MatchingSequenceCount", "HasValidSequenceList");
            }
        }

        /// <summary>
        /// RNA location identifier
        /// </summary>
        public int LocationId
        {
            get { return _filter.LocationId; }
            set
            {
                _filter.LocationId = value;
                _matchingSequenceCount = -1;
                OnPropertyChanged("LocationId", "MatchingSequenceCount", "HasValidSequenceList");
            }
        }

        /// <summary>
        /// Sequence type (5s, 16s, 23s)
        /// </summary>
        public int SequenceTypeId
        {
            get { return _filter.SequenceTypeId; }
            set
            {
                _filter.SequenceTypeId = value;
                _matchingSequenceCount = -1;
                _alignments = null;
                OnPropertyChanged("SequenceTypeId", "MatchingSequenceCount", "HasValidSequenceList", "Alignments");
            }
        }

        /// <summary>
        /// Alignment id
        /// </summary>
        public int AlignmentId
        {
            get { return _filter.AlignmentId; }    
            set
            {
                _filter.AlignmentId = value;
                _matchingSequenceCount = -1;
                OnPropertyChanged("AlignmentId", "MatchingSequenceCount", "HasValidSequenceList");
            }
        }

        /// <summary>
        /// The server name (.\SQLEXPRESS, (local), etc.)
        /// </summary>
        [Required]
        public string Server
        {
            get { return Connection.Server; }
            set
            {
                Connection.Server = value;
                OnPropertyChanged("Server");
                ResetConnectionDependencies();
                ThreadPool.QueueUserWorkItem(o => OnFillDatabases(false));
            }
        }

        /// <summary>
        /// Returns the total count of matching sequences found.
        /// </summary>
        public int MatchingSequenceCount
        {
            get
            {
                if (IsValid && _matchingSequenceCount == -1)
                {
                    using (var dc = RcadSequenceProvider.CreateDbContext(ConnectionString))
                    {
                        const string sqlQuery = @"with cte as (select TaxID from Taxonomy where TaxID={0} " +
                                                @"union all select t.TaxID from Taxonomy t join cte tp on t.ParentTaxID = tp.TaxID) " +
                                                @"select * from cte";
                        var results = dc
                            .ExecuteQuery<Taxonomy>(sqlQuery, ParentTaxId)
                            .Select(tax => tax.TaxID).ToList();

                        var queryGroups =
                            results.Select((id, index) => new {GroupID = index/2000, id}).GroupBy(x => x.GroupID);

                        _matchingSequenceCount = 0;
                        foreach (var singleGroup in queryGroups)
                        {
                            var thisGroup = singleGroup;
                            //KJD (10/21/2009) - Change query to use vAlignmentGridUngapped which takes into account column indirection supported by the rCAD schema
                            //instead of querying the Sequence table directly. We also now enforce that a specific alignment is used to determine sequence counts.
                            int count = (from seq in dc.SequenceMains
                                         where thisGroup.Select(x => x.id).Contains(seq.TaxID)
                                               && (LocationId == 0 || seq.LocationID == LocationId)
                                               && (seq.SeqTypeID == SequenceTypeId)
                                               && dc.vAlignmentGridUngappeds.Where(s => s.SeqID == seq.SeqID && s.AlnID == AlignmentId && s.LogicalColumnNumber > 0).FirstOrDefault() != null
                                         select seq.SeqID).Count();
                            _matchingSequenceCount += count;
                        }

                        OnPropertyChanged("HasValidSequenceList");

                        return _matchingSequenceCount;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Provides limitations on the sequence list.
        /// </summary>
        public bool HasValidSequenceList
        {
            get { return (_matchingSequenceCount > 0 && _matchingSequenceCount < 2000); }
        }

        private void ResetConnectionDependencies()
        {
            _rootTaxonomy = null;
            _seqTypes = null;
            _locationTypes = null;
            _matchingSequenceCount = -1;
            OnPropertyChanged("IsValid", "ConnectionString", "MatchingSequenceCount",
                "TaxonomyRoot", "SequenceTypes", "CellLocationTypes");
        }

        /// <summary>
        /// The database name on above server
        /// </summary>
        [Required]
        public string Database
        {
            get { return Connection.Database; }
            set
            {
                Connection.Database = value;
                OnPropertyChanged("Database");
                ResetConnectionDependencies();
            }
        }

        /// <summary>
        /// Returns if this is SQL security
        /// </summary>
        public bool IsSqlServerSecurity
        {
            get { return Connection.SecurityType == SecurityType.SqlAuthentication; }
            set
            {
                Connection.SecurityType = (value == true) ? SecurityType.SqlAuthentication : SecurityType.WindowsAuthentication;
                OnPropertyChanged("IsSqlServerSecurity", "IsWindowsSecurity");
                ResetConnectionDependencies();
            }
        }

        /// <summary>
        /// Returns if this is Windows security
        /// </summary>
        public bool IsWindowsSecurity
        {
            get { return Connection.SecurityType != SecurityType.SqlAuthentication; }
            set
            {
                Connection.SecurityType = value == false ? SecurityType.SqlAuthentication : SecurityType.WindowsAuthentication;
                OnPropertyChanged("IsSqlServerSecurity", "IsWindowsSecurity");
                ResetConnectionDependencies();
                ThreadPool.QueueUserWorkItem(o => OnFillDatabases(false));
            }
        }

        /// <summary>
        /// User name (if SecurityType = SQL)
        /// </summary>
        public string Username
        {
            get { return Connection.Username; }
            set
            {
                Connection.Username = value;
                OnPropertyChanged("Username");
                ResetConnectionDependencies();
            }
        }

        /// <summary>
        /// Password for SecurityType = SQL
        /// </summary>
        public string Password
        {
            get { return Connection.Password; }
            set
            {
                Connection.Password = value;
                OnPropertyChanged("Password");
                ResetConnectionDependencies();
            }
        }

        /// <summary>
        /// True to serialize connection
        /// </summary>
        private bool _shouldSerialize;
        public bool ShouldSerialize
        {
            get { return _shouldSerialize; }
            set { _shouldSerialize = value; OnPropertyChanged("ShouldSerialize"); }
        }

        /// <summary>
        /// Returns whether this connection is "valid"
        /// </summary>
        public bool IsValid
        {
            get { return _filter.IsValid; }
        }

        /// <summary>
        /// Returns the connection string
        /// </summary>
        public string ConnectionString
        {
            get { return Connection.BuildConnectionString(); }
        }

        /// <summary>
        /// Retrieve the Taxonomy information from the database.
        /// </summary>
        public IEnumerable<TaxonomyEntry> TaxonomyRoot
        {
            get
            {
                if (_rootTaxonomy == null && Connection.IsValid)
                    _rootTaxonomy = new TaxonomyEntry(RcadSequenceProvider.CreateDbContext(ConnectionString), true);
                yield return _rootTaxonomy;
            }
        }

        // Retrieve the RNA based sequence types
        private MTObservableCollection<KeyNameData> _seqTypes;
        public MTObservableCollection<KeyNameData> SequenceTypes
        {
            get
            {
                if (_seqTypes == null && Connection.IsValid)
                {
                    try
                    {
                        _seqTypes = new MTObservableCollection<KeyNameData>();

                        using (var dc = RcadSequenceProvider.CreateDbContext(ConnectionString))
                        {
                            _seqTypes.AddRange(from st in dc.SequenceTypes
                                               where st.MoleculeType == "RNA"
                                               orderby st.GeneType
                                               select new KeyNameData
                                                          {
                                                              Id = st.SeqTypeID,
                                                              Name = st.GeneType + " - " + st.GeneName
                                                          });
                        }

                        if (_filter.SequenceTypeId < 1 && _seqTypes.Count > 0)
                        {
                            var entry = (_seqTypes.FirstOrDefault(s => s.Name.StartsWith("Ribo")));
                            SequenceTypeId = (entry != null) ? entry.Id : _seqTypes[0].Id;
                        }
                    }
                    catch
                    {
                        _seqTypes = null;
                    }
                }
                return _seqTypes;
            }
        }

        // Retrieve the call location types
        private ObservableCollection<KeyNameData> _locationTypes;
        public ObservableCollection<KeyNameData> CellLocationTypes
        {
            get
            {
                if (_locationTypes == null && Connection.IsValid)
                {
                    try
                    {
                        _locationTypes = new ObservableCollection<KeyNameData>
                                             {new KeyNameData {Id = 0, Name = "All Cell Locations"}};

                        using (var dc = RcadSequenceProvider.CreateDbContext(ConnectionString))
                        {
                            _locationTypes.AddRange(from cl in dc.CellLocationInfos
                                                    orderby cl.Description
                                                    select new KeyNameData
                                                               {
                                                                   Id = cl.LocationID,
                                                                   Name = cl.Description
                                                               });
                        }

                        if (_filter.LocationId < 1)
                            LocationId = 0;
                    }
                    catch
                    {
                        _locationTypes = null;
                    }
                }
                return _locationTypes;
            }
        }

        /// <summary>
        /// Retrieve all the alignments
        /// </summary>
        private ObservableCollection<KeyNameData> _alignments;
        public ObservableCollection<KeyNameData> Alignments
        {
            get
            {
                if (_alignments == null && Connection.IsValid)
                {
                    try
                    {
                        _alignments = new ObservableCollection<KeyNameData>();
                        using (var dc = RcadSequenceProvider.CreateDbContext(ConnectionString))
                        {
                            _alignments.AddRange(from al in dc.Alignments
                                                 where al.SeqTypeID == SequenceTypeId
                                                 orderby al.AlignmentName
                                                 select new KeyNameData {
                                                        Id = al.AlnID,
                                                        Name = al.AlignmentName
                                                    });
                        }

                        if (_filter.AlignmentId < 0)
                            AlignmentId = _alignments[0].Id;
                    }
                    catch
                    {
                        _alignments = null;
                    }
                }
                return _alignments;
            }
        }

        /// <summary>
        /// Gets/Sets the current step in the creation process (1 or 2)
        /// </summary>
        public int CurrentStep
        {
            get { return _step; }
            set { _step = value; OnPropertyChanged("CurrentStep"); }
        }

        public MTObservableCollection<string> DatabaseNames { get; private set; }
        public ICommand FillDatabaseCommand { get; private set; }
        public ICommand SelectTaxonomyCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand PreviousCommand { get; private set; }

        public FilterViewModel()
        {
            _filter = new AlignmentFilter();
            _shouldSerialize = false;
            LocationId = 0;
            SequenceTypeId = 0;

            Initialize();
        }

        public FilterViewModel(AlignmentFilter filter, bool shouldSerialize)
        {
            _filter = filter;
            _shouldSerialize = shouldSerialize;
            Initialize();
        }

        private void Initialize()
        {
            CurrentStep = 1;
            DatabaseNames = new MTObservableCollection<string>();
            FillDatabaseCommand = new DelegatingCommand(() => OnFillDatabases(true));
            SelectTaxonomyCommand = new DelegatingCommand(OnTaxonomySelected);
            PreviousCommand = new DelegatingCommand(OnPreviousStep, () => CurrentStep == 2);
            NextCommand = new DelegatingCommand(OnNextStep, OnCanGotoNextStep);

            if (ParentTaxId != 0)
            {
                ThreadPool.QueueUserWorkItem(o =>
                 {
                     for (int i = 0; i < 2; i++)
                     {
                         try
                         {
                             LoadAndFindTaxonomy();
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
                 });
            }

            ThreadPool.QueueUserWorkItem(o => OnFillDatabases(false));
        }

        private void LoadAndFindTaxonomy()
        {
            var dc = RcadSequenceProvider.CreateDbContext(ConnectionString);
            _rootTaxonomy = new TaxonomyEntry(dc, true);

            // Get a list of the taxids leading up to the selected one.
            string sql = @"with cte as (select ParentTaxID from Taxonomy where TaxID={0} " +
                         @"union all select t.ParentTaxID from Taxonomy t join cte tp on t.TaxID = tp.ParentTaxID) " +
                         @"select * from cte";

            var taxIds = dc.ExecuteQuery<int>(sql, ParentTaxId).Reverse().ToList();
            taxIds.RemoveAt(0); // Remove root entry
            taxIds.Add(ParentTaxId);

            TaxonomyEntry current = _rootTaxonomy;

            foreach (int tid in taxIds)
            {
                // Skip root (level 0)
                if (tid <= 1)
                    continue;

                current.IsExpanded = true;
                foreach (var child in current.Children)
                {
                    if (child.Id == tid)
                    {
                        current = child;
                        break;
                    }
                }
            }

            if (current.Id == ParentTaxId)
                current.IsSelected = true;
            else
            {
                _rootTaxonomy = null;
                dc.Dispose();
            }
        }

        private void OnPreviousStep()
        {
            if (CurrentStep == 2)
                CurrentStep--;
        }

        private bool OnCanGotoNextStep()
        {
            return (CurrentStep == 1)
               ? !string.IsNullOrEmpty(Name) && Filter.Connection.IsValid
               : IsValid;
        }

        private void OnNextStep()
        {
            if (CurrentStep == 1)
            {
                CurrentStep++;
            }
            else
                RaiseCloseRequest(true);
        }

        internal AlignmentFilter Filter
        {
            get { return _filter; }    
        }

        private RcadConnection Connection
        {
            get { return _filter.Connection; }    
        }

        /// <summary>
        /// Fills the database selection combo box
        /// </summary>
        private void OnFillDatabases(bool showError)
        {
            try
            {
                lock (DatabaseNames)
                {
                    if (Connection.IsValid)
                    {
                        DatabaseNames.Clear();
                        foreach (var name in Connection.GetDatabaseList(showError))
                            DatabaseNames.Add(name);

                        if (string.IsNullOrEmpty(Database))
                        {
                            string dbName = DatabaseNames.FirstOrDefault(db => string.Compare(db, "rCAD", true) == 0);
                            if (dbName != null)
                                Database = dbName;
                        }
                    }
                }
            }
            catch
            {
                DatabaseNames.Clear();
                DatabaseNames.Add("rCAD");
                Database = DatabaseNames[0];
            }
        }

        /// <summary>
        /// This is used to detect changes in the taxonomy selection.
        /// We could
        /// </summary>
        /// <param name="parameter"></param>
        private void OnTaxonomySelected(object parameter)
        {
            EventParameters ep = (EventParameters) parameter;
            RoutedPropertyChangedEventArgs<object> e = (RoutedPropertyChangedEventArgs<object>) ep.EventArgs;
            TaxonomyEntry tw = (TaxonomyEntry) e.NewValue;
            if (tw != null)
                ParentTaxId =  tw.Id;
        }

        /// <summary>
        /// Disposes the view
        /// </summary>
        /// <param name="isDisposing"></param>
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_rootTaxonomy != null)
                {
                    _rootTaxonomy.Dispose();
                    _rootTaxonomy = null;
                }
            }

            base.Dispose(isDisposing);
        }
    }
}
