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
    class NestedBasePairViewModel : NestedElementBaseViewModel
    {
        public NestedElementViewModel FivePrimeElement { get; private set; }
        public NestedElementViewModel ThreePrimeElement { get; private set; }

        public string Connector
        {
            get 
            {
                double height = Parent.ComputeBasePairHeight(BasePairDistance);
                double ypos = FivePrimeElement.Position.Y - height;
                return string.Format("M {0},{1} L {2},{3} L {4},{5} L {6},{7}", FivePrimeElement.Position.X,
                    FivePrimeElement.Position.Y, FivePrimeElement.Position.X, ypos, ThreePrimeElement.Position.X,
                    ypos, ThreePrimeElement.Position.X, ThreePrimeElement.Position.Y); 
            }
        }

        public int BasePairDistance
        {
            get { return ThreePrimeElement.Index - FivePrimeElement.Index - 1; }
        }

        public NestedBasePairViewModel(NestedViewModel parent, NestedElementViewModel fivePrimeElement, 
            NestedElementViewModel threePrimeElement) : base(parent)
        {
            FivePrimeElement = fivePrimeElement;
            ThreePrimeElement = threePrimeElement;
        }

        public override string ToString()
        {
            return string.Format("BP: ({0},{1}), Distance: {2}", FivePrimeElement.Index,
                ThreePrimeElement.Index, BasePairDistance);
        }
    }
}
