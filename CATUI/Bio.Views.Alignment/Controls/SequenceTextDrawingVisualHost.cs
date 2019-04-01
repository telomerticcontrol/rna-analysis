using System;
using System.Windows;
using System.Windows.Media;
using Bio.Views.Alignment.Internal;

namespace Bio.Views.Alignment.Controls
{
    /// <summary>
    /// This class renders a single DrawingVisual as a FrameworkElement.
    /// </summary>
    public class SequenceTextDrawingVisualHost : FrameworkElement
    {
        private SequenceTextDrawingVisual _visual;

        /// <summary>
        /// The drawing to render
        /// </summary>
        public SequenceTextDrawingVisual Child
        {
            get
            {
                return _visual;
            }

            private set
            {
                if (ReferenceEquals(value, _visual))
                    return;

                if (_visual != null)
                {
                    RemoveVisualChild(_visual);
                    RemoveLogicalChild(_visual);
                }

                _visual = value;
                if (_visual != null)
                {
                    AddVisualChild(_visual);
                    AddLogicalChild(_visual);
                }

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// This is called to create the visual child
        /// </summary>
        /// <param name="owner"></param>
        internal void DrawVisualSequence(SequenceBlock owner)
        {
            // Nothing to do
            if (owner == null || owner.Sequence == null || owner.Count == 0)
                Child = null;
            else
            {

                if (Child == null || Child.FontFamily != owner.FontFamily || Child.FontSize != owner.FontSize)
                    Child = new SequenceTextDrawingVisual(owner.SequenceColorSelector, owner.Sequence, owner.FontFamily,
                                                          owner.FontSize);
                Child.Render(owner.Column, owner.Count);
            }
        }

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and determines a size for the <see cref="T:System.Windows.FrameworkElement"/>-derived class. 
        /// </summary>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations of child element sizes.
        /// </returns>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        protected override Size MeasureOverride(Size availableSize) 
        {
            Size safeBounds = new Size(GetSafeValue(availableSize.Width), GetSafeValue(availableSize.Height));
            Size bounds = _visual != null ? Child.ContentBounds.Size : safeBounds;
            return bounds.IsEmpty ? safeBounds : bounds;   
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>
        /// The number of visual child elements for this element.
        /// </returns>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements. 
        /// </summary>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException("index");

            return _visual;
        }

        /// <summary>
        /// Returns "safe" bounds when Double.NaN is present.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static private double GetSafeValue(double value) 
        {   
            return (double.IsNaN(value) || double.IsInfinity(value)) ? 0.0 : value;   
        }
    }
}
