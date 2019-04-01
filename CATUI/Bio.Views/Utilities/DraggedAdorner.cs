﻿using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Bio.Views.Utilities
{
    /// <summary>
    /// Drag Adorner - this was taken from a sample posted by Bea Stollnitz
    /// See http://www.beacosta.com/blog/?p=53 for the original article.
    /// </summary>
    public class DraggedAdorner : Adorner
    {
        #region Data
        private readonly ContentPresenter _contentPresenter;
        private readonly AdornerLayer _adornerLayer;
        private double _left, _top;
        #endregion

        /// <summary>
        /// Constructor for the drag adorner
        /// </summary>
        /// <param name="dragDropData">Tag data</param>
        /// <param name="dragDropTemplate">Template for visual</param>
        /// <param name="adornedElement">Element we are adorning</param>
        /// <param name="adornerLayer">Adorner layer to insert into</param>
        public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            _adornerLayer = adornerLayer;
            _contentPresenter = new ContentPresenter
                {
                    Content = dragDropData,
                    ContentTemplate = dragDropTemplate,
                    Opacity = 0.7
                };
            _adornerLayer.Add(this);
        }

        /// <summary>
        /// This changes the position of the adorner.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public void SetPosition(double left, double top)
        {
            _left = left;
            _top = top;
            if (_adornerLayer != null)
                _adornerLayer.Update(AdornedElement);
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Size"/> object representing the amount of layout space needed by the adorner.
        /// </returns>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        protected override Size MeasureOverride(Size constraint)
        {
            _contentPresenter.Measure(constraint);
            return _contentPresenter.DesiredSize;
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
            _contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
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
            return _contentPresenter;
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
        /// Returns a <see cref="T:System.Windows.Media.Transform"/> for the adorner, based on the transform that is currently applied to the adorned element.
        /// </summary>
        /// <returns>
        /// A transform to apply to the adorner.
        /// </returns>
        /// <param name="transform">The transform that is currently applied to the adorned element.</param>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_left, _top));

            return result;
        }

        /// <summary>
        /// This removes the item from the adorner layer
        /// </summary>
        public void Detach()
        {
            _adornerLayer.Remove(this);
        }
    }
}


