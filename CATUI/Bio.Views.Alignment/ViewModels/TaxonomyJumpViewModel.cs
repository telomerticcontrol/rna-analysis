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
using System.Linq;
using System.Windows.Input;
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Internal;
using Bio.Views.Alignment.Views;
using Bio.Views.ViewModels;
using JulMar.Windows.Mvvm;
using System;

namespace Bio.Views.Alignment.ViewModels
{
    /// <summary>
    /// This view model drives the Taxonomy Jump floating window
    /// </summary>
    internal class TaxonomyJumpViewModel : SidebarViewModel<TaxonmyJumpView>
    {
        private int _collapseLevel, _maxLevels;
        private TaxonomyNode _root;

        /// <summary>
        /// This is a single node in the grouped taxonomy list
        /// </summary>
        internal class TaxonomyNode : SimpleViewModel
        {
            private bool _isExpanded;

            public string Name { get; set; }
            public int Count { get; set; }
            public int TotalCount { get; set; }

            public bool IsExpanded
            {
                get { return _isExpanded; }
                set { _isExpanded = value; OnPropertyChanged("IsExpanded"); }
            }

            public ObservableCollection<TaxonomyNode> Children { get; private set; }

            public TaxonomyNode(GroupListGenerator.GroupNode node)
            {
                Name = node.Header;
                TotalCount = Count = node.Nodes.Count;

                Children = new ObservableCollection<TaxonomyNode>();
                foreach (var child in node.Children)
                {
                    var tn = new TaxonomyNode(child);
                    TotalCount += tn.TotalCount;

                    Children.Add(tn);
                }
            }
        }

        /// <summary>
        /// List we are data bound to
        /// </summary>
        public TaxonomyNode[] Root
        {
            get { return new[] {_root}; }
        }

        /// <summary>
        /// Command used to jump in the UI
        /// </summary>
        public ICommand Jump { get; private set; }

        /// <summary>
        /// True/False whether to select rows in alignment view
        /// </summary>
        public bool SelectRowsInAlignmentView { get; set; }

        /// <summary>
        /// This is bound to the treeview selection changed
        /// </summary>
        public ICommand SelectionChanged { get; private set; }

        /// <summary>
        /// The expand/collapse level used
        /// </summary>
        public int CollapseLevel
        {
            get { return _collapseLevel; }
            set
            {
                int newValue = value;
                if (newValue < 2)
                    newValue = 2;
                if (newValue > _maxLevels)
                    newValue = _maxLevels;

                if (value != _collapseLevel)
                {
                    _collapseLevel = newValue;
                    OnPropertyChanged("CollapseLevel");

                    ExpandCollapse(_root, 0, _collapseLevel);
                }
            }
        }

        /// <summary>
        /// This method expands and collapses the Tree List.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="currentLevel"></param>
        /// <param name="collapseLevel"></param>
        private static void ExpandCollapse(TaxonomyNode node, int currentLevel, int collapseLevel)
        {
            if (node == null)
                return;
            node.IsExpanded = (currentLevel < collapseLevel);
            foreach (var child in node.Children)
                ExpandCollapse(child, currentLevel + 1, collapseLevel);
        }

        /// <summary>
        /// This method changes the current grouping and is called whenever the grouping
        /// parameters are changed.
        /// </summary>
        /// <param name="allData">List of entities</param>
        /// <param name="groupLevel">Grouping level</param>
        public void ChangedGrouping(IList<IAlignedBioEntity> allData, int groupLevel)
        {
            _root = new TaxonomyNode(new GroupListGenerator(allData.OrderBy(r => r.Entity.TaxonomyId).GroupBy(r => r.Entity.TaxonomyId)).GetRoot(groupLevel));

            _maxLevels = FindMaxLevel(_root, 0);
            if (_maxLevels < _collapseLevel)
                CollapseLevel = _maxLevels;
            else
            {
                int lastLevel = _collapseLevel;
                _collapseLevel = 0;
                CollapseLevel = lastLevel;
            }

            OnPropertyChanged("Root");
        }

        /// <summary>
        /// This locates the maximum depth of the taxonomy tree we are using.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="currLevel"></param>
        /// <returns></returns>
        private static int FindMaxLevel(TaxonomyNode node, int currLevel)
        {
            return (node == null || node.Children.Count == 0)
                ? currLevel
                : node.Children.Max(child => FindMaxLevel(child, currLevel + 1));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entities">Alignment View data</param>
        /// <param name="jumpCommand">Command to jump to a group</param>
        /// <param name="groupLevel">Grouping level</param>
        public TaxonomyJumpViewModel(IList<IAlignedBioEntity> entities, int groupLevel, ICommand jumpCommand)
        {
            Title = "Taxonomy Browser";
            ImageUrl = "/Bio.Views.Alignment;component/images/tax_icon.png";

            Jump = jumpCommand;
            SelectRowsInAlignmentView = false;
            SelectionChanged = new DelegatingCommand(OnSelectionChanged);
            ChangedGrouping(entities, groupLevel);
            CollapseLevel = 3;
        }


        /// <summary>
        /// This is the handler for the selection changed event
        /// </summary>
        /// <param name="newSelection">Event parameters</param>
        private void OnSelectionChanged(object newSelection)
        {
            var newNode = newSelection as TaxonomyNode;
            if (newNode != null)
            {
                var jumpCommand = Jump;
                if (jumpCommand != null)
                    jumpCommand.Execute(new TaxonomyJumpEventArgs { Name = newNode.Name, SelectRows = SelectRowsInAlignmentView });
            }
        }
    }

    /// <summary>
    /// The event arguments passed to the jump to taxonomy
    /// </summary>
    public class TaxonomyJumpEventArgs
    {
        public string Name { get; set; }
        public bool SelectRows { get; set; }
    }
}
