using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Bio.SharedResources
{
    /// <summary>
    /// This panel provides a proporotional sizing panel that resizes it's children
    /// based on the available space and their requirements.
    /// </summary>
    public class ProportionalPanel : Panel
    {
        /// <summary>
        /// Orientation of the panel
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ProportionalPanel),
                                        new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Orientation of the panel
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
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
            if (InternalChildren.Count == 0)
                return new Size();

            double minSize = double.PositiveInfinity;

            Size usedSize = availableSize;
            if (Orientation == Orientation.Horizontal)
            {
                usedSize.Width = double.PositiveInfinity;
                if (!double.IsInfinity(availableSize.Width))
                    minSize = availableSize.Width / InternalChildren.Count;
            }
            else
            {
                usedSize.Height = double.PositiveInfinity;
                if (!double.IsInfinity(availableSize.Height))
                    minSize = availableSize.Height / InternalChildren.Count;
            }

            // Go through and get desired sizes - including the size in the axis we are
            // proportionally sizing to
            foreach (UIElement element in InternalChildren)
                element.Measure(usedSize);

            // Take out reserved space - these are the elements that are requesting less size than our minimum.
            if (!double.IsInfinity(minSize))
            {
                IEnumerable<UIElement> reservedElements;

                if (Orientation == Orientation.Horizontal)
                {
                    reservedElements = InternalChildren.Cast<UIElement>().Where(child => child.DesiredSize.Width < minSize);
                    if (reservedElements.Count() != InternalChildren.Count)
                    {
                        double reservedSize = reservedElements.Select(child => child.DesiredSize.Width).Sum();
                        minSize = (availableSize.Width - reservedSize) /
                                  (InternalChildren.Count - reservedElements.Count());
                        foreach (UIElement child in InternalChildren.Cast<UIElement>().Except(reservedElements))
                            child.Measure(new Size(minSize, child.DesiredSize.Height));
                    }
                }
                else
                {
                    reservedElements = InternalChildren.Cast<UIElement>().Where(child => child.DesiredSize.Height < minSize);
                    if (reservedElements.Count() != InternalChildren.Count)
                    {
                        double reservedSize = reservedElements.Select(child => child.DesiredSize.Height).Sum();
                        minSize = (availableSize.Height - reservedSize) / (InternalChildren.Count - reservedElements.Count());
                        foreach (UIElement child in InternalChildren.Cast<UIElement>().Except(reservedElements))
                            child.Measure(new Size(child.DesiredSize.Width, minSize));
                    }
                }
            }

            // Total up the required space
            Size neededSize = new Size(0, 0);
            foreach (UIElement element in InternalChildren)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    neededSize.Width += element.DesiredSize.Width;
                    if (neededSize.Height < element.DesiredSize.Height)
                        neededSize.Height = element.DesiredSize.Height;
                }
                else
                {
                    neededSize.Height += element.DesiredSize.Height;
                    if (neededSize.Width < element.DesiredSize.Width)
                        neededSize.Width = element.DesiredSize.Width;
                }

            }

            return neededSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class. 
        /// </summary>
        /// <returns>
        /// The actual size used.
        /// </returns>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count == 0)
                return finalSize;

            double pad = 0;
            List<UIElement> visibleChildren;

            if (Orientation == Orientation.Horizontal)
            {
                visibleChildren = InternalChildren.Cast<UIElement>().Where(child => child.DesiredSize.Width > 0).ToList();
                double width = InternalChildren.Cast<UIElement>().Sum(child => child.DesiredSize.Width);
                if (width < finalSize.Width)
                    pad = (finalSize.Width - width);
            }
            else
            {
                visibleChildren = InternalChildren.Cast<UIElement>().Where(child => child.DesiredSize.Height > 0).ToList();
                double height = InternalChildren.Cast<UIElement>().Sum(child => child.DesiredSize.Height);
                if (height < finalSize.Height)
                    pad = (finalSize.Height - height);
            }
               

            var finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);
            for (int index = 0; index < visibleChildren.Count; index++)
            {
                UIElement element = visibleChildren[index];
                if (Orientation == Orientation.Horizontal)
                {
                    finalRect.Width = element.DesiredSize.Width;
                    if (index == visibleChildren.Count - 1)
                        finalRect.Width += pad;
                    element.Arrange(finalRect);
                    finalRect.X += finalRect.Width;
                }
                else
                {
                    finalRect.Height = element.DesiredSize.Height;
                    if (index == visibleChildren.Count - 1)
                        finalRect.Height += pad;
                    element.Arrange(finalRect);
                    finalRect.Y += finalRect.Height;
                }
            }
            return finalSize;
        }
    }

    public class DebugDecorator : Decorator
    {
        protected override Size MeasureOverride(Size constraint)
        {
            if (Child != null)
            {
                Size sz = constraint;
                if (sz.Width == 0)
                    sz.Width = double.PositiveInfinity;
                if (sz.Height == 0)
                    sz.Height = double.PositiveInfinity;

                Child.Measure(sz);
                return Child.DesiredSize;
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child != null)
            {
                Child.Arrange(new Rect(new Point(), Child.DesiredSize));
            }

            return arrangeSize;
        }
    }
}


