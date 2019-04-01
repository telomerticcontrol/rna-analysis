using JulMar.Windows.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;
using System;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Bio.Views.Alignment.Internal
{
    public class HeaderRulerIndex : FrameworkElement
    {
        private FormattedText _text;
        private ColumnSelectionAdorner _cursor;
        private readonly TextBlock _toolTipText;
        private double _visibleColumns, _cellSize;

        #region FontFamily
        /// <summary> 
        /// DependencyProperty for <see cref="FontFamily" /> property.
        /// </summary> 
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(HeaderRulerIndex),
                            new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// The FontFamily property specifies the name of font family. 
        /// </summary> 
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }
        #endregion

        #region FontSize
        /// <summary>
        ///     The DependencyProperty for the FontSize property. 
        ///     Flags:              Can be used in style rules
        ///     Default Value:      System Dialog Font Size 
        /// </summary> 
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(HeaderRulerIndex),
                                                                                                           new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary> 
        ///     The size of the desired font. 
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing. 
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        #endregion

        #region Column
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register("Column", typeof(int), typeof(HeaderRulerIndex), new FrameworkPropertyMetadata(0,FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public int Column
        {
            get { return (int)base.GetValue(ColumnProperty); }
            set { base.SetValue(ColumnProperty, value); }
        }
        #endregion

        #region Foreground
        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(HeaderRulerIndex), new FrameworkPropertyMetadata(Brushes.Black));
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        #endregion

        #region DisplayIncrement
        public static readonly DependencyProperty DisplayIncrementProperty = DependencyProperty.Register("DisplayIncrement", typeof(int), typeof(HeaderRulerIndex), new FrameworkPropertyMetadata(10,FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public int DisplayIncrement
        {
            get { return (int)base.GetValue(DisplayIncrementProperty); }
            set { base.SetValue(DisplayIncrementProperty, value); }
        }
        #endregion

        #region CurrentColumn
        public static readonly DependencyProperty CurrentColumnProperty = DependencyProperty.Register("CurrentColumn", typeof(int), typeof(HeaderRulerIndex), new FrameworkPropertyMetadata(-1, OnCurrentColumnChanged));
        public int CurrentColumn
        {
            get { return (int)base.GetValue(CurrentColumnProperty); }
            set { base.SetValue(CurrentColumnProperty, value); }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public HeaderRulerIndex()
        {
            _toolTipText = new TextBlock();
            ToolTip = _toolTipText;
            ToolTipService.SetShowDuration(this, 30000);
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
            var ft = new FormattedText("W", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), FontSize, Brushes.Black);
            _cellSize = ft.Width;

            if (!Double.IsInfinity(availableSize.Width))
            {
                _visibleColumns = (int) (availableSize.Width/_cellSize);
                return new Size(_visibleColumns*_cellSize, ft.Height + 5);
            }

            // No size known yet - this will be recalculated later.
            _visibleColumns = 10;
            return new Size();
        }

        /// <summary>
        /// Invoked when an unhandled attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point pt = e.GetPosition(this);
            int column = Column + (int)(pt.X / _cellSize) + 1;
            _toolTipText.Text = column.ToString();

            ToolTip toolTip = _toolTipText.FindVisualParent<ToolTip>();
            if (toolTip != null)
            {
                toolTip.Placement = PlacementMode.Relative;
                toolTip.PlacementTarget = this;
                toolTip.HorizontalOffset = pt.X;
                toolTip.VerticalOffset = ActualHeight+10;
            }
        }

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing. 
        /// </summary>
        /// <param name="dc">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void  OnRender(DrawingContext dc)
        {
            // Provide a hit-testable surface for tooltip work
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0,0,ActualWidth,ActualHeight));

            Pen markerPen = new Pen(Foreground, 1 + FontSize/14);

            // Draw markers
            for (int i = Column+1; i <= Column+_visibleColumns; i++)
            {
                if (i==1 || i % DisplayIncrement == 0)
                {
                    _text = new FormattedText(i.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), FontSize, Foreground);

                    Point pos = new Point(_cellSize * (i-Column) - (_cellSize / 2) - _text.Width/2, 0);

                    dc.DrawText(_text, pos);
                    dc.DrawLine(markerPen, new Point(_cellSize * (i - Column) - (_cellSize / 2), _text.Height), new Point(_cellSize * (i - Column) - (_cellSize / 2), ActualHeight));
                }
            }

            // Reposition cursor
            RepositionColumnPosition(CurrentColumn);
        }

        /// <summary>
        /// Called when the current column changes
        /// </summary>
        /// <param name="dpo"></param>
        /// <param name="e"></param>
        private static void OnCurrentColumnChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            HeaderRulerIndex hri = (HeaderRulerIndex) dpo;
            hri.RepositionColumnPosition((int) e.NewValue);
        }

        /// <summary>
        /// Repositions the cursor marker.
        /// </summary>
        /// <param name="newValue"></param>
        private void RepositionColumnPosition(int newValue)
        {
            if (newValue >= 0)
            {
                if (_cursor == null)
                {
                    if (AdornerLayer.GetAdornerLayer(this) != null)
                    {
                        _cursor = new ColumnSelectionAdorner(this)
                        {
                            CellSize = new Size(_cellSize, ActualHeight / 4),
                            SelectionBrush = Foreground,
                            Position = new Point(_cellSize * (newValue - Column), (_text != null) ? _text.Height : 0)
                        };
                    }
                }
                else
                    _cursor.Position = new Point(_cellSize * (newValue - Column), (_text != null) ? _text.Height : 0);
            }
            else
            {
                if (_cursor != null)
                {
                    _cursor.Dispose();
                    _cursor = null;
                }
            }
        }
    }
}