using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Bio.Controls
{
    public class HueSelector : FrameworkElement
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), 
                    typeof(HueSelector), new UIPropertyMetadata(Colors.Red));
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            private set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorBrushProperty = DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), 
                    typeof(HueSelector), new UIPropertyMetadata(Brushes.Red));
        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            private set { SetValue(ColorBrushProperty, value); }
        }

        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), 
                    typeof(HueSelector), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(HueChanged), HueCoerce));
        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public static void HueChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            ((HueSelector) o).UpdateColor();
        }

        public static object HueCoerce(DependencyObject d, object value)
        {
            double num = (double) value;
            return (num < 0.0) ? 0.0 : (num > 1.0) ? 1.0 : num;
        }

        private void UpdateColor()
        {
            Color = ColorUtilities.ColorFromAhsb(0xff, Hue, 1.0, 1.0);
            ColorBrush = new SolidColorBrush(Color);
            InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            CalculateHue(e.GetPosition(null));
            Mouse.Capture(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                CalculateHue(e.GetPosition(this));
        }

        private void CalculateHue(Point position)
        {
            double hue = 1.0 - (position.Y/ActualHeight);
            if (hue > 1.0)
                hue = 1.0;
            else if (hue < 0.0)
                hue = 0.0;
            Hue = hue;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.0, 0.0), EndPoint = new Point(0.0, 1.0),
                GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(0xff, 0, 0), 1.0),          
                        new GradientStop(Color.FromRgb(0xff, 0xff, 0), 0.85),
                        new GradientStop(Color.FromRgb(0, 0xff, 0), 0.76),
                        new GradientStop(Color.FromRgb(0, 0xff, 0xff), 0.5),
                        new GradientStop(Color.FromRgb(0, 0, 0xff), 0.33),
                        new GradientStop(Color.FromRgb(0xff, 0, 0xff), 0.16),
                        new GradientStop(Color.FromRgb(0xff, 0, 0), 0.0),
                    }
            };

            Pen blackPen = new Pen(Brushes.Black, 1);

            drawingContext.DrawRectangle(brush, blackPen, new Rect(0.0, 0.0, ActualWidth, ActualHeight));

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));

            double hueOffset = ActualHeight - (ActualHeight * Hue);
            double triangleSizeX = ActualWidth/3;
            double triangleSizeY = ActualWidth - ActualWidth/4;
            Point pos = new Point(0, hueOffset - triangleSizeY/2);

            PathGeometry triangle = new PathGeometry(new[] {
                 new PathFigure(pos, new[] {
                       new LineSegment(new Point(triangleSizeX + pos.X, pos.Y + triangleSizeY/2),true),
                       new LineSegment(new Point(pos.X, pos.Y + triangleSizeY),true),
                   }, true)
             });

            drawingContext.DrawGeometry(Brushes.Black, null, triangle);

            pos.X = ActualWidth;
            triangle = new PathGeometry(new[] {
                 new PathFigure(pos, new[] {
                       new LineSegment(new Point(pos.X - triangleSizeX, pos.Y + triangleSizeY/2),true),
                       new LineSegment(new Point(pos.X, pos.Y + triangleSizeY),true),
                   }, true)
             });
            drawingContext.DrawGeometry(Brushes.Black, null, triangle);

            drawingContext.Pop();
        }

    }

}
