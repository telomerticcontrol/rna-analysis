using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System;

namespace Bio.Views.Alignment.Text
{
    public class SelectionAdorner : Adorner, IDisposable
    {
        #region Fill

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(SelectionAdorner),
                new FrameworkPropertyMetadata(Brushes.AliceBlue, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        #endregion

        #region Stroke

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof (Brush), typeof (SelectionAdorner),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        #endregion

        #region Geometry

        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("Geometry", typeof (Geometry), typeof (SelectionAdorner),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Geometry Geometry
        {
            get { return (Geometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        #endregion

        public SelectionAdorner(UIElement text, Geometry geometry, Brush fillBrush, Brush strokeBrush) : base(text)
        {
            Fill = fillBrush;
            Stroke = strokeBrush;
            Geometry = geometry;

            IsHitTestVisible = false;
            Opacity = .33;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(text);
            Debug.Assert(layer != null);
            layer.Add(this);
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawGeometry(Fill, new Pen(Stroke,1), Geometry);
        }

        public void Dispose()
        {
            if (AdornedElement != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornedElement);
                if (layer != null)
                    layer.Remove(this);
            }
        }
    }
}