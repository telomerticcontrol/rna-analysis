using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Data.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace Bio.Views.Structure.ViewModels
{
    public class CircleSequenceViewModel : CircleElementBaseViewModel
    {
        public List<CircleElementViewModel> Elements { get; set; }
        public List<CircleTickMarkViewModel> TickMarks { get; set; }
        public List<CircleTickLabelViewModel> TickLabels { get; set; }

        public double Width
        {
            get { return 1.20 * _diameter; }
        }

        public double Height
        {
            get { return 1.20 * _diameter; }
        }

        public double Circumference
        {
            get { return Math.PI * Diameter; }
        }

        private Point _origin;
        public Point Origin
        {
            get { return _origin; }
            private set { _origin = value; }
        }

        public double _elementSpacing;
        public double ElementSpacing
        {
            get { return _elementSpacing; }
            private set { _elementSpacing = value; }
        }

        public double _diameter;
        public double Diameter
        {
            get { return _diameter; }
            set
            {
                _diameter = value;
                Origin = new Point() { X = Width / 2.0, Y = Height / 2.0 };
                X = (Origin.X - _diameter / 2.0);
                Y = (Origin.Y - _diameter / 2.0);
                ElementSpacing = Circumference / Sequence.RawData.Count;
            }
        }

        private IBioEntity _sequence;
        public IBioEntity Sequence
        {
            get { return _sequence; }
            private set { _sequence = value; }
        }

        public CircleSequenceViewModel(IBioEntity sequence)
        {
            Sequence = sequence;
            Elements = new List<CircleElementViewModel>();
            TickMarks = new List<CircleTickMarkViewModel>();
            TickLabels = new List<CircleTickLabelViewModel>();
            SetDiameter();
            BuildElements();
            BuildTickMarks();
        }

        private double _maxDiameter = 700.0;
        private double _minDiameter = 250.0;

        public double ComputeRayAngle(int index)
        {
            double offset = index * ElementSpacing;
            return (2 * Math.PI) * (offset / (Math.PI * Diameter));
        }

        public double ComputePathDistanceBetween(int index1, int index2)
        {
            double rayAngleidx1 = ComputeRayAngle(index1);
            double rayAngleidx2 = ComputeRayAngle(index2);
            double path1distance = Circumference * (rayAngleidx1 / (Math.PI * 2));
            double path2distance = Circumference * (rayAngleidx2 / (Math.PI * 2));
            return (index1 <= index2) ? path2distance - path1distance :
                path1distance - path2distance;
        }

        public double ComputeXPos(int index)
        {
            return Origin.X + Math.Cos(ComputeRayAngle(index)) * (Diameter / 2.0);
        }

        public double ComputeYPos(int index)
        {
            return Origin.Y + Math.Sin(ComputeRayAngle(index)) * (Diameter / 2.0);
        }

        public string ComputeBinaryConnector(int index1, int index2, double maxRadiusFactor)
        {
            /*
             * Algorithm:
             * 1. Compute the Path Distance
             * 2. Compute the MidPoint of the Path in Polar Cooridinates.
             * 3. Use Linear Interpolation to determine how far to move the control point along
             * the midpoint ray.
             */
            double pathSegmentLength = ComputePathDistanceBetween(index1, index2);
            double index1RayAngle = ComputeRayAngle(index1);
            double index2RayAngle = ComputeRayAngle(index2);
            Point controlPoint = new Point();
            if (index1 <= index2)
            {
                double midPointRayAngle = index1RayAngle + ((index2RayAngle - index1RayAngle) / 2.0);
                controlPoint = ComputeControlPoint(pathSegmentLength, midPointRayAngle, maxRadiusFactor);
                return string.Format("M {0},{1} Q {2},{3} {4},{5}", ComputeXPos(index1), ComputeYPos(index1),
                    controlPoint.X, controlPoint.Y, ComputeXPos(index2), ComputeYPos(index2));
            }
            else
            {
                double midPointRayAngle = index2RayAngle + ((index1RayAngle - index2RayAngle) / 2.0);
                controlPoint = ComputeControlPoint(pathSegmentLength, midPointRayAngle, maxRadiusFactor);
                return string.Format("M {0},{1} Q {2},{3} {4},{5}", ComputeXPos(index2), ComputeYPos(index2),
                    controlPoint.X, controlPoint.Y, ComputeXPos(index1), ComputeYPos(index1));
            }
        }

        private int ComputeTickIncrement()
        {
            double retValue = 1;
            if (Sequence.RawData.Count() / retValue <= 20)
            {
                return (int)retValue;
            }
            retValue = 5;
            while (Sequence.RawData.Count() / retValue > 20)
            {
                retValue += 5;
            }
            return (int)retValue;
        }

        private double ComputeTickXPos(int index, double tickSize)
        {
            double xCenter = ComputeXPos(index);
            return xCenter - (tickSize / 2.0);
        }

        private double ComputeTickYPos(int index, double tickSize)
        {
            double yCenter = ComputeYPos(index);
            return yCenter - (tickSize / 2.0);
        }

        private Point ComputeControlPoint(double pathDistance, double midPointRayAngle, double maxRadiusFactor)
        {
            double x0 = 0;
            double x1 = Circumference / 2.0;
            double y0 = maxRadiusFactor * (Diameter / 2.0);
            double y1 = 0;

            double ctrlRadius = y0 + (pathDistance - x0) * ((y1 - y0) / (x1 - x0));
            Point ctrlPoint = new Point()
            {
                X = Origin.X + ctrlRadius * (Math.Cos(midPointRayAngle)),
                Y = Origin.Y + ctrlRadius * (Math.Sin(midPointRayAngle))
            };

            return ctrlPoint;
        }

        private void SetDiameter()
        {
            int ntCount = (Sequence.RawData.Count >= 1500) ? 1500 : Sequence.RawData.Count;
            double x0 = 1;
            double x1 = 1500;
            double y0 = _minDiameter;
            double y1 = _maxDiameter;
            Diameter = y0 + (ntCount - x0) * ((y1 - y0) / (x1 - x0));
        }

        private void BuildElements()
        {
            for (int i = 0; i < Sequence.RawData.Count; i++)
            {
                int idx = i + 1;
                int nextIdx = (idx + 1 > Sequence.RawData.Count) ? 1 : i + 2;
                Point idxPt = new Point(ComputeXPos(idx), ComputeYPos(idx));
                Point nextIdxPt = new Point(ComputeXPos(nextIdx), ComputeYPos(nextIdx));
                CircleElementViewModel elementVM = new CircleElementViewModel(Sequence.RawData[i], idx, nextIdx,
                    idxPt, nextIdxPt, (Diameter / 2.0), ComputeRayAngle(idx));
                Elements.Add(elementVM);
            }
        }

        private void BuildTickMarks()
        {
            double tickSize = 10;
            double tickXPos = ComputeTickXPos(1, tickSize);
            double tickYPos = ComputeTickYPos(1, tickSize);
            double tickRayAngle = ComputeRayAngle(1) * (180.0 / Math.PI);
            double tickLabelPadding = 25;
            CircleTickMarkViewModel startTick = new CircleTickMarkViewModel(tickXPos,
                tickYPos, 1, tickSize);
            CircleTickLabelViewModel startTickLabel = new CircleTickLabelViewModel(tickXPos,
                tickYPos, tickLabelPadding, tickRayAngle, 1);
            TickMarks.Add(startTick);
            TickLabels.Add(startTickLabel);
            int tickIncrement = ComputeTickIncrement();
            int currentTickIndex = tickIncrement;
            do
            {
                tickXPos = ComputeTickXPos(currentTickIndex, tickSize);
                tickYPos = ComputeTickYPos(currentTickIndex, tickSize);
                tickRayAngle = ComputeRayAngle(currentTickIndex) * (180.0 / Math.PI);
                CircleTickMarkViewModel tick = new CircleTickMarkViewModel(tickXPos,
                    tickYPos, currentTickIndex, 10);
                CircleTickLabelViewModel label = new CircleTickLabelViewModel(tickXPos,
                    tickYPos, tickLabelPadding, tickRayAngle, currentTickIndex);
                TickMarks.Add(tick);
                TickLabels.Add(label);
                currentTickIndex += tickIncrement;
            } while (currentTickIndex <= _sequence.RawData.Count);
        }
    }
}
