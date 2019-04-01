using System.Windows;
using Bio.Data;
using Bio.Data.Interfaces;

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

namespace Bio.Views.Structure.ViewModels
{
    class NestedElementViewModel : NestedElementBaseViewModel
    {
        private Point _position;
        public Point Position
        {
            get { return _position; }
            private set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        private Point _nextPosition;
        public Point NextPosition
        {
            get { return _nextPosition; }
            private set
            {
                _nextPosition = value;
                OnPropertyChanged("NextPosition");
            }
        }

        private int _currentIndex;
        public int Index
        {
            get { return _currentIndex + 1; }
            private set { _currentIndex = value; }
        }

        public IBioSymbol Symbol { get; private set; }

        public double Width { get { return Parent.ElementSpacing; } }

        public string LinePath
        {
            get 
            { 
                return string.Format("M {0},{1} L {2},{3}", Position.X, Position.Y,
                    NextPosition.X, NextPosition.Y); 
            }
        }

        public NestedElementViewModel(NestedViewModel parent, IBioSymbol symbol, int index) : base(parent)
        {
            Symbol = symbol;
            Index = index;
            ComputePosition();
        }

        public override string ToString()
        {
            return string.Format("Nt: {0}", Index);
        }

        private void ComputePosition()
        {
            Position = new Point()
            {
                X = Parent.StartX + _currentIndex * Parent.ElementSpacing,
                Y = Parent.StartY
            };

            NextPosition = new Point()
            {
                X = Parent.StartX + (_currentIndex+1) * Parent.ElementSpacing,
                Y = Parent.StartY
            };
        }
    }
}
