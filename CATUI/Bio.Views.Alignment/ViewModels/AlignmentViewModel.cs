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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Bio.Data;
using Bio.Data.Interfaces;
using System.Diagnostics;
using Bio.Data.Providers.Interfaces;
using Bio.Views.Alignment.Controls;
using Bio.Views.Alignment.Internal;
using Bio.Views.Alignment.Views;
using Bio.Views.ViewModels;
using JulMar.Windows;
using JulMar.Windows.Extensions;
using JulMar.Windows.Interfaces;
using JulMar.Windows.Mvvm;
using Bio.Views.Alignment.Options;

namespace Bio.Views.Alignment.ViewModels
{
    /// <summary>
    /// This is the primary view model for the alignment viewer
    /// </summary>
    public class AlignmentViewModel : BioViewModel<AlignmentEditor>
    {
        #region Private Data

        private readonly MenuItem _groupByMenuItem, _splitScreenMenuItem;
        private bool _supportsGrouping, _isScreenSplit, _isGrouped;
        private static bool _isBevShowing;
        private int _focusColumnIndex, _columns, _topRow, _visibleRows;
        private double _rightPaneSize;
        private AlignmentEntityViewModel _focusedRow;
        private IBioDataLoader<IAlignedBioEntity> _data;
        private FindSearchViewModel _finder;
        private TaxonomyJumpViewModel _taxonomyJumper;
        private BirdsEyeViewModel _beView;

        private readonly List<IAlignedBioEntity> _groupedList = new List<IAlignedBioEntity>();
        private readonly IList<AlignmentEntityViewModel> _lockedRows = new ObservableCollection<AlignmentEntityViewModel>();
        private readonly MTObservableCollection<AlignmentEntityViewModel> _visibleData = new MTObservableCollection<AlignmentEntityViewModel>();

        private readonly SplitPaneViewModel _leftView;
        private readonly SplitPaneViewModel _rightView;
        private SplitPaneViewModel _activeView;

        #endregion

        /// <summary>
        /// List of currently "locked" rows
        /// </summary>
        public IList<AlignmentEntityViewModel> LockedRows
        {
            get { return _lockedRows; }    
        }

        /// <summary>
        /// True if we have locked/refseq rows
        /// </summary>
        public bool HasLockedRows
        {
            get { return _lockedRows.Count > 0; }
        }

        /// <summary>
        /// This returns the visible rows (things the user can see).
        /// </summary>
        public IList<AlignmentEntityViewModel> VisibleData
        {
            get { return _visibleData;  }    
        }

        /// <summary>
        /// Returns whether the data may be grouped
        /// </summary>
        public bool SupportsGrouping
        {
            get { return _supportsGrouping; }
            set { _supportsGrouping = value; OnPropertyChanged("SupportsGrouping"); }
        }

        /// <summary>
        /// Returns whether the data is grouped
        /// </summary>
        public bool IsGrouped
        {
            get 
            {
                return _isGrouped; 
            }

            set
            {
                bool oldValue = _isGrouped;
                _isGrouped = (SupportsGrouping == false) ? false : value;
                if (_isGrouped != oldValue)
                {
                    _groupByMenuItem.IsChecked = _isGrouped;
                    ReloadSequences();
                    OnPropertyChanged("IsGrouped");
                }
            }
        }

        /// <summary>
        /// Birds-Eye Viewer
        /// </summary>
        public bool IsBevShowing
        {
            get { return _isBevShowing; }    
            set
            {
                _isBevShowing = value;
                OnPropertyChanged("IsBevShowing");
            }
        }

        /// <summary>
        /// True/False if we are viewing two side-by-side alignment views
        /// </summary>
        public bool IsScreenSplit
        {
            get { return _isScreenSplit; }
            set
            {
                _isScreenSplit = value;
                SetActiveView(_leftView);
                OnPropertyChanged("IsScreenSplit", "RightSidePaneSize");
                _splitScreenMenuItem.IsChecked = _isScreenSplit;
            }
        }

