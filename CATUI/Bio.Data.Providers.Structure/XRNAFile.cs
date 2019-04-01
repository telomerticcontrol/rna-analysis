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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Bio.Data.Providers.Helpers;
using Bio.Data.Providers.Interfaces;
using Bio.Data.Interfaces;
using System.Collections;

namespace Bio.Data.Providers.Structure
{
    /// <summary>
    /// This loads the XRNA file format
    /// </summary>
    class XRNAFile : IBioDataLoader<IStructureModelBioEntity>
    {
        public string Filename { get; private set; }

        IList IBioDataLoader.Entities { get { return _entities; } }

        public IList<IStructureModelBioEntity> Entities
        {
            get { return _entities; }
        }

        /// <summary>
        /// This method is used to prepare the loader to access the
        /// Entities collection.
        /// </summary>
        /// <returns>Count of loaded records</returns>
        public int Load()
        {
            //We pretty much ignore the passed in vaidator since we require an RNA sequence.
            _sequence = new SimpleRNASequence();
            _entities.Clear();
            _bioDefs.Clear();
            
            Parse();
            Compile();

            return _entities.Count;
        }

        /// <summary>
        /// This is used to initialize the BioDataLoader when it is first created.
        /// </summary>
        /// <param name="fileName">String data</param>
        /// <returns>True/False success</returns>
        public bool Initialize(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            Filename = fileName;
            return true;
        }

        /// <summary>
        /// This provides access to any initialization data used to create this loader.
        /// </summary>
        public string InitializationData
        {
            get { return Path.GetFileName(Filename); }
        }

        /// <summary>
        /// This method will just parse and validate the data file
        /// </summary>
        private void Parse()
        {
            using (var reader = File.OpenText(Filename))
            {
                string line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (Nt_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        int fivePrimeIndex = Int32.Parse(tokens[0]);
                        _bioDefs.Add(fivePrimeIndex, tokens);
                    }
                    else if (DefaultFont_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _ntFontDefs.Add(tokens);
                    }
                    else if (BPLineSymbol_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _lineConnectorDefs.Add(tokens);
                    }
                    else if (BPCircleSymbol_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _circleConnectorDefs.Add(tokens);
                    }
                    else if (BPNullSymbol_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _hiddenConnectorDefs.Add(tokens);
                    }
                    else if (StringLabel_Def.IsMatch(line))
                    {
                        _textLabelDefs.Add(line);
                    }
                    else if (LineLabel_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _lineLabelDefs.Add(tokens);
                    }
                    else if (ParallelogramLabel_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _parallelogramDefs.Add(tokens);
                    }
                    else if (ArcLabel_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _arcDefs.Add(tokens);
                    }
                    else if (ArrowLabel_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _arrowDefs.Add(tokens);
                    }
                    else if (HiddenNt_Def.IsMatch(line))
                    {
                        string[] tokens = Regex.Split(line, " ");
                        _hiddenNtDefs.Add(tokens);
                    }
                    line = reader.ReadLine();
                }
            }
        }

        private void Compile()
        {
            var nt = from idx in _bioDefs.Keys
                     orderby idx ascending
                     select idx;
            foreach (int idx in nt)
            {
                HandleNucleotideDef(_bioDefs[idx]);
            }

            foreach (string[] lineDef in _lineConnectorDefs)
            {
                HandleBasePairConnectorLine(lineDef);
            }

            foreach (string[] lineDef in _circleConnectorDefs)
            {
                HandleBasePairConnectorCircle(lineDef);
            }

            foreach (string[] lineDef in _hiddenConnectorDefs)
            {
                HandleBasePairConnectorNull(lineDef);
            }

            foreach (string stringLabelDef in _textLabelDefs)
            {
                HandleTextLabelDef(stringLabelDef);
            }

            foreach (string[] ntFontDef in _ntFontDefs)
            {
                HandleSymbolFontDef(ntFontDef);
            }

            foreach (string[] lineLabelDef in _lineLabelDefs)
            {
                HandleLineLabelDef(lineLabelDef);
            }

            foreach (string[] parallelogramLabelDef in _parallelogramDefs)
            {
                HandleParallelogramLabelDef(parallelogramLabelDef);
            }

            foreach (string[] arcLabelDef in _arcDefs)
            {
                HandleArcLabelDef(arcLabelDef);
            }

            foreach (string[] arrowLabelDef in _arrowDefs)
            {
                HandleArrowLabelDef(arrowLabelDef);
            }

            foreach (string[] hiddenNTdef in _hiddenNtDefs)
            {
                HandleHiddenNucleotideDef(hiddenNTdef);
            }

            IEnumerable<XRNABasePair> defaultConnectors = _basePairs.Except(_basePairsWithConnectorMod);
            
            foreach (XRNABasePair defaultConnectorBP in defaultConnectors)
            {
                if (string.Compare(defaultConnectorBP.Label, "G-U", true) == 0 || string.Compare(defaultConnectorBP.Label, "U-G", true) == 0)
                {
                    _entities.Add(new XRNABasePairConnectorCircle
                    {
                        BasePair = defaultConnectorBP,
                        Color = Default_Connector_Color,
                        Radius = 1.5,
                        Thickness = 1.0,
                        Filled = true
                    });
                }
                else
                {
                    _entities.Add(new XRNABasePairConnector
                    {
                        BasePair = defaultConnectorBP,
                        Color = Default_Connector_Color,
                        Thickness = Default_Connector_Thickness,
                        Visible = true
                    });
                }
            }
        }

        private void HandleBasePairConnectorLine(string[] def)
        {
            Debug.Assert(def.Length == 9);

            string[] ntPositions = Regex.Split(def[0], ",");
            string hexColorVal = "#" + def[3];
            hexColorVal = hexColorVal.PadRight(7, '0');

            var selectedBasePairs = from bp in _basePairs
                                    join idx in
                                        (from token in ntPositions
                                         from val in ExpandRange(token, true)
                                         select val)
                                    on bp.FivePrimeIndex equals idx-1
                                    select bp;
            foreach (XRNABasePair bp in selectedBasePairs)
            {
                XRNABasePairConnector connector = new XRNABasePairConnector()
                {
                    BasePair = bp,
                    Visible = true,
                    Thickness = Double.Parse(def[2]),
                    Color = hexColorVal
                };
                _entities.Add(connector);
                _basePairsWithConnectorMod.Add(bp);
            }
        }

        private void HandleBasePairConnectorCircle(string[] def)
        {
            Debug.Assert(def.Length == 8);

            string[] ntPositions = Regex.Split(def[0], ",");
            string hexColorVal = "#" + def[7];
            hexColorVal = hexColorVal.PadRight(7, '0');
            int filledBit = Int32.Parse(def[6]);


            var selectedBasePairs = from bp in _basePairs
                                    join idx in
                                        (from token in ntPositions
                                         from val in ExpandRange(token, true)
                                         select val)
                                     on bp.FivePrimeIndex equals idx-1
                                    select bp;

            foreach (XRNABasePair bp in selectedBasePairs)
            {
                XRNABasePairConnectorCircle connector = new XRNABasePairConnectorCircle()
                {
                    BasePair = bp,
                    Thickness = Double.Parse(def[5]),
                    Filled = (filledBit == 0) ? true : false,
                    Radius = Double.Parse(def[4]),
                    Color = hexColorVal
                };
                _entities.Add(connector);
                _basePairsWithConnectorMod.Add(bp);
            }
        }

        private void HandleBasePairConnectorNull(string[] def)
        {
            Debug.Assert(def.Length == 2);

            string[] ntPositions = Regex.Split(def[0], ",");

            var selectedBasePairs = from bp in _basePairs
                                    join idx in
                                        (from token in ntPositions
                                         from val in ExpandRange(token, true)
                                         select val)
                                     on bp.FivePrimeIndex equals idx-1
                                    select bp;

            foreach (XRNABasePair bp in selectedBasePairs)
            {
                XRNABasePairConnector connector = new XRNABasePairConnector()
                {
                    BasePair = bp,
                    Color = Default_Connector_Color,
                    Visible = false,
                    Thickness = Default_Connector_Thickness
                };
                _entities.Add(connector);
                _basePairsWithConnectorMod.Add(bp);
            }

        }

        private void HandleSymbolFontDef(string[] def)
        {
            Debug.Assert(def.Length == 4);

            string[] field1 = Regex.Split(def[0], "#");
            string[] ntPositions = Regex.Split(field1[0], ",");
            int typefaceId = Int32.Parse(def[2]);
            string hexColorVal = "#" + def[3];
            hexColorVal = hexColorVal.PadRight(7, '0');
            double fontSize = Double.Parse(def[1]);
            FontDescriptor desc = DecodeTypeFace(typefaceId);

            var selectedNt = from nt in _entities.OfType<XRNANucleotide>()
                             join idx in
                                 (from token in ntPositions
                                  from val in ExpandRange(token, true)
                                  select val)
                                on nt.Index equals idx - 1
                             select nt;
            foreach (XRNANucleotide nt in selectedNt)
            {
                nt.FontSize = fontSize;
                nt.FontFace = desc.Font;
                nt.FontStyle = desc.FontStyle;
                nt.FontWeight = desc.FontWeight;
            }
        }

        private void HandleHiddenNucleotideDef(string[] def)
        {
            Debug.Assert(def.Length == 4);

            int startIdx = Int32.Parse(def[2]);
            int endIdx = Int32.Parse(def[3]);
            if (startIdx < endIdx &&
                startIdx >= 1 &&
                endIdx <= _sequence.RawData.Count)
            {
                string rangeDef = string.Format("{0}-{1}", startIdx-1, endIdx-1);
                IEnumerable<int> hiddenNt = ExpandRange(rangeDef, true);
                var upHiddenNt = from hnt in hiddenNt
                                 join upnt in _unpairedNt
                                 on hnt equals upnt.Index
                                 select upnt;
                var bpHiddenNtFp = from hnt in hiddenNt
                                   join bp in _basePairs
                                   on hnt equals bp.FivePrimeIndex
                                   select bp.FivePrimeNucleotide;
                var bpHiddenNtTp = from hnt in hiddenNt
                                   join bp in _basePairs
                                   on hnt equals bp.ThreePrimeIndex
                                   select bp.ThreePrimeNucleotide;

                foreach (XRNANucleotide hnt in upHiddenNt.Union(bpHiddenNtFp).Union(bpHiddenNtTp))
                {
                    hnt.Hidden = true;
                }
            }
        }

        private void HandleNucleotideDef(string[] def)
        {
            Debug.Assert(def.Length == 6);
            
            int fivePrimeIdx = Int32.Parse(def[0]);
            double xpos = Double.Parse(def[2]);
            double ypos = Double.Parse(def[3]);
            int pairedIdx = Int32.Parse(def[5]);
            char val = def[1][0];

            _sequence.AddSymbol(val);

            XRNANucleotide nt = new XRNANucleotide()
            {
                Hidden = false,
                Index = fivePrimeIdx - 1,
                Sequence = _sequence,
                Center = new Position() { X = xpos, Y = (-1 * ypos) },
                FontFace = Default_Font_Typeface.Font,
                FontStyle = Default_Font_Typeface.FontStyle,
                FontWeight = Default_Font_Typeface.FontWeight,
                FontSize = Default_Font_Size,
                Color = Default_Font_Color
            };

            if (pairedIdx > 0)
            {
                if (pairedIdx > fivePrimeIdx)
                {
                    //We convert to 0-based indexing.
                    XRNABasePair bp = new XRNABasePair(_sequence)
                    {
                        FivePrimeNucleotide = nt,
                        FivePrimeIndex = fivePrimeIdx - 1
                    };
                    _basePairs.Add(bp);
                    _entities.Add(bp);
                    _byThreePrimeNtCache.Add(pairedIdx, bp);
                }
                else
                {
                    if (_byThreePrimeNtCache.ContainsKey(fivePrimeIdx))
                    {
                        XRNABasePair bp = _byThreePrimeNtCache[fivePrimeIdx];
                        bp.ThreePrimeNucleotide = nt;
                        bp.ThreePrimeIndex = fivePrimeIdx - 1;
                    }
                }
            }
            else
            {
                _unpairedNt.Add(nt);
                _entities.Add(nt);
            }
        }

        private void HandleTextLabelDef(string line)
        {
            char[] spaceDelimiter = { ' ' };
            string[] stringLabelLine = Text_Def.Split(line);
            string[] tokens = stringLabelLine[0].Trim().Split(spaceDelimiter);

            int fontID = Int32.Parse(tokens[7]);
            FontDescriptor fontDesc = DecodeTypeFace(fontID);

            string hexColorVal = "#" + tokens[8];
            hexColorVal = hexColorVal.PadRight(7, '0');
            double xpos = Double.Parse(tokens[1]);
            double ypos = Double.Parse(tokens[2]);

            TextLabel label = new TextLabel()
            {
                Start = new Position { X = xpos, Y = -1 * ypos },
                FontSize = Double.Parse(tokens[6]),
                Text = stringLabelLine[1],
                FontFace = fontDesc.Font,
                FontStyle = fontDesc.FontStyle,
                FontWeight = fontDesc.FontWeight,
                Color = hexColorVal
            };
            _entities.Add(label);
        }

        private void HandleLineLabelDef(string[] def)
        {
            Debug.Assert(def.Length == 13);

            string hexColorVal = "#" + def[7];
            hexColorVal = hexColorVal.PadRight(7, '0');
            double xposstart = Double.Parse(def[1]);
            double yposstart = Double.Parse(def[2]);
            double xposend = Double.Parse(def[3]);
            double yposend = Double.Parse(def[4]);
            int attachedNtIdx = Int32.Parse(def[5]);
            double lineWeight = Double.Parse(def[6]);

            LineLabel label = new LineLabel()
            {
                Start = new Position() { X = xposstart, Y = -1 * yposstart },
                End = new Position() { X = xposend, Y = -1 * yposend },
                Color = hexColorVal,
                LineWeight = lineWeight,
                AttachedNtIdx = attachedNtIdx-1
            };
            _entities.Add(label);
        }

        private void HandleParallelogramLabelDef(string[] def)
        {
            Debug.Assert(def.Length == 12);

            string hexColorVal = "#" + def[11];
            hexColorVal = hexColorVal.PadRight(7, '0');

            double xpos = Double.Parse(def[1]);
            double ypos = -1 * Double.Parse(def[2]);
            double angle1 = Double.Parse(def[4]);
            double s1len = Double.Parse(def[5]);
            double angle2 = Double.Parse(def[6]);
            double s2len = Double.Parse(def[7]);
            int attachedNtIdx = Int32.Parse(def[8]);
            double lineWeight = Double.Parse(def[9]);
            int closedBit = Int32.Parse(def[10]);

            ParallelogramLabel label = new ParallelogramLabel()
            {
                Center = new Position() { X = xpos, Y = ypos },
                LineWeight = lineWeight,
                Color = hexColorVal,
                AttachedNtIdx = attachedNtIdx,
                Side1Length = s1len,
                Side1Angle = angle1,
                Side2Length = s2len,
                Side2Angle = angle2,
                Closed = (closedBit==1) ? true : false
            };
            _entities.Add(label);
        }

        private void HandleArcLabelDef(string[] def)
        {
            Debug.Assert(def.Length == 11);

            string hexColorVal = "#" + def[10];
            hexColorVal = hexColorVal.PadRight(7, '0');

            double xcenter = Double.Parse(def[1]);
            double ycenter = -1 * Double.Parse(def[2]);
            double angle1 = Double.Parse(def[5]);
            double angle2 = Double.Parse(def[6]);
            double radius = Double.Parse(def[7]);
            double lineWeight = Double.Parse(def[8]);
            int closedBit = Int32.Parse(def[9]);
            int attachedNtIdx = Int32.Parse(def[4]);

            ArcLabel label = new ArcLabel()
            {
                Center = new Position() { X = xcenter, Y = ycenter },
                Radius = radius,
                LineWeight = lineWeight,
                Color = hexColorVal,
                Angle1 = angle1,
                Angle2 = angle2,
                Closed = (closedBit==1) ? true : false,
                AttachedNtIdx = attachedNtIdx
            };
            _entities.Add(label);
        }

        private void HandleArrowLabelDef(string[] def)
        {
            Debug.Assert(def.Length == 13);

            string hexColorVal = "#" + def[12];
            hexColorVal = hexColorVal.PadRight(7, '0');

            double xtip = Double.Parse(def[1]);
            double ytip = -1 * Double.Parse(def[2]);
            double xlefttip = Double.Parse(def[4]);
            double ylefttip = Double.Parse(def[5]);
            double vectorHeadInc = Double.Parse(def[6]);
            double angle = Double.Parse(def[7]);
            int attachedNtIdx = Int32.Parse(def[8]);
            double lineWeight = Double.Parse(def[9]);
            double tailLength = Double.Parse(def[10]);
            int closedBit = Int32.Parse(def[11]);

            ArrowLabel label = new ArrowLabel()
            {
                ArrowTip = new Position() { X = xtip, Y = ytip },
                LeftTip = new Position() { X = xlefttip, Y = ylefttip },
                VectorHeadIncrement = vectorHeadInc,
                Angle = angle,
                AttachedNtIdx = attachedNtIdx,
                LineWeight = lineWeight,
                TailLength = tailLength,
                Color = hexColorVal,
                Closed = (closedBit==1) ? true : false
            };
            _entities.Add(label);
        }

        private FontDescriptor DecodeTypeFace(int typeFaceId)
        {
            FontDescriptor fdesc = new FontDescriptor();
            switch (typeFaceId)
            {
                case 0:
                    fdesc.Font = "Helvetica, Arial";
                    fdesc.FontStyle = "Normal";
                    fdesc.FontWeight = "Regular";
                    break;
                case 1:
                    fdesc.Font = "Helvetica, Arial";
                    fdesc.FontStyle = "Oblique";
                    fdesc.FontWeight = "Regular";
                    break;
                case 2:
                    fdesc.Font = "Helvetica, Arial";
                    fdesc.FontWeight = "Bold";
                    fdesc.FontStyle = "Normal";
                    break;
                case 3:
                    fdesc.Font = "Helvetica, Arial";
                    fdesc.FontStyle = "Oblique";
                    fdesc.FontWeight = "Bold";
                    break;
                case 4:
                    fdesc.Font = "Times, Times New Roman";
                    fdesc.FontStyle = "Normal";
                    fdesc.FontWeight = "Regular";
                    break;
                case 5:
                    fdesc.Font = "Times, Times New Roman";
                    fdesc.FontStyle = "Italic";
                    fdesc.FontWeight = "Regular";
                    break;
                case 6:
                    fdesc.Font = "Times, Times New Roman";
                    fdesc.FontWeight = "Bold";
                    fdesc.FontStyle = "Normal";
                    break;
                case 7:
                    fdesc.Font = "Times, Times New Roman";
                    fdesc.FontStyle = "Italic";
                    fdesc.FontWeight = "Bold";
                    break;
                case 8:
                    fdesc.Font = "Courier, Courier New";
                    fdesc.FontStyle = "Normal";
                    fdesc.FontWeight = "Regular";
                    break;
                case 9:
                    fdesc.Font = "Courier, Courier New";
                    fdesc.FontStyle = "Oblique";
                    fdesc.FontWeight = "Regular";
                    break;
                case 10:
                    fdesc.Font = "Courier, Courier New";
                    fdesc.FontWeight = "Bold";
                    fdesc.FontStyle = "Normal";
                    break;
                case 11:
                    fdesc.Font = "Courier, Courier New";
                    fdesc.FontStyle = "Oblique";
                    fdesc.FontWeight = "Bold";
                    break;
                case 13:
                default:
                    fdesc.Font = "Symbol";
                    fdesc.FontStyle = "Normal";
                    fdesc.FontWeight = "Regular";
                    break;
            };
            return fdesc;
        }

        private IEnumerable<int> ExpandRange(string rangeDef, bool inclusive)
        {
            if (!rangeDef.Contains("-"))
            {
                yield return Int32.Parse(rangeDef);
            }
            else
            {
                string[] ends = Regex.Split(rangeDef, "-");
                int start = Int32.Parse(ends[0]), end = Int32.Parse(ends[1]), count = end - start;
                foreach (int val in Enumerable.Range(start, (inclusive) ? count + 1 : count))
                {
                    yield return val;
                }
            }
        }

        private Dictionary<int, string[]> _bioDefs = new Dictionary<int, string[]>();
        private List<string[]> _arrowDefs = new List<string[]>();
        private List<string[]> _arcDefs = new List<string[]>();
        private List<string[]> _parallelogramDefs = new List<string[]>();
        private List<string[]> _lineConnectorDefs = new List<string[]>();
        private List<string[]> _circleConnectorDefs = new List<string[]>();
        private List<string[]> _hiddenConnectorDefs = new List<string[]>();
        private List<string[]> _lineLabelDefs = new List<string[]>();
        private List<string[]> _ntFontDefs = new List<string[]>();
        private List<string[]> _hiddenNtDefs = new List<string[]>();
        private List<string> _textLabelDefs = new List<string>();
        private Dictionary<int, XRNABasePair> _byThreePrimeNtCache = new Dictionary<int, XRNABasePair>();
        private List<XRNABasePair> _basePairs = new List<XRNABasePair>();
        private List<XRNABasePair> _basePairsWithConnectorMod = new List<XRNABasePair>();
        private List<XRNANucleotide> _unpairedNt = new List<XRNANucleotide>();
        private SimpleRNASequence _sequence;
        private List<IStructureModelBioEntity> _entities = new List<IStructureModelBioEntity>();
        private static readonly FontDescriptor Default_Font_Typeface = new FontDescriptor() { Font = "Helvetica, Arial", FontStyle = "Normal", FontWeight = "Regular" };
        private static readonly double Default_Font_Size = 8.0;
        private static readonly string Default_Font_Color = "#000000";
        private static readonly string Default_Connector_Color = "#000000";
        private static readonly double Default_Connector_Thickness = 1.0;
        private static readonly Regex Nt_Def = new Regex(@"\d+\s[AGCUN]\s-?\d+.\d{2}\s-?\d+.\d{2}\s-?[0-9]2?\s\d+");
        private static readonly Regex DefaultFont_Def = new Regex(@"\d+-\d+#!s\s\d+\s\d+\s\d+");
        private static readonly Regex BPLineSymbol_Def = new Regex(@"[\d+(-?\d+),?]+\s#!Bl\s\d+.\d{2}\s[0-9a-fA-F]+(\s\d){5}");
        private static readonly Regex BPCircleSymbol_Def = new Regex(@"[\d+(-?\d+),?]+\s#!Bc\s\d+.\d{2}\s\d+.\d{2}\s\d+.\d{2}\s\d+.\d{2}\s\d\s[a-zA-Z0-9]+");
        private static readonly Regex BPNullSymbol_Def = new Regex(@"[\d+(-?\d+),?]+\s#!Bn");
        private static readonly Regex StringLabel_Def = new Regex(@"s\s-?\d+.\d{2}\s-?\d+.\d{2}\s\d+.\d{2}\s\d.\d\s\d+\s\d+\s\d+\s[0-9a-fA-F]+\s""([^""\\]*(\.[^""\\]*)*)""");
        private static readonly Regex Text_Def = new Regex(@"""([^""\\]*(\.[^""\\]*)*)""");
        private static readonly Regex LineLabel_Def = new Regex(@"l(\s-?\d+.\d{2}){4}\s\d+\s\d+.\d{2}\s[0-9a-fA-F]+(\s\d){5}");
        private static readonly Regex ParallelogramLabel_Def = new Regex(@"p(\s-?\d+.\d+){2}(\s\d+.\d){5}\s\d+\s\d+.\d+\s\d\s[0-9a-fA-F]+");
        private static readonly Regex ArcLabel_Def = new Regex(@"a(\s-?\d+.\d+){2}\s\d+.\d+\s\d+\s\d+.\d{2}\s\d+.\d{2}(\s\d+.\d{2}){2}\s\d\s[0-9a-fA-F]+");
        private static readonly Regex ArrowLabel_Def = new Regex(@"r(\s-?\d+.\d+){7}\s\d+(\s-?\d+.\d+){2}\s\d\s[0-9a-fA-F]+");
        private static readonly Regex HiddenNt_Def = new Regex(@"#!\sh\s\d+\s\d+");
    }

    struct FontDescriptor
    {
        public string Font;
        public string FontStyle;
        public string FontWeight;
    }
}