using System.Windows.Media;
using System.Windows;
using System;

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
    public class SSParallelogramLabelViewModel : SSElementBaseViewModel
    {
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

        private double _centerX;
        public double CenterX
        {
            get { return _centerX; }
            set { _centerX = value; }
        }

        private double _centerY;
        public double CenterY
        {
            get { return _centerY; }
            set { _centerY = value; }
        }

        private double _rotationAngle;
        public double RotationAngle
        {
            get { return _rotationAngle; }
            set { _rotationAngle = value; }
        }

        private double _parallelogramAngle;
        public double ParallelogramAngle
        {
            get { return _parallelogramAngle; }
            set { _parallelogramAngle = value; }
        }

        private double _side1Length;
        public double Side1Length
        {
            get { return _side1Length; }
            set { _side1Length = value; }
        }

        private double _side2Length;
        public double Side2Length
        {
            get { return _side2Length; }
            set { _side2Length = value; }
        }

        public PointCollection VertexPoints
        {
            get;
            private set;
        }

        /// <summary>
        /// This implementation is too heavily tied to the XRNA format as far as 
        /// how the geometry is specified, we may want to do some of the conversion in
        /// the provider.
        /// </summary>
        public SSParallelogramLabelViewModel()
        {
            VertexPoints = new PointCollection();
        }

        public void ComputeParallelogram()
        {
            ComputeEdgePoints();
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
            return string.Format("Anchor:({0},{1}), Rotation Angle: {2}", X, Y, RotationAngle);
        }

        private void ComputeEdgePoints()
        {
            VertexPoints.Clear();

            Point a = new Point() { X = 0, Y = 0 };
            Point b = new Point()
            {
                X = Math.Cos((360.0 - RotationAngle) * (Math.PI / 180.0)) * Side1Length,
                Y = Math.Sin((360.0 - RotationAngle) * (Math.PI / 180.0)) * Side1Length
            };
            Point c = new Point()
            {
                X = b.X - Math.Cos((180.0 - ParallelogramAngle) * (Math.PI / 180.0)) * Side2Length,
                Y = b.Y - Math.Sin((180.0 - ParallelogramAngle) * (Math.PI / 180.0)) * Side2Length
            };
            Point d = new Point()
            {
                X = c.X - (Math.Cos((360.0 - RotationAngle) * (Math.PI / 180.0)) * Side1Length),
                Y = c.Y - (Math.Sin((360.0 - RotationAngle) * (Math.PI / 180.0)) * Side1Length)
            };

            VertexPoints.Add(a);
            VertexPoints.Add(b);
            VertexPoints.Add(c);
            VertexPoints.Add(d);

            Y = CenterY - ((a.Y + b.Y + c.Y + d.Y) / 4.0);
            X = CenterX - ((a.X + b.X + c.X + d.X) / 4.0);
        }
    }
}
