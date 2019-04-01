using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Bio.Controls
{
    public class ColorSaturationSelector : FrameworkElement
    {
        public event EventHandler ColorChanged;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ColorSaturationSelector), 
                        new UIPropertyMetadata(Colors.Black, OnColorChanged));
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorBrushProperty = DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), typeof(ColorSaturationSelector), new UIPropertyMetadata(Brushes.Black));
        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            private set { SetValue(ColorBrushProperty, value); }
        }

        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(ColorSaturationSelector),
                        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, HsbChanged));

        public double Hue
        {
            private get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(ColorSaturationSelector),
                                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, HsbChanged, SaturationCoerce));

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(ColorSaturationSelector),
                                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, HsbChanged, BrightnessCoerce));

        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static void HsbChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            ((ColorSaturationSelector) o).UpdateColor();
        }

        public static object BrightnessCoerce(DependencyObject d, object value)
        {
            double num = (double)value;
            return (num < 0.0) ? 0.0 : num > 1.0 ? 1.0 : num;
        }

        public static object SaturationCoerce(DependencyObject d, object value)
        {
            double num = (double)value;
            return (num < 0.0) ? 0.0 : (num > 1.0) ? 1.0 : num;
        }

        private static void OnColorChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            ColorSaturationSelector css = (ColorSaturationSelector) dpo;
            if (!css._updatingColor)
            {
                css._updatingColor = true;
                double h, s, b;
                css.Color.ToHsb(out h, out s, out b);
                css.Hue = h;
                css.Saturation = s;
                css.Brightness = b;
                css._updatingColor = false;
            }

            css.ColorBrush = new SolidColorBrush(css.Color);
            css.CalculateMarkerPosition();

            var cce = css.ColorChanged;
            if (cce != null)
                cce.Invoke(css, EventArgs.Empty);
        }

        private bool _updatingColor;
        public void UpdateColor()
        {
            if (!_updatingColor)
            {
                _updatingColor = true;
                Color = ColorUtilities.ColorFromAhsb(0xff, Hue, Saturation, Brightness);
                _updatingColor = false;
            }
        }

        /// <summary>
        /// Determines the position of the marker
        /// </summary>
        private Point CalculateMarkerPosition()
        {
            // X = S, Y = B
            return new Point {X = Saturation*ActualWidth, Y = ActualHeight + (-1*Brightness*ActualHeight)};
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            CalculateSb(e.GetPosition(this));
            Mouse.Capture(this);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseMove"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                CalculateSb(e.GetPosition(this));
        }

        /// <summary>
        /// This is used to calculate the saturation/brightness values
        /// </summary>
        /// <param name="position">Position within selector</param>
        private void CalculateSb(Point position)
        {
            Saturation = position.X / ActualWidth;
            Brightness = (ActualHeight - position.Y) / ActualHeight;
            UpdateColor();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseUp"/> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing. 
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            Pen blackPen = new Pen(Brushes.Black, 1), whitePen = new Pen(Brushes.White, 3);

            // Draw the color as two overlapping gradients.
            LinearGradientBrush brush = new LinearGradientBrush 
            {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(1.0, 0.0),
                GradientStops =
                    {
                        new GradientStop(Colors.White, 0.0), 
                        new GradientStop(ColorUtilities.ColorFromAhsb(0xff,Hue,1,1), 1.0)
                    }
            };
            drawingContext.DrawRectangle(brush, null, new Rect(0.0, 0.0, ActualWidth, ActualHeight));
            
            brush = new LinearGradientBrush 
            {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(0.0, 1.0),
                GradientStops = 
                { 
                    new GradientStop(Colors.Black, 1.0), 
                    new GradientStop(Color.FromArgb(0x80,0,0,0), 0.5), 
                    new GradientStop(Color.FromArgb(0,0,0,0), 0.0) 
                }
            };
            drawingContext.DrawRectangle(brush, blackPen, new Rect(0.0, 0.0, ActualWidth, ActualHeight));

            // Draw the current position
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));

            Point markerCenter = CalculateMarkerPosition();
            drawingContext.DrawEllipse(null, whitePen, markerCenter, 4, 4);
            drawingContext.DrawEllipse(null, blackPen, markerCenter, 4, 4);

            drawingContext.Pop();
        }
    }
}
