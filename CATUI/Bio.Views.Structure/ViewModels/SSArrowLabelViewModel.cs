using System.Windows.Media;
using System.Windows;

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
    public class SSArrowLabelViewModel : SSElementBaseViewModel
    {
        private PointCollection _edgePoints;
        public PointCollection EdgePoints
        {
            get { return _edgePoints; }
            private set { _edgePoints = value; }
        }

        private Brush _color;
        public Brush Color
        {
            get { return _color; }
            set { _color = value; }
        }

        private double _lineWeight;
        public double LineWeight
        {
            get { return _lineWeight; }
            set { _lineWeight = value; }
        }

        private double _tailLength;
        public double TailLength
        {
            get { return _tailLength; }
            set { _tailLength = value; }
        }

        private double _leftTipX;
        public double LeftTipX
        {
            get { return _leftTipX; }
            set { _leftTipX = value; }
        }

        private double _leftTipY;
        public double LeftTipY
        {
            get { return _leftTipY; }
            set { _leftTipY = value; }
        }

        private double _angle;
        public double RotationAngle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        public double ArrowTipX
        {
            get { return X; }
            set { X = value; }
        }

        public double ArrowTipY
        {
            get { return Y; }
            set { Y = value; }
        }

        public SSArrowLabelViewModel()
        {
            EdgePoints = new PointCollection();
        }

        public void ComputeArrow()
        {
            EdgePoints.Add(new Point() { X = 0.0, Y = 0.0 });
            EdgePoints.Add(new Point() { X = LeftTipX, Y = LeftTipY });
            EdgePoints.Add(new Point() { X = LeftTipX, Y = 0.0 });
            EdgePoints.Add(new Point()
            {
                X = LeftTipX + (-1 * TailLength),
                Y = 0
            });
            EdgePoints.Add(new Point() { X = LeftTipX, Y = 0.0 });
            EdgePoints.Add(new Point() { X = LeftTipX, Y = -1 * LeftTipY });
            EdgePoints.Add(new Point() { X = 0.0, Y = 0.0 });
        }
    }
}
