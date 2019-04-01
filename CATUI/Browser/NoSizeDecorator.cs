using System;
using System.Windows;
using System.Windows.Controls;

namespace BioBrowser
{
    /// <summary>
    /// Decorator that keeps an element from resizing the parent panel.
    /// </summary>
    public class NoSizeDecorator : Decorator
    {
        /// <summary>
        /// Max height to allow
        /// </summary>
        public double MaxElementHeight { get; set; }

        /// <summary>
        /// Measures the child element of a <see cref="T:System.Windows.Controls.Decorator"/> to prepare for arranging it during the <see cref="M:System.Windows.Controls.Decorator.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <returns>
        /// The target <see cref="T:System.Windows.Size"/> of the element.
        /// </returns>
        /// <param name="constraint">An upper limit <see cref="T:System.Windows.Size"/> that should not be exceeded.</param>
        protected override Size MeasureOverride(Size constraint)
        {
            double childHeight = 0.0;
            if (Child != null)
            {
                Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                childHeight = Child.DesiredSize.Height;
            }

            if (double.IsInfinity(constraint.Width))
                constraint.Width = 0;
            if (double.IsInfinity(constraint.Height))
                constraint.Height = Math.Max(MaxElementHeight, childHeight);
            return constraint;
        }
    }
}
