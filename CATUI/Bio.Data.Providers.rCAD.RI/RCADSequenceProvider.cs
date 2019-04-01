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
using Bio.Data.Interfaces;
using Bio.Data.Providers.Interfaces;
using Bio.Data.Providers.rCAD.RI.Models;
using Bio.Data.Providers.rCAD.RI.Views;
using JulMar.Windows.Mvvm;
using JulMar.Windows.Interfaces;
using System.Diagnostics;
using Bio.Data.Providers.rCAD.RI.ViewModels;
using System.Configuration;
using System.Collections;

namespace Bio.Data.Providers.rCAD.RI
{
    class RcadSequenceProvider : IBioDataLoader<IAlignedBioEntity>, IDisposable, IBioDataLoaderProperties
    {
        const string SAVED_CONNECTION_FILE = @"Bio.Data.Providers.rCADRI.SaveDb.xml";
        public readonly static string RCADDI_SELECT_EXISTING_CONNECTION_UI = "RCADRI_selectExistingConnection";
        public readonly static string RCADRI_CREATE_CONNECTION_UI = "RCADRI_createConnection";

        private rcadDataContext _dc;
        private AlignmentFilter _dbConnection;
        private List<IAlignedBioEntity> _entities;

        /// <summary>
        /// Static constructor which registers UI dialogs.
        /// </summary>
        static RcadSequenceProvider()
        {
            IUIVisualizer uiVisualizer = ViewModel.ServiceProvider.Resolve<IUIVisualizer>();
            Debug.Assert(uiVisualizer != null);
            uiVisualizer.Register(RCADDI_SELECT_EXISTING_CONNECTION_UI, typeof(SelectExistingConnection));
            uiVisualizer.Register(RCADRI_CREATE_CONNECTION_UI, typeof(CreateNewConnection));
        }

        /// <summary>
        /// This is used to initialize the BioDataLoader when it is first created.
        /// </summary>
        /// <param name="initData">String data</param>
        /// <returns>True/False success</returns>
        public bool Initialize(string initData)
        {
            _dbConnection = PromptDbInfo();
            return _dbConnection != null && _dbConnection.IsValid;
        }

        /// <summary>
        /// This provides access to any initialization data used to create this loader.
        /// </summary>
        public string InitializationData
        {
            get { return _dbConnection.Name; }
        }

        /// <summary>
        /// This event should be raised by the loader when it changes properties
        /// that affect the sequence of data.
        /// </summary>
        public event EventHandler PropertiesChanged = delegate { };

        /// <summary>
        /// This method is used to change the properties of this loader.
        /// It is assumed that a GUI will be provided by the implementation.
        /// </summary>
        public bool ChangeProperties()
        {
            if (_dbConnection != null)
            {
                var fvm = new FilterViewModel(_dbConnection, true);
                IUIVisualizer uiVisualizer = ViewModel.ServiceProvider.Resolve<IUIVisualizer>();
                Debug.Assert(uiVisualizer != null);
                if (uiVisualizer.ShowDialog(RCADRI_CREATE_CONNECTION_UI, fvm) == true)
                {
                    // TODO: should save changes to the file..
                    Load();
                    PropertiesChanged(this, EventArgs.Empty);
                    return true;
                }
            }
            return false;
        }

        private static AlignmentFilter PromptDbInfo()
        {
            IUIVisualizer uiVisualizer = ViewModel.ServiceProvider.Resolve<IUIVisualizer>();
            Debug.Assert(uiVisualizer != null);

            var savedConnections = AlignmentFilter.Load(SAVED_CONNECTION_FILE);
            using (var cvm = new FilterListViewModel(savedConnections))
            {
                try
                {
                    if (savedConnections.Count > 0)
                    {
                        if (uiVisualizer.ShowDialog(RCADDI_SELECT_EXISTING_CONNECTION_UI, cvm) == true)
                            return cvm.SelectedFilter.Filter;
                    }
                    else
                    {
                        var vm = new FilterViewModel();
                        if (uiVisualizer.ShowDialog(RCADRI_CREATE_CONNECTION_UI, vm) == true)
                        {
                            cvm.Filters.Add(vm);
                            cvm.SelectedFilter = vm;
                            return vm.Filter;
                        }
                    }
                    return null;
                }
                finally
                {
                    AlignmentFilter.Save(SAVED_CONNECTION_FILE, cvm.Filters
                                                                    .Where(vm => vm.ShouldSerialize)
                                                                    .Select(vm => vm.Filter));
                }
            }
        }

        internal static rcadDataContext CreateDbContext(string connectionString)
        {
            var dc = new rcadDataContext(connectionString);
            //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RcadDbLog"]))
                dc.Log = Console.Out;
            return dc;
        }