        /// <summary>
        /// Gets or sets the TotalColumns property.
        /// </summary>
        public int TotalColumns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("TotalColumns"); }
        }

        /// <summary>
        /// Gets or sets the TotalRows property.
        /// </summary>
        public int TotalRows
        {
            get { return GroupedEntities.Count; } 
        }

        /// <summary>
        /// Visible rows - used by the BirdsEyeViewer
        /// </summary>
        public int VisibleRows
        {
            get { return _visibleRows; }
            set 
            { 
                _visibleRows = value;
                if (_beView != null)
                    _beView.VisibleRows = _visibleRows;
                OnPropertyChanged("VisibleRows");
            }
        }

        /// <summary>
        /// Count of data entities (excluding grouping)
        /// </summary>
        public int TotalEntities
        {
            get { return _data.Entities.Count; }
        }

        /// <summary>
        /// The current (index) of the first visible row
        /// </summary>
        public int TopRow
        {
            get { return _topRow; }    
            set
            {
                if (_topRow == value)
                    return;

                if (_topRow < 0)
                    throw new ArgumentOutOfRangeException("TopRow");

                _topRow = value;
                OnPropertyChanged("TopRow");

                if (_beView != null)
                    _beView.TopRow = _topRow;
            }
        }

        /// <summary>
        /// Color selection service for nucelotides.
        /// </summary>
        public SequenceColorSelector NucleotideColorSelector { get; private set; }

        /// <summary>
        /// This returns the selected reference sequences
        /// </summary>
        public IList<AlignmentEntityViewModel> SelectedReferenceSequences
        {
            get { return _lockedRows; }
        }

        /// <summary>
        /// Returns whether there is at least one reference sequence
        /// </summary>
        public bool HasReferenceSequence
        {
            get { return SelectedReferenceSequences.Count > 0; }    
        }

        /// <summary>
        /// Returns the logical index (column) of the focused element.  -1 if none. This value is zero-based.
        /// </summary>
        public int FocusedColumnIndex
        {
            get { return _focusColumnIndex; }
            set
            {
                _focusColumnIndex = value; 
                OnPropertyChanged("FocusedColumnIndex", "FocusedColumnReferenceIndex");
                _leftView.OnFocusedColumnIndexChanged();
                _rightView.OnFocusedColumnIndexChanged();
            }
        }

        /// <summary>
        /// Returns the Focused Column reference index, null if none. This value is 1-based.
        /// </summary>
        public string FocusedColumnReferenceIndex
        {
            get
            {
                if (!HasReferenceSequence || FocusedColumnIndex == -1)
                    return string.Empty;

                var rs = SelectedReferenceSequences[0];
                if (rs.AlignedData[FocusedColumnIndex].Type != BioSymbolType.Nucleotide)
                    return "-";

                // If the focused column index is less than the first data index then return empty.
                if ((FocusedColumnIndex - rs.FirstDataIndex) <= 0)
                    return string.Empty;

                return (1 + Enumerable.Range(rs.FirstDataIndex, FocusedColumnIndex-rs.FirstDataIndex)
                                .Select(i => rs.AlignedData[i])
                                .Where(bs => bs.Type == BioSymbolType.Nucleotide)
                                .Count()).ToString();
            }
        }

        /// <summary>
        /// This returns the "focused" selected row
        /// </summary>
        public AlignmentEntityViewModel FocusedRow
        {
            get { return _focusedRow; }
            set
            {
                if (_focusedRow == value)
                    return;

                if (_focusedRow != null)
                    _focusedRow.IsFocused = false;

                _focusedRow = value;
                if (_focusedRow != null)
                    _focusedRow.IsFocused = true;

                OnPropertyChanged("FocusedRow", "FocusedRowDisplayIndex");
            }
        }

        /// <summary>
        /// Returns the display index of the focused row (i.e. position within Entity data)
        /// </summary>
        public int? FocusedRowDisplayIndex
        {
            get { return FocusedRow != null ? (int?) FocusedRow.DisplayIndex : null; }
        }

        /// <summary>
        /// The changable options
        /// </summary>
        public RuntimeOptionsViewModel Options { get; private set; }

        public SplitPaneViewModel LeftView { get { return _leftView; } }
        public SplitPaneViewModel RightView { get { return _rightView; } }
        public SplitPaneViewModel ActiveView { get { return _activeView; } }

        /// <summary>
        /// Retrieves the right side pane size
        /// </summary>
        public double RightSidePaneSize
        {
            get
            {
                if (!_isScreenSplit)
                    return 0.0;

                return _rightPaneSize;
            }
            set 
            { 
                _rightPaneSize = value;
                OnPropertyChanged("RightSidePaneSize"); 
            }
        }

        #region Commands
        /// <summary>
        /// Command to split the screen
        /// </summary>
        public ICommand SplitScreenCommand { get; private set; }

        /// <summary>
        /// Command to change the reference sequence colors used
        /// </summary>
        public ICommand ShowOptionsCommand { get; private set; }

        /// <summary>
        /// This selects an alignment as a reference sequence
        /// </summary>
        public ICommand MakeReferenceSequence { get; private set; }

        /// <summary>
        /// Turns grouping on and off in the UI
        /// </summary>
        public ICommand FlipGrouping { get; private set; }

        /// <summary>
        /// Positions the view to a specific column
        /// </summary>
        public ICommand GotoColumn { get; private set; }

        /// <summary>
        /// Positions the view to a specific reference column
        /// </summary>
        public ICommand GotoRefColumn { get; private set; }

        /// <summary>
        /// Positions the view to a given row
        /// </summary>
        public ICommand GotoRow { get; private set; }

        /// <summary>
        /// Finds a specific entity by name
        /// </summary>
        public ICommand FindByName { get; private set; }

        /// <summary>
        /// Find an entity by taxonomy
        /// </summary>
        public ICommand FindByTaxonomy { get; private set; }

        /// <summary>
        /// Jump to the next nucleotide
        /// </summary>
        public ICommand JumpNextNucleotide { get; private set; }

        /// <summary>
        /// Jump to the next nucleotide
        /// </summary>
        public ICommand JumpPreviousNucleotide { get; private set; }

        /// <summary>
        /// Jump to the next reference sequence nucleotide
        /// </summary>
        public ICommand JumpNextRefNucleotide { get; private set; }

        /// <summary>
        /// Jump to the next reference sequence nucleotide
        /// </summary>
        public ICommand JumpPreviousRefNucleotide { get; private set; }

        /// <summary>
        /// Shift to the left
        /// </summary>
        public ICommand ShiftFocusLeft { get; private set; }

        /// <summary>
        /// Shift to the right
        /// </summary>
        public ICommand ShiftFocusRight { get; private set; }

        /// <summary>
        /// Show/Hide the Birds-Eye Viewer
        /// </summary>
        public ICommand ShowBirdsEyeView { get; private set; }
        #endregion

        /// <summary>
        /// Title for the window
        /// </summary>
        public override string Title
        {
            get 
            {
                string baseTitle = base.Title;
                if (string.IsNullOrEmpty(baseTitle))
                    baseTitle = _data.InitializationData;
                
                return string.Format("Alignment View: {0}, {1} sequences", baseTitle, (_data != null) ? _data.Entities.Count : 0); 
            }    
            set { base.Title = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AlignmentViewModel()
        {
            Options = new RuntimeOptionsViewModel();

            _isScreenSplit = false;
            _rightPaneSize = Double.PositiveInfinity;

            SplitScreenCommand = new DelegatingCommand(() => IsScreenSplit = !IsScreenSplit);
            ShowOptionsCommand = new DelegatingCommand(OnShowOptions);
            ShowBirdsEyeView = new DelegatingCommand(OnShowBev);
            MakeReferenceSequence = new DelegatingCommand<AlignmentEntityViewModel>(OnMakeReferenceSequence);
            FlipGrouping = new DelegatingCommand(() => IsGrouped = !IsGrouped, () => SupportsGrouping);
            GotoColumn = new DelegatingCommand(OnGotoColumn);
            GotoRow = new DelegatingCommand(OnGotoRow);
            GotoRefColumn = new DelegatingCommand(OnGotoRefColumn, () => HasReferenceSequence);
            FindByName = new DelegatingCommand(OnFindByName);
            JumpNextNucleotide = new DelegatingCommand(OnGotoNextNucleotide, () => FocusedRow != null && FocusedColumnIndex < TotalColumns-1);
            JumpPreviousNucleotide = new DelegatingCommand(OnGotoPreviousNucleotide, () => FocusedRow != null && FocusedColumnIndex > 0);
            JumpNextRefNucleotide = new DelegatingCommand(OnGotoNextRefNucleotide, () => FocusedRow != null && FocusedColumnIndex < TotalColumns - 1 && HasReferenceSequence);
            JumpPreviousRefNucleotide = new DelegatingCommand(OnGotoPreviousRefNucleotide, () => FocusedRow != null && FocusedColumnIndex > 0 && HasReferenceSequence);
            ShiftFocusLeft = new DelegatingCommand(OnShiftFocusLeft, () => FocusedRow != null);
            ShiftFocusRight = new DelegatingCommand(OnShiftFocusRight, () => FocusedRow != null);
            FindByTaxonomy = new DelegatingCommand(OnFindByTaxonomy, () => IsGrouped);

            OptionsMenu.Add(new MenuItem("Change _View Options") {Command = ShowOptionsCommand, GestureText="Ctrl+O"});
            _groupByMenuItem = new MenuItem("Group By Taxonomy") { Command = FlipGrouping, IsChecked = IsGrouped, GestureText = "Ctrl+G" };
            OptionsMenu.Add(_groupByMenuItem);

            _splitScreenMenuItem = new MenuItem("Split Screen View") {Command = SplitScreenCommand, IsChecked = IsScreenSplit, GestureText="Ctrl+S"};
            ViewMenu.Add(_splitScreenMenuItem);
            ViewMenu.Add(new MenuItem("_Taxonomy Browser") { Command = FindByTaxonomy, GestureText = "Ctrl+T" });
            ViewMenu.Add(new MenuItem("Birds-Eye Viewer") { Command = ShowBirdsEyeView, GestureText = "Ctrl+B" });

            EditMenu.Add(new MenuItem("Jump To _Column") {Command = GotoColumn, GestureText="Ctrl+C"});
            EditMenu.Add(new MenuItem("Jump To _Row") { Command = GotoRow, GestureText = "Ctrl+R" });
            EditMenu.Add(new MenuItem());
            EditMenu.Add(new MenuItem("Jump To Previous Nucelotide") { Command = JumpPreviousNucleotide, GestureText="F2" });
            EditMenu.Add(new MenuItem("Jump To Next Nucelotide") { Command = JumpNextNucleotide, GestureText="F3" });
            EditMenu.Add(new MenuItem());
            EditMenu.Add(new MenuItem("Jump To Reference Nucelotide") { Command = GotoRefColumn, GestureText="Ctrl+N" });
            EditMenu.Add(new MenuItem("Jump To Previous Reference Nucelotide") { Command = JumpPreviousRefNucleotide, GestureText = "Ctrl+F2" });
            EditMenu.Add(new MenuItem("Jump To Next Reference Nucelotide") { Command = JumpNextRefNucleotide, GestureText = "Ctrl+F3" });
            EditMenu.Add(new MenuItem());
            EditMenu.Add(new MenuItem("Find By _Name") { Command = FindByName, GestureText = "Ctrl+F" });

            NucleotideColorSelector = new NucleotideColorSelector(this, Options);
            _leftView = new SplitPaneViewModel(this);
            _rightView = new SplitPaneViewModel(this);

            // Set the intial view
            SetActiveView(_leftView);
        }

        /// <summary>
        /// Initialize globals
        /// </summary>
        static AlignmentViewModel()
        {
            IUIVisualizer uiVisualizer = ServiceProvider.Resolve<IUIVisualizer>();
            if (uiVisualizer != null)
            {
                uiVisualizer.Register(typeof(OptionsWindow).ToString(), typeof(OptionsWindow));
                uiVisualizer.Register(typeof(GotoColumnRowView).ToString(), typeof(GotoColumnRowView));
                uiVisualizer.Register(typeof(GotoRefSeqColumnView).ToString(), typeof(GotoRefSeqColumnView));
                uiVisualizer.Register(typeof(FindSearchView).ToString(), typeof(FindSearchView));
            }
        }

        /// <summary>
        /// This initializes the view model and generates the appropriate view.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Initialize(IBioDataLoader data)
        {
            _data = data as IBioDataLoader<IAlignedBioEntity>;
            Debug.Assert(_data != null);
            
            if (_data == null)
                return false;

            IBioDataLoaderProperties bioProps = data as IBioDataLoaderProperties;
            if (bioProps != null)
                bioProps.PropertiesChanged += OnLoaderPropertiesChanged;

            var allData = _data.Entities;
            TotalColumns = allData != null && allData.Count > 0
                               ? allData.Max(seq => (seq.AlignedData != null) ? seq.AlignedData.Count : 0)
                               : 0;

            ReloadSequences();
            FocusedRow = VisibleData[0];

            if (SupportsGrouping && Options.OpenWithGrouping)
                IsGrouped = true;

            return base.Initialize(data);
        }

        /// <summary>
        /// This targets a view for commands.
        /// </summary>
        /// <param name="view"></param>
        internal void SetActiveView(SplitPaneViewModel view)
        {
            _activeView = view;
        }

        /// <summary>
        /// This turns the Birds-Eye-Viewer on and off
        /// </summary>
        private void OnShowBev()
        {
            IsBevShowing = !IsBevShowing;
            BirdsEyeViewModel bev = BirdsEyeViewModel;
            if (IsBevShowing)
            {
                bev.Show();
                bev.SetActiveViewModel(this);
            }
            else
            {
                bev.CloseView.Execute(null);
            }
        }

        /// <summary>
        /// This method displays the Options dialog for the view
        /// </summary>
        private void OnShowOptions()
        {
            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            if (uiVisualizer != null)
            {
                int groupingMinMaxLevel = Options.MinGroupingRange;
                bool sortByTaxonomy = Options.SortByTaxonomy;
                var displayOptions = new DisplayOptionsViewModel();
                bool? rc = uiVisualizer.ShowDialog(typeof (OptionsWindow).ToString(), displayOptions);
                if (rc == true)
                {
                    Options.BroadcastOptionChange();
                    if (groupingMinMaxLevel != Options.MinGroupingRange || sortByTaxonomy != Options.SortByTaxonomy)
                    {
                        ReloadSequences();
                    }
                }
            }
        }

        /// <summary>
        /// Jumps to the next nucleotide in the current sequence
        /// </summary>
        private void OnGotoNextNucleotide()
        {
            Debug.Assert(FocusedRow != null);
            int newPos = Enumerable.Range(FocusedColumnIndex+1, TotalColumns - FocusedColumnIndex - 1)
                                   .FirstOrDefault(i => FocusedRow.AlignedData[i].Type == BioSymbolType.Nucleotide);
            if (newPos > FocusedColumnIndex)
            {
                FocusedColumnIndex = newPos;
                int currentRange = (_activeView.FirstColumn + _activeView.VisibleColumns)-1;
                if (newPos > currentRange)
                    _activeView.FirstColumn = newPos - _activeView.VisibleColumns;
            }
        }

        /// <summary>
        /// Jumps to the previous nucleotide in the first reference sequence
        /// </summary>
        private void OnGotoPreviousRefNucleotide()
        {
            Debug.Assert(FocusedRow != null);
            Debug.Assert(SelectedReferenceSequences.Count > 0);
            
            int newPos = Enumerable.Range(0, FocusedColumnIndex).Reverse()
                                   .FirstOrDefault(i => SelectedReferenceSequences[0].AlignedData[i].Type == BioSymbolType.Nucleotide);
            if (newPos >= 0 && newPos < FocusedColumnIndex)
            {
                FocusedColumnIndex = newPos;
                if (newPos < _activeView.FirstColumn)
                    _activeView.FirstColumn = newPos;
            }
        }

        /// <summary>
        /// Jumps to the next nucleotide in the first reference sequence
        /// </summary>
        private void OnGotoNextRefNucleotide()
        {
            Debug.Assert(FocusedRow != null);
            Debug.Assert(SelectedReferenceSequences.Count > 0);

            int newPos = Enumerable.Range(FocusedColumnIndex + 1, TotalColumns - FocusedColumnIndex - 1).FirstOrDefault(
                i => SelectedReferenceSequences[0].AlignedData[i].Type == BioSymbolType.Nucleotide);
            if (newPos > FocusedColumnIndex)
            {
                FocusedColumnIndex = newPos;
                int currentRange = (_activeView.FirstColumn + _activeView.VisibleColumns) - 1;
                if (newPos > currentRange)
                    _activeView.FirstColumn = newPos - _activeView.VisibleColumns;
            }
        }

        /// <summary>
        /// Jumps to the previous nucleotide in the current sequence
        /// </summary>
        private void OnGotoPreviousNucleotide()
        {
            Debug.Assert(FocusedRow != null);
            int newPos = Enumerable.Range(0, FocusedColumnIndex).Reverse().FirstOrDefault(
                i => FocusedRow.AlignedData[i].Type == BioSymbolType.Nucleotide);
            if (newPos >= 0 && newPos < FocusedColumnIndex)
            {
                FocusedColumnIndex = newPos;
                if (newPos < _activeView.FirstColumn)
                    _activeView.FirstColumn = newPos;
            }
        }

        /// <summary>
        /// Displays the "goto column" dialog and positions the view
        /// </summary>
        private void OnGotoColumn()
        {
            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            if (uiVisualizer != null)
            {
                var vm = new GotoColumnRowViewModel
                 {
                     Type = "Column",
                     MinPosition =  1,
                     Position = 1 + _activeView.FirstColumn,
                     MaxPosition = TotalColumns,
                 };
                bool? rc = uiVisualizer.ShowDialog(typeof (GotoColumnRowView).ToString(), vm);
                if (rc == true)
                {
                    FocusedColumnIndex = _activeView.FirstColumn = vm.Position - 1;
                }
            }
        }

        /// <summary>
        /// This method scrolls vertically
        /// </summary>
        private void OnGotoRow()
        {
            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            if (uiVisualizer != null)
            {
                var vm = new GotoColumnRowViewModel
                {
                    Type = "Row",
                    MinPosition = 1,
                    Position = 1,
                    MaxPosition = TotalEntities,
                };
                bool? rc = uiVisualizer.ShowDialog(typeof(GotoColumnRowView).ToString(), vm);
                if (rc == true)
                {
                    int realIndex = Enumerable.Range(0, TotalRows).First(i => VisibleData[i].DisplayIndex == vm.Position);
                    FocusedRow = VisibleData[realIndex];
                    TopRow = realIndex;
                }
            }
        }

        /// <summary>
        /// Displays the "goto column" dialog and positions the view
        /// </summary>
        private void OnGotoRefColumn()
        {
            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            if (uiVisualizer != null)
            {
                var refSequences = SelectedReferenceSequences;

                var vm = new GotoColumnRowViewModel
                {
                    MinPosition = 1,
                    Position = 1,
                    MaxPosition = TotalColumns,
                    ReferenceSequences = refSequences,
                    SelectedReferenceSequence = refSequences[0]
                };
                bool? rc = uiVisualizer.ShowDialog(typeof(GotoRefSeqColumnView).ToString(), vm);
                if (rc == true)
                {
                    FocusedColumnIndex = _activeView.FirstColumn = (vm.SelectedReferenceSequence.FirstDataIndex + vm.Position - 1);
                }
            }
        }

        /// <summary>
        /// Shift the focus to the left
        /// </summary>
        private void OnShiftFocusLeft()
        {
            if (FocusedColumnIndex > 0)
            {
                FocusedColumnIndex--;
                if (_activeView.FirstColumn > FocusedColumnIndex)
                    _activeView.FirstColumn--;
            }
        }

        /// <summary>
        /// Shifts the focus to the right
        /// </summary>
        private void OnShiftFocusRight()
        {
            if (FocusedColumnIndex < TotalColumns)
            {
                FocusedColumnIndex++;
                if (FocusedColumnIndex + 1 >= Math.Abs(_activeView.FirstColumn + _activeView.VisibleColumns))
                    _activeView.FirstColumn++;
            }
        }

        /// <summary>
        /// Locate a specific sequence by name (or partial name)
        /// </summary>
        private void OnFindByName()
        {
            if (_finder != null)
            {
                _finder.RaiseActivateRequest();
                return;
            }

            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            if (uiVisualizer != null)
            {
                _finder = new FindSearchViewModel(new DelegatingCommand<FindSearchViewModel>(OnFindNext, fsvm => fsvm!=null && !string.IsNullOrEmpty(fsvm.SearchString)));
                uiVisualizer.Show(typeof(FindSearchView).ToString(), _finder, true, (s, e) => { _finder = null; RaiseActivateRequest(); });
            }
        }

        /// <summary>
        /// This is called by the Find dialog to locate the next name
        /// </summary>
        /// <param name="finder"></param>
        private void OnFindNext(FindSearchViewModel finder)
        {
            Debug.Assert(finder == _finder);
            IEnumerable<int> searchRange;

            if (finder.SearchBackward)
            {
                int spos = (FocusedRow != null) ? FocusedRow.Position : TotalRows;
                searchRange = Enumerable.Range(0, spos).Reverse();
            }
            else
            {
                int spos = (FocusedRow != null) ? FocusedRow.Position : -1;
                searchRange = Enumerable.Range(spos + 1, TotalRows - (spos + 1));
            }

            int foundAt = searchRange.Where(i => VisibleData[i].IsSequence).FirstOrDefault(
                i => finder.Match(VisibleData[i].Header));

            if (foundAt == 0 && !finder.Match(VisibleData[0].Header))
            {
                IErrorVisualizer errorVisualizer = Resolve<IErrorVisualizer>();
                if (errorVisualizer != null)
                    errorVisualizer.Show("No Match", "No matches were found.");
            }
            else
            {
                FocusedRow = VisibleData[foundAt];
                TopRow = foundAt;
            }
        }

        /// <summary>
        /// This is called when this view becomes the "active" view
        /// </summary>
        protected override void OnActivate()
        {
            if (IsBevShowing)
                BirdsEyeViewModel.SetActiveViewModel(this);
            if (TaxonomyViewModel != null)
                TaxonomyViewModel.SetActiveViewModel(this);

            // Replace any open taxonomy view with our new data
            SendMessage(AlignmentViewMessages.SwitchAlignmentView, this);
            base.OnActivate();
        }

        /// <summary>
        /// This displays the find by taxonomy box
        /// </summary>
        private void OnFindByTaxonomy()
        {
            var taxonomyJumper = TaxonomyViewModel;
            if (taxonomyJumper != null)
            {
                taxonomyJumper.Show();
                taxonomyJumper.SetActiveViewModel(this);
            }
        }

        /// <summary>
        /// Retrieves the Taxonomy view model
        /// </summary>
        internal TaxonomyJumpViewModel TaxonomyViewModel
        {
            get
            {
                if (_taxonomyJumper == null && IsGrouped)
                {
                    _taxonomyJumper = new TaxonomyJumpViewModel(_data.Entities, Options.MinGroupingRange,
                                           new DelegatingCommand<TaxonomyJumpEventArgs>(OnJumpToTaxonomyGroup));
                }
                return _taxonomyJumper;
            }
        }

        /// <summary>
        /// Retrieves the BirdsEyeView
        /// </summary>
        internal BirdsEyeViewModel BirdsEyeViewModel
        {
            get
            {
                if (_beView == null)
                {
                    _beView = new BirdsEyeViewModel(this)
                      {
                          VisibleColumns = _activeView.VisibleColumns, 
                          FirstColumn = _activeView.FirstColumn,
                          TopRow = TopRow,
                          VisibleRows = VisibleRows,
                      };
                    _beView.ViewClosed += v => { _beView.Dispose();  _beView = null; IsBevShowing = false; };
                }
                return _beView;
            }
        }

        /// <summary>
        /// This is invoked when the user selects a "jump" in the taxonomy view
        /// </summary>
        /// <param name="e">Name of taxonomy group to jump to</param>
        private void OnJumpToTaxonomyGroup(TaxonomyJumpEventArgs e)
        {
            var groupHeader = VisibleData.FirstOrDefault(vd => vd.IsGroupHeader && vd.Header == e.Name);
            if (groupHeader != null)
            {
                // De-select everything
                VisibleData.Where(vd => vd.IsSelected).ForEach(vd => vd.IsSelected = false);

                // Reposition and select this specific grouping
                TopRow = groupHeader.Position;
                if (e.SelectRows)
                {
                    Enumerable.Range(groupHeader.Position + 1, TotalRows - groupHeader.Position - 1).TakeWhile(
                       i => VisibleData[i].IsSequence).Select(i => VisibleData[i]).ForEach(vd => vd.IsSelected = true);
                }
            }
        }

        /// <summary>
        /// This method selects or deselects a reference sequence
        /// </summary>
        /// <param name="wrapper">Entity</param>
        private void OnMakeReferenceSequence(AlignmentEntityViewModel wrapper)
        {
            if (!wrapper.IsReferenceSequence)
            {
                LockedRows.Add(wrapper);
                wrapper.ReferenceSequenceBorder = null;
                wrapper.ReferenceSequenceColor = Options.GetNextReferenceSequenceBrush(LockedRows.Count);
                wrapper.IsReferenceSequence = true;
            }
            else
            {
                wrapper.ReferenceSequenceBorder = null;
                wrapper.ReferenceSequenceColor = null;
                LockedRows.Remove(wrapper);
                wrapper.IsReferenceSequence = false;
            }
            OnPropertyChanged("HasReferenceSequence", "HasLockedRows", "FocusedColumnReferenceIndex");
        }

        /// <summary>
        /// This is called when the database or filter is being changed - we need to completely redraw
        /// the view on the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaderPropertiesChanged(object sender, EventArgs e)
        {
            ReloadSequences();
            // The world just changed.
            OnPropertyChanged(null);
        }

        /// <summary>
        /// The raw entity data to display - this is always wrapped in AlignmentEntityViewModel objects
        /// but this list represents the ordered, grouped data from the provider.
        /// </summary>
        public IList<IAlignedBioEntity> GroupedEntities
        {
            get
            {
                if (_groupedList.Count == 0)
                {
                    var allData = _data.Entities;
                    var rawList = allData.OrderBy(r => r.Entity.TaxonomyId).GroupBy(r => r.Entity.TaxonomyId);

                    int groupCount = rawList.Count();
                    SupportsGrouping = groupCount > 1;

                    var items = (SupportsGrouping && IsGrouped)
                                    ? new GroupListGenerator(rawList).Generate(Options.MinGroupingRange).AsEnumerable()
                                    : ((Options.SortByTaxonomy)
                                            ? allData.OrderBy(r => r.Entity.ScientificName)
                                            : allData.AsEnumerable());

                    _groupedList.AddRange(items);
                    OnPropertyChanged("TotalRows");
                }

                return _groupedList;
            }
        }

        /// <summary>
        /// This method is used to load the VisibleData collection with the proper set of sequences.
        /// </summary>
        public void ReloadSequences()
        {
            INotificationVisualizer wait = Resolve<INotificationVisualizer>();
            Debug.Assert(wait != null);
            using (wait.BeginWait("Refreshing View", "Loading Data"))
            {
                _groupedList.Clear();

                // No entities? 
                if (GroupedEntities == null)
                {
                    if (_taxonomyJumper != null)
                        _taxonomyJumper.RaiseCloseRequest();

                    VisibleData.Clear();
                    return;
                }

                // Add all the entries
                var seqCollection = new AlignmentEntityViewModel[GroupedEntities.Count];
                Parallel.For(0, GroupedEntities.Count,
                             i => seqCollection[i] = new AlignmentEntityViewModel(this, GroupedEntities[i], i));

                // Push it to the UI
                VisibleData.Clear();
                foreach (var item in seqCollection)
                    VisibleData.Add(item);

                // Add in the proper line numbers
                var sequences = VisibleData.Where(vd => vd.IsSequence).ToList();
                Parallel.For(0, sequences.Count, i => { sequences[i].DisplayIndex = i + 1; });

                // If the taxonomy view is alive, refresh it.
                if (_taxonomyJumper != null)
                {
                    if (IsGrouped)
                        _taxonomyJumper.ChangedGrouping(_data.Entities, Options.MinGroupingRange);
                    else
                        _taxonomyJumper.RaiseCloseRequest();
                }
            }
        }

        internal void SplitViewDimensionsChanged(SplitPaneViewModel splitPaneViewModel)
        {
            if (splitPaneViewModel == ActiveView && _beView != null)
            {
                _beView.FirstColumn = splitPaneViewModel.FirstColumn;
                _beView.VisibleColumns = splitPaneViewModel.VisibleColumns;
            }
        }

        /// <summary>
        /// This is called when the view changes it's docking.  We need to re-parent the Tax/Bev viewers.
        /// </summary>
        protected override void OnDockStyleChanged()
        {
            if (_beView != null)
                _beView.SetActiveViewModel(this);
            if (_taxonomyJumper != null)
                _taxonomyJumper.SetActiveViewModel(this);

            base.OnDockStyleChanged();
        }

        public void ChangeCurrentPosition(int firstColumn, int topRow)
        {
            _topRow = topRow;
            _activeView.FirstColumn = firstColumn;
            OnPropertyChanged("TopRow");
        }
    }
}