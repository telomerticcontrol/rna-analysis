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
    public class SSArcLabelViewModel : SSElementBaseViewModel
    {
        public Geometry _curve;
        public Geometry Curve
        {
            get { return _curve; }
            private set { _curve = value; }
        }

        public double _radius;
        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public double _lineWeight;
        public double LineWeight
        {
            get { return _lineWeight; }
            set { _lineWeight = value; }
        }

        public Brush _color;
        public Brush Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public double _angle1;
        public double Angle1
        {
            get { return _angle1; }
            set { _angle1 = value; }
        }

        public double _angle2;
        public double Angle2
        {
            get { return _angle2; }
            set { _angle2 = value; }
        }

        public double CenterX
        {
            get { return X; }
            set { X = value; }
        }

        public double CenterY
        {
            get { return Y; }
            set { Y = value; }
        }

        public SSArcLabelViewModel() { }

        public void ComputeArc()
        {
            if ((Angle2 - Angle1) % 360.0 == 0 && Angle2 != Angle1)
            {
                EllipseGeometry geometry = new EllipseGeometry();
                geometry.RadiusX = Radius;
                geometry.RadiusY = Radius;
                geometry.Center = new Point() { X = 0.0, Y = 0.0 };
                Curve = geometry;
                return;
            }

            double angle1rads = Angle1 * (Math.PI / 180.0);
            double angle2rads = Angle2 * (Math.PI / 180.0);

            Point startPoint = new Point()
            {
                X = Math.Cos(angle1rads) * Radius,
                Y = -1 * Math.Sin(angle1rads) * Radius
            };

            Point endPoint = new Point()
            {
                X = Math.Cos(angle2rads) * Radius,
                Y = -1 * Math.Sin(angle2rads) * Radius
            };

            PathGeometry val = new PathGeometry();
            val.Figures = new PathFigureCollection();

            PathFigure curveFig = new PathFigure();
            curveFig.StartPoint = startPoint;
            curveFig.Segments = new PathSegmentCollection();

            ArcSegment arc = new ArcSegment();
            arc.Point = endPoint;
            arc.IsLargeArc = (Angle2 - Angle1 >= 180.0) ? true : false;
            arc.SweepDirection = (Angle1 >= 0 && Angle2 >= 0) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            arc.Size = new Size()
            {
                Width = 2 * Radius,
                Height = 2 * Radius
            };

            curveFig.Segments.Add(arc);
            val.Figures.Add(curveFig);
            Curve = val;
        }

        public override string ToString()
        {
            return string.Format("Center: ({0},{1}), Angle1: {2}, Angle2: {3}", CenterX, CenterY, Angle1, Angle2);
        }

    }
}