        public int Load()
        {
            string dbConnectionString = _dbConnection.Connection.BuildConnectionString();
            try
            {
                _dc = CreateDbContext(dbConnectionString);

                // Retrieve all the TAXIDs we want.  We essentially want all taxonomy IDs
                // which are parented by the selected id.  The easiest way to do this is with a
                // CTE using SQL2005+.  Unfortunately, LINQ doesn't support that so we drop to straight
                // T-SQL for this.
                const string sqlQuery = @"with cte as (select TaxID from Taxonomy where TaxID={0} " +
                                        @"union all select t.TaxID from Taxonomy t join cte tp on t.ParentTaxID = tp.TaxID) " +
                                        @"select * from cte";

                var results = _dc
                    .ExecuteQuery<Taxonomy>(sqlQuery, _dbConnection.ParentTaxId)
                    .Select(tax => tax.TaxID).ToList();

                // BUGBUG: if the # of sequences is > 2100 then we cannot generate the proper
                // query here since T-SQL doesn't support that many parameters.  So, here we'll group
                // it in batches of 2000.

                var queryGroups = results.Select((id, index) => new {GroupID = index/2000, id}).GroupBy(x => x.GroupID);

                // Now, grab all the sequences from this group of TaxIds.
                // select distinct seqid from dbo.sequence where alnid = @alignment_id
                //KJD (10/21/2009) - Change query to use vAlignmentGridUngapped which takes into account column indirection supported by the rCAD schema
                //instead of querying the Sequence table directly
                var allSequences = new List<int>();
                foreach (var singleGroup in queryGroups)
                {
                    var thisGroup = singleGroup;
                    var query = from seq in _dc.SequenceMains
                                where thisGroup.Select(x => x.id).Contains(seq.TaxID)
                                      && _dc.vAlignmentGridUngappeds.Where(s => ((s.SeqID == seq.SeqID) && (s.AlnID == _dbConnection.AlignmentId))).FirstOrDefault() != null
                                      && seq.TaxID > 0
                                      && seq.SeqLength > 0
                                      && (_dbConnection.LocationId == 0 || seq.LocationID == _dbConnection.LocationId)
                                      && (seq.SeqTypeID == _dbConnection.SequenceTypeId)
                                select seq.SeqID;
                    allSequences.AddRange(query.Cast<int>());
                }

                // Now get the sequence headers based on the above collection
                queryGroups = allSequences.Distinct().Select((id, index) => new {GroupID = index/1000, id}).GroupBy(x => x.GroupID);
                _entities = new List<IAlignedBioEntity>();

                // 2 minute timeout
                _dc.CommandTimeout = 60*2*1000;

                // Now execute the queries
                //KJD (10/21/2009) - Change query to use vAlignmentGridUngapped which takes into account column indirection supported by the rCAD schema
                //instead of querying the Sequence table directly
                foreach (var singleGroup in queryGroups)
                {
                    var thisGroup = singleGroup;
                    var query = from smd in _dc.SequenceMains
                                from tax in _dc.TaxonomyNamesOrdereds
                                let maxColumnNumber = _dc.vAlignmentGridUngappeds.Where(s => ((s.SeqID == smd.SeqID) && (s.AlnID == _dbConnection.AlignmentId))).Max(s => s.LogicalColumnNumber)
                                where thisGroup.Select(x => x.id).Contains(smd.SeqID)
                                      && tax.TaxID == smd.TaxID
                                select new DbAlignedBioEntity(_dc, dbConnectionString, smd.SeqID, _dbConnection.AlignmentId, maxColumnNumber)
                                           {
                                               ScientificName = tax.ScientificName,
                                               TaxonomyId = tax.LineageName,
                                               Validator = BioValidator.rRNAValidator
                                           };

                    // Add the results to our list.
                    _entities.AddRange(query.Cast<IAlignedBioEntity>());
                }
                return _entities.Count;
            }
            catch (Exception ex)
            {
                IErrorVisualizer errorVisualizer = ViewModel.ServiceProvider.Resolve<IErrorVisualizer>();
                if (errorVisualizer != null)
                {
                    errorVisualizer.Show("Encountered error loading data", ex.Message);
                }

                _dc.Dispose();
                _dc = null;
            }

            return 0;
        }

        public IList<IAlignedBioEntity> Entities
        {
            get { return _entities; }
        }

        IList IBioDataLoader.Entities { get { return _entities; } }

        public void Dispose()
        {
            _dc.Dispose();
            // Wipe the properties changed list.
            PropertiesChanged = delegate { };
        }
    }
}
