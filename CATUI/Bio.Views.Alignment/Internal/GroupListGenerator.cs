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

namespace Bio.Views.Alignment.Internal
{
    /// <summary>
    /// This class is an in-memory representation of the the loaded sequence graph based on the 
    /// selected grouping properties.  It is used to generate the final sorted/grouped list of items.
    /// </summary>
    internal class GroupListGenerator
    {
        private const char Separator = '\\';
        internal class GroupNode
        {
            public string Header { get; set; }
            public List<IAlignedBioEntity> Nodes { get; private set; }
            public List<GroupNode> Children { get; private set; }

            public GroupNode(string header)
            {
                Header = header.Trim();
                Nodes = new List<IAlignedBioEntity>();
                Children = new List<GroupNode>();
            }

            public GroupNode Find(string[] path)
            {
                if (string.Compare(Header, path[0]) == 0 && path.Length == 1)
                    return this;

                GroupNode child = Children.Find(gn => string.Compare(gn.Header, path[1], true) == 0);
                if (child == null)
                {
                    GroupNode currNode = this;
                    for (int i = 1; i < path.Length; i++)
                    {
                        GroupNode node = new GroupNode(path[i]);
                        currNode.Children.Add(node);
                        currNode = node;
                    }
                    return currNode;
                }

                return child.Find(Enumerable.Range(1, path.Length - 1).Select(i => path[i]).ToArray());
            }

            public IEnumerable<IAlignedBioEntity> Generate(string prefix, int level)
            {
                string fullHeader = !string.IsNullOrEmpty(prefix) ? (prefix + Separator + Header) : Header;

                yield return new GroupHeader(Header, fullHeader) { TaxonomyLevel = level };

                foreach (var node in Nodes.OrderBy(n => n.Entity.ScientificName))
                    yield return node;

                foreach (var child in Children.OrderBy(n => n.Header))
                {
                    foreach (var entry in child.Generate(fullHeader, level + 1))
                        yield return entry;
                }
            }

            private IEnumerable<IAlignedBioEntity> CheckIfCollapse(int count)
            {
                // Only look at leaf nodes.
                if (Children.Count == 0)
                {
                    // Not enough entries to make a grouping?  Collapse to a higher level..
                    if (Nodes.Count > 0 && Nodes.Count < count)
                    {
                        var myList = Nodes;
                        Nodes = new List<IAlignedBioEntity>();
                        return myList;
                    }
                }

                return null;
            }

            private bool InternalCollapse(int count)
            {
                bool foundNodes = false;

                for (int i = 0; i < Children.Count; i++)
                {
                    var node = Children[i];

                    if (node.InternalCollapse(count))
                        return true;

                    var newList = node.CheckIfCollapse(count);
                    if (newList == null) 
                        continue;
                    
                    Nodes.AddRange(newList);
                    Children.RemoveAt(i);
                    i--;
                    foundNodes = true;
                }
                return foundNodes;
            }

            public void Collapse(int count)
            {
                while (InternalCollapse(count))
                {
                }
            }
        }

        private GroupNode _rootNode;

        public GroupListGenerator(IEnumerable<IGrouping<string, IAlignedBioEntity>> groupedList)
        {
            // Build our tree based on the input.
            foreach (var grouping in groupedList)
            {
                string group = grouping.Key.Trim();
                if (!string.IsNullOrEmpty(group))
                {
                    GroupNode node = FindNode(group);
                    node.Nodes.AddRange(grouping);
                }
            }
        }

        private GroupNode FindNode(string name)
        {
            string[] groupNames = name.Split(Separator).Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToArray();
            if (groupNames.Length == 0)
                throw new ArgumentException("Invalid grouping name");

            if (_rootNode == null)
            {
                _rootNode = new GroupNode(groupNames[0]);
                GroupNode node = _rootNode;
                for (int i = 1; i < groupNames.Length; i++)
                {
                    GroupNode next = new GroupNode(groupNames[i]);
                    node.Children.Add(next);
                    node = next;
                }
                return node;
            }

            return _rootNode.Find(groupNames);
        }

        public GroupNode GetRoot(int count)
        {
            _rootNode.Collapse(count);
            return _rootNode;
        }

        public List<IAlignedBioEntity> Generate(int count)
        {
            // Go through the list and collapse nodes that don't match our count..
            _rootNode.Collapse(count);
            return _rootNode.Generate(null, 1).ToList();
        }
    }
}