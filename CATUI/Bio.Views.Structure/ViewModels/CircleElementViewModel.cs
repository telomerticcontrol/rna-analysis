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

using System.Windows;
using System;
using Bio.Data.Interfaces;

namespace Bio.Views.Structure.ViewModels
{
    public class CircleElementViewModel : CircleElementBaseViewModel
    {
        private readonly double _rayAngle;

        private Point _position;
        public Point Position 
        {
            get { return _position; }
            set { _position = value; }
        }

        private Point _nextPosition;
        public Point NextPosition
        {
            get { return _nextPosition; }
            set { _nextPosition = value; }
        }

        private int _currentIndex;
        public int Index
        {
            get { return _currentIndex; }
            private set { _currentIndex = value; }
        }

        private int _nextIndex;
        public int NextIndex
        {
            get { return _nextIndex; }
            private set { _nextIndex = value; }
        }

        public string Label
        {
            get { return this.ToString(); }
        }

        public string Path
        {
            get 
            { 
                double xend = Position.X + (10 * Math.Cos(_rayAngle));
                double yend = Position.Y + (10 * Math.Sin(_rayAngle));
                double xstart = Position.X - (10 * Math.Cos(_rayAngle));
                double ystart = Position.Y - (10 * Math.Sin(_rayAngle));
                return string.Format("M {0},{1} {2},{3}", xstart, ystart,
                    xend, yend);
            }
        }

        public override bool IsSelected
        {
            get
            {
                return base.IsSelected;
            }
            set
            {
                if(!base.IsSelected && value)
                    SendMessage(CircleStructureBrowserMessages.ShowPhyloDInteractions, Index);
                if (base.IsSelected && !value)
                    SendMessage(CircleStructureBrowserMessages.HidePhyloDInteractions, Index);
                base.IsSelected = value;    
            }
        }

        public IBioSymbol Symbol { get; private set; }

        public CircleElementViewModel(IBioSymbol symbol, int index, int nextIndex, Point start, Point end, double circleRadius, double rayAngle)
        {
            Symbol = symbol;
            Index = index;
            NextIndex = nextIndex;
            Position = start;
            NextPosition = end;
            _rayAngle = rayAngle;
        }

        public override string ToString()
        {
            return string.Format("Nt: {0}, {1}", Index, Symbol);
        }
    }
}
