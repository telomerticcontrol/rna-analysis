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
using System.IO;
using System.Text.RegularExpressions;
using ManagedLabelRNA;
using System.Xml.Linq;

namespace Alignment
{
    /// <summary>
    /// Abstract a BPSEQ file into an entity of base pairs.
    /// </summary>
    public class StructureModel
    {
        //Note: Note mapping the adjacent elements, for now.
        public abstract class BaseStructureElement
        {
            public BaseStructureElement(string id)
            {
                ID = id;
                //AdjacentElements = new List<BaseStructureElement>();
            }

            public string ID { get; private set; }
            //public List<BaseStructureElement> AdjacentElements { get; private set; }
        }

        public class Segment : BaseStructureElement
        {
            public Segment(string id) : base(id) { }
            public int LoopStart { get; set; }
            public int LoopEnd { get; set; }
        }

        public class Helix : BaseStructureElement
        {
            public Helix(string id) : base(id) { }
            public int FivePrimeStart { get; set; }
            public int FivePrimeEnd { get; set; }
            public int ThreePrimeStart { get; set; }
            public int ThreePrimeEnd { get; set; }
        }

        public class HairpinLoop : BaseStructureElement
        {
            public HairpinLoop(string id) : base(id) { }
            public Segment Loop { get; set; }
        }

        public class InternalLoop : BaseStructureElement
        {
            public InternalLoop(string id) : base(id) { }
            public Segment FivePrimeLoop { get; set; }
            public Segment ThreePrimeLoop { get; set; }
        }

        public class BulgeLoop : BaseStructureElement
        {
            public BulgeLoop(string id) : base(id) { }
            public Segment Bulge { get; set; }
        }

        public class MultistemLoop : BaseStructureElement
        {
            public MultistemLoop(string id) 
                : base(id) 
            {
                Segments = new List<Segment>();
            }
            public MultistemLoop(string id, List<Segment> segs)
                : base(id)
            {
                Segments = segs;
            }
            public List<Segment> Segments { get; private set; }
        }

        public static StructureModel Parse(string filename)
        {
            try
            {
                //Load the file into memory, were expecting a BPSEQ here.
                string filedata = File.ReadAllText(filename);
                Dictionary<int, int> pairs = (from Match m in Regex.Matches(filedata, @"(?<fiveprime>\d+)\s[AGCUT]\s(?<threeprime>\d+)")
                                              let fiveprimeidx = Int32.Parse(m.Groups["fiveprime"].Value)
                                              let threeprimeidx = Int32.Parse(m.Groups["threeprime"].Value)
                                              where threeprimeidx > 0 && threeprimeidx > fiveprimeidx
                                              orderby fiveprimeidx
                                              select new { fiveprimeidx, threeprimeidx }).ToDictionary(p => p.fiveprimeidx, p => p.threeprimeidx);
                var seqLength = (from Match m in Regex.Matches(filedata, @"(?<fiveprime>\d+)\s[AGCUT]\s(?<threeprime>\d+)")
                                 let fiveprimeidx = Int32.Parse(m.Groups["fiveprime"].Value)
                                 orderby fiveprimeidx
                                 select fiveprimeidx).Max();
                StructureModel retValue = new StructureModel(seqLength);
                retValue.Pairs = pairs;
                return retValue;
            }
            catch
            {
                return new StructureModel(0); //For now we just return an empty StructureModel if we fail.
            }
        }

        public StructureModel(int sequenceLength) 
        {
            Pairs = new Dictionary<int, int>();
            Helices = new List<Helix>();
            KnottedHelices = new List<Helix>();
            Hairpins = new List<HairpinLoop>();
            Internals = new List<InternalLoop>();
            Bulges = new List<BulgeLoop>();
            Stems = new List<MultistemLoop>();
            Strands = new List<Segment>();
            Tails = new List<Segment>();
            SequenceLength = sequenceLength;
        }

        public int SequenceLength { get; private set; }
        public Dictionary<int, int> Pairs { get; private set; }
        public List<Helix> Helices { get; private set; }
        public List<Helix> KnottedHelices { get; private set; }
        public List<HairpinLoop> Hairpins { get; private set; }
        public List<InternalLoop> Internals { get; private set; }
        public List<BulgeLoop> Bulges { get; private set; }
        public List<MultistemLoop> Stems { get; private set; }
        public List<Segment> Strands { get; private set; }
        public List<Segment> Tails { get; private set; }

        public void DecomposeStructure()
        {
            if (Pairs.Count() <= 0)
                return;
            //We initialize the interface to the alden parser.
            ManagedRNA structureparser = new ManagedRNA(Pairs, SequenceLength);
            XElement extents = structureparser.ComputeTieredAsXML();
            //We use LINQ queries to build up the extents.
            HandleHelices(extents);
            HandleHairpins(extents);
            HandleInternals(extents);
            HandleBulges(extents);
            HandleMultistems(extents);
            HandleStrands(extents);
            HandleTails(extents);
        }

        private void HandleStrands(XElement extents)
        {
            Strands = (from s in extents.Descendants("Free")
                       select new Segment(s.Attribute("ID").Value)
                       {
                            LoopStart = Int32.Parse(s.Element("StrandStart").Value),
                            LoopEnd = Int32.Parse(s.Element("StrandEnd").Value)
                       }).ToList<Segment>();
        }

        private void HandleTails(XElement extents)
        {
            Tails = (from t in extents.Descendants("Tail")
                     select new Segment(t.Attribute("ID").Value)
                     {
                            LoopStart = Int32.Parse(t.Element("StrandStart").Value),
                            LoopEnd = Int32.Parse(t.Element("StrandEnd").Value)
                     }).ToList<Segment>();
        }

        private void HandleHelices(XElement extents)
        {
            Helices = (from h in extents.Descendants("Helix")
                       select new Helix(h.Attribute("ID").Value)
                       {
                           FivePrimeStart = Int32.Parse(h.Element("FivePrimeStart").Value),
                           FivePrimeEnd = Int32.Parse(h.Element("FivePrimeEnd").Value),
                           ThreePrimeStart = Int32.Parse(h.Element("ThreePrimeStart").Value),
                           ThreePrimeEnd = Int32.Parse(h.Element("ThreePrimeEnd").Value)
                       }).ToList<Helix>();
            
            KnottedHelices = (from hk in extents.Descendants("Helix-Knot")
                              select new Helix(hk.Attribute("ID").Value)
                              {
                                  FivePrimeStart = Int32.Parse(hk.Element("FivePrimeStart").Value),
                                  FivePrimeEnd = Int32.Parse(hk.Element("FivePrimeEnd").Value),
                                  ThreePrimeStart = Int32.Parse(hk.Element("ThreePrimeStart").Value),
                                  ThreePrimeEnd = Int32.Parse(hk.Element("ThreePrimeEnd").Value)
                              }).ToList<Helix>();
        }

        private void HandleHairpins(XElement extents)
        {
            Hairpins = (from h in extents.Descendants("HairpinLoop")
                       select new HairpinLoop(h.Attribute("ID").Value)
                       {
                           Loop = new Segment(null)
                           {
                                LoopStart = Int32.Parse(h.Element("StrandStart").Value),
                                LoopEnd = Int32.Parse(h.Element("StrandEnd").Value)
                           }
                       }).ToList<HairpinLoop>();
        }

        private void HandleInternals(XElement extents)
        {
            Internals = (from i in extents.Descendants("InternalLoop")
                        select new InternalLoop(i.Attribute("ID").Value)
                        {
                            FivePrimeLoop = new Segment(null)
                            {
                                LoopStart = Int32.Parse(i.Element("FivePrimeStrandStart").Value),
                                LoopEnd = Int32.Parse(i.Element("FivePrimeStrandEnd").Value)
                            },
                            ThreePrimeLoop = new Segment(null)
                            {
                                LoopStart = Int32.Parse(i.Element("ThreePrimeStrandStart").Value),
                                LoopEnd = Int32.Parse(i.Element("ThreePrimeStrandEnd").Value)
                            }
                        }).ToList<InternalLoop>();
        }

        private void HandleBulges(XElement extents)
        {
            Bulges = (from b in extents.Descendants("BulgeLoop")
                     select new BulgeLoop(b.Attribute("ID").Value)
                     {
                         Bulge = new Segment(null)
                         {
                             LoopStart = Int32.Parse(b.Element("StrandStart").Value),
                             LoopEnd = Int32.Parse(b.Element("StrandEnd").Value)
                         }
                     }).ToList<BulgeLoop>();
        }

        private void HandleMultistems(XElement extents)
        {
            Stems = (from s in extents.Descendants("MultistemLoop")
                     select new MultistemLoop(s.Attribute("ID").Value,
                         (from seg in s.Descendants("Strand")
                          select new Segment(null)
                          {
                              LoopStart = Int32.Parse(seg.Element("StrandStart").Value),
                              LoopEnd = Int32.Parse(seg.Element("StrandEnd").Value)
                          }).ToList<Segment>())).ToList<MultistemLoop>();
        }
    }
}
