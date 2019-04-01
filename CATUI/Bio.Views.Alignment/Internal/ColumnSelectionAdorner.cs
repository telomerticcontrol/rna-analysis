using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System;

namespace Bio.Views.Alignment.Internal
{
    /// <summary>
    /// This draws the focus rectangle on a given column.  
    /// </summary>
    internal class ColumnSelectionAdorner : Adorner, IDisposable
    {
        private Point _position;
        public Point Position
        {
            get { return _position; }
            set { _position = value; InvalidateVisual(); }
        }

        private Size _cellSize;
        public Size CellSize
        {
            get { return _cellSize; }
            set { _cellSize = value; InvalidateVisual();}
        }

        public Brush SelectionBrush { get; set; }

        public ColumnSelectionAdorner(UIElement columnHeader) : base(columnHeader)
        {
            IsHitTestVisible = false;
            AdornerLayer.GetAdornerLayer(columnHeader).Add(this);
        }

        protected override void OnRender(DrawingContext dc)
        {
            // Do not render for out of bounds positions.
            if (_position.X < 0 || _position.X > ActualWidth)
                return;

            PathGeometry triangle = new PathGeometry(new[] {
                 new PathFigure(_position, new[] {
                       new LineSegment(new Point(_cellSize.Width +_position.X,_position.Y),true),
                       new LineSegment(new Point(_cellSize.Width/2 +_position.X,_position.Y + _cellSize.Height),true),
                   }, true)
             });

            dc.DrawGeometry(SelectionBrush, new Pen(SelectionBrush, 1), triangle);
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