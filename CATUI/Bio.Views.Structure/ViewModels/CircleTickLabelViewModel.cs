using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace Bio.Views.Structure.ViewModels
{
    public class CircleTickLabelViewModel : CircleElementBaseViewModel
    {
        private string _tickText;
        public string TickText
        {
            get { return _tickText; }
            private set { _tickText = value; }
        }

        private Typeface _typeface;
        public Typeface Typeface
        {
            get { return _typeface; }
            private set { _typeface = value; }
        }

        private double _labelSize;
        public double LabelSize
        {
            get { return _labelSize; }
            private set { _labelSize = value; }
        }

        public CircleTickLabelViewModel(double tickX, double tickY, double padding,
            double rayAngle, int index)
        {
            Typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            LabelSize = 14;
            TickText = index.ToString();
            _index = index;
            _labelText = new FormattedText(TickText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, LabelSize, Brushes.Black);
            _rayAngle = rayAngle;
            _padding = padding;
            _tickPoint = new Point() { X = tickX, Y = tickY };
            ComputeLabelPosition();
        }

        private double _padding;
        private int _index;
        private double _rayAngle;
        private Point _tickPoint;
        private FormattedText _labelText;

        private void ComputeLabelPosition()
        {
            double labelWidth = _labelText.WidthIncludingTrailingWhitespace;
            double labelHeight = _labelText.Height;
            Point anchorPoint = new Point()
            {
                X = _tickPoint.X + _padding * Math.Cos(_rayAngle * (Math.PI / 180.0)),
                Y = _tickPoint.Y + _padding * Math.Sin(_rayAngle * (Math.PI / 180.0))
            };

            
            if(_rayAngle >= 0 && _rayAngle <= 90)
            {
                X = anchorPoint.X;
                Y = anchorPoint.Y;
            }
            else if (_rayAngle > 90 && _rayAngle < 270)
            {
                X = anchorPoint.X - labelWidth;
                Y = anchorPoint.Y;
            }
            else
            {
                X = anchorPoint.X;
                Y = anchorPoint.Y;
            }
        }
    }
}
