using System.Windows.Media;

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
    public class SSLineLabelViewModel : SSElementBaseViewModel
    {
        private double _x1;
        public double X1
        {
            get { return _x1; }
            set { _x1 = value; }
        }

        private double _y1;
        public double Y1
        {
            get { return _y1; }
            set { _y1 = value; }
        }

        private double _thickness;
        public double Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        private Brush _color;
        public Brush Color
        {
            get { return _color; }
            set { _color = value; }
        }

        private int _attachedNtIdx;
        public int AttachedNtIdx
        {
            get { return _attachedNtIdx + 1; }
            set { _attachedNtIdx = value; }
        }

        public SSLineLabelViewModel()
        {
            Width = 5.0;
            Height = 5.0;
        }

        public override void ShiftX(double shiftBy)
        {
            X = X + shiftBy;
        }

        public override void ShiftY(double shiftBy)
        {
            Y = Y + shiftBy;
        }

        public override string ToString()
        {
            return string.Format("Start:({0},{1}), End:({2},{3})", X, Y, X + X1, Y + Y1);
        }
    }
}
