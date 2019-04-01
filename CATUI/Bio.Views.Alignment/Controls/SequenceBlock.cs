using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Bio.Data.Interfaces;
using System.Windows.Controls;

namespace Bio.Views.Alignment.Controls
{
    /// <summary>
    /// This class renders a single string of text
    /// </summary>
    [TemplatePart(Name = "PART_TextHost", Type=typeof(SequenceTextDrawingVisualHost))]
    public class SequenceBlock : Control
    {
        private SequenceTextDrawingVisualHost _text;

        #region Sequence
        /// <summary>
        /// Sequence Dependency Property
        /// </summary>
        public static readonly DependencyProperty SequenceProperty = DependencyProperty.Register("Sequence", typeof(IList<IBioSymbol>), typeof(SequenceBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the Symbols property. 
        /// </summary>
        public IList<IBioSymbol> Sequence
        {
            get { return (IList<IBioSymbol>)GetValue(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        #endregion

        #region Column
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register("Column", typeof(int), typeof(SequenceBlock), 
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnAdjustFocusIndicator));

        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        static void OnAdjustFocusIndicator(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.InvalidateProperty(ShowFocusIndicatorProperty);
            d.InvalidateProperty(FocusRectangleOffsetProperty);
        }

        #endregion

        #region Count
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(SequenceBlock),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnAdjustFocusIndicator));

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        #endregion

        #region SequenceColorSelector

        /// <summary>
        /// SequenceColorSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty SequenceColorSelectorProperty = DependencyProperty.Register("SequenceColorSelector", typeof(SequenceColorSelector), typeof(SequenceBlock),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the SequenceColorSelector property.  This dependency property 
        /// indicates ....
        /// </summary>
        public SequenceColorSelector SequenceColorSelector
        {
            get { return (SequenceColorSelector)GetValue(SequenceColorSelectorProperty); }
            set { SetValue(SequenceColorSelectorProperty, value); }
        }

        #endregion

        #region CellWidth
        public static readonly DependencyProperty CellWidthProperty = DependencyProperty.Register("CellWidth", typeof(double), 
            typeof(SequenceBlock), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double CellWidth
        {
            get { return (double)GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }
        #endregion

        #region CellHeight
        public static readonly DependencyProperty CellHeightProperty = DependencyProperty.Register("CellHeight", typeof(double), 
            typeof(SequenceBlock), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double CellHeight
        {
            get { return (double)GetValue(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }
        #endregion

        #region ShowFocusIndicator

        /// <summary>
        /// ShowFocusIndicator Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey ShowFocusIndicatorPropertyKey = DependencyProperty.RegisterReadOnly("ShowFocusIndicator", typeof(bool), typeof(SequenceBlock), new FrameworkPropertyMetadata(false, OnFocusIndicatorChanged, OnFocusIndicatorCoerceCallback));
        public static readonly DependencyProperty ShowFocusIndicatorProperty = ShowFocusIndicatorPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the ShowFocusIndicator property.
        /// </summary>
        public bool ShowFocusIndicator
        {
            get { return (bool)GetValue(ShowFocusIndicatorProperty); }
            private set { SetValue(ShowFocusIndicatorPropertyKey, value); }
        }

        static void OnFocusIndicatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (true == (bool)e.NewValue)
            {
                d.InvalidateProperty(FocusRectangleOffsetProperty);
                d.InvalidateProperty(FocusedSymbolProperty);
                d.InvalidateProperty(FocusedSymbolBrushProperty);
            }
        }

        static object OnFocusIndicatorCoerceCallback(DependencyObject d, object baseValue)
        {
            SequenceBlock sb = (SequenceBlock) d;
            int newColumn = sb.FocusedColumnIndex;
            return (newColumn >= sb.Column && newColumn <= sb.Column + sb.Count);
        }

        #endregion

        #region FocusRectangleOffset

        /// <summary>
        /// FocusRectangleOffset Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey FocusRectangleOffsetPropertyKey = DependencyProperty.RegisterReadOnly("FocusRectangleOffset", typeof(double), typeof(SequenceBlock), new FrameworkPropertyMetadata(0.0, null, OnFocusRectangleOffsetCoerceCallback));
        public static readonly DependencyProperty FocusRectangleOffsetProperty = FocusRectangleOffsetPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the FocusRectangleOffset property.  This dependency property 
        /// indicates ....
        /// </summary>
        public double FocusRectangleOffset
        {
            get { return (double)GetValue(FocusRectangleOffsetProperty); }
            private set { SetValue(FocusRectangleOffsetPropertyKey, value); }
        }

        static object OnFocusRectangleOffsetCoerceCallback(DependencyObject d, object baseValue)
        {
            SequenceBlock sb = (SequenceBlock) d;
            return (sb.FocusedColumnIndex - sb.Column) * sb.CellWidth;
        }

        #endregion

        #region FocusedColumnIndex

        /// <summary>
        /// FocusColumnIndex Dependency Property
        /// </summary>
        public static readonly DependencyProperty FocusedColumnIndexProperty = DependencyProperty.Register("FocusedColumnIndex", typeof(int), typeof(SequenceBlock), 
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFocusColumnIndexChanged));

        /// <summary>
        /// Gets or sets the FocusedColumnIndex property.  This dependency property 
        /// </summary>
        public int FocusedColumnIndex
        {
            get { return (int)GetValue(FocusedColumnIndexProperty); }
            set { SetValue(FocusedColumnIndexProperty, value); }
        }

        /// <summary>
        /// Handles changes to the FocusColumnIndex property.
        /// </summary>
        private static void OnFocusColumnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.InvalidateProperty(ShowFocusIndicatorProperty);
            d.InvalidateProperty(FocusRectangleOffsetProperty);
            d.InvalidateProperty(FocusedSymbolProperty);
            d.InvalidateProperty(FocusedSymbolBrushProperty);
        }

        #endregion

        #region IsReferenceSequence

        /// <summary>
        /// IsReferenceSequence Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsReferenceSequenceProperty = DependencyProperty.Register("IsReferenceSequence", typeof(bool), typeof(SequenceBlock),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the IsReferenceSequence property.  This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsReferenceSequence
        {
            get { return (bool)GetValue(IsReferenceSequenceProperty); }
            set { SetValue(IsReferenceSequenceProperty, value); }
        }

        #endregion

        #region FocusedSymbol

        /// <summary>
        /// FocusedSymbol Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey FocusedSymbolPropertyKey = DependencyProperty.RegisterReadOnly("FocusedSymbol", typeof(IBioSymbol), typeof(SequenceBlock), new FrameworkPropertyMetadata(null, null, OnFocusedSymbolCoerceCallback));
        public static readonly DependencyProperty FocusedSymbolProperty = FocusedSymbolPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the FocusedSymbol property.  This dependency property 
        /// </summary>
        public IBioSymbol FocusedSymbol
        {
            get { return (IBioSymbol)GetValue(FocusedSymbolProperty); }
            private set { SetValue(FocusedSymbolPropertyKey, value); }
        }

        static object OnFocusedSymbolCoerceCallback(DependencyObject d, object baseValue)
        {
            SequenceBlock sb = (SequenceBlock) d;
            return (sb.Sequence != null && sb.FocusedColumnIndex >= 0 && sb.FocusedColumnIndex < sb.Sequence.Count)
                ? sb.Sequence[sb.FocusedColumnIndex] 
                : null;
        }

        #endregion

        #region FocusedSymbolBrush

        /// <summary>
        /// FocusedSymbolBrush Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey FocusedSymbolBrushPropertyKey = DependencyProperty.RegisterReadOnly("FocusedSymbolBrush", typeof(Brush), typeof(SequenceBlock), new FrameworkPropertyMetadata(Brushes.Black, null, OnFocusedSymbolBrushCoerceCallback));
        public static readonly DependencyProperty FocusedSymbolBrushProperty = FocusedSymbolBrushPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the FocusedSymbolBrush property.  
        /// </summary>
        public Brush FocusedSymbolBrush
        {
            get { return (Brush)GetValue(FocusedSymbolBrushProperty); }
            private set { SetValue(FocusedSymbolBrushPropertyKey, value); }
        }

        static object OnFocusedSymbolBrushCoerceCallback(DependencyObject d, object baseValue)
        {
            SequenceBlock sb = (SequenceBlock) d;
            return (sb.FocusedSymbol != null && sb.SequenceColorSelector != null)
                       ? sb.SequenceColorSelector.GetSymbolBrush(sb.FocusedSymbol)
                       : baseValue;
        }

        #endregion

        #region IsActive

        /// <summary>
        /// IsActive Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(SequenceBlock), new FrameworkPropertyMetadata(false, OnFocusIndicatorChanged));

        /// <summary>
        /// Gets or sets the IsActive property.  This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        #endregion

        #region FocusBackgroundBrush

        /// <summary>
        /// FocusBackgroundBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty FocusBackgroundBrushProperty = DependencyProperty.Register("FocusBackgroundBrush", typeof(Brush), typeof(SequenceBlock), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the FocusBackgroundBrush property.  This dependency property 
        /// indicates ....
        /// </summary>
        public Brush FocusBackgroundBrush
        {
            get { return (Brush)GetValue(FocusBackgroundBrushProperty); }
            set { SetValue(FocusBackgroundBrushProperty, value); }
        }

        #endregion

        #region FocusBorderBrush

        /// <summary>
        /// FocusBorderBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty FocusBorderBrushProperty = DependencyProperty.Register("FocusBorderBrush", typeof(Brush), typeof(SequenceBlock), new FrameworkPropertyMetadata(Brushes.Black));

        /// <summary>
        /// Gets or sets the FocusBorderBrush property.  This dependency property 
        /// indicates ....
        /// </summary>
        public Brush FocusBorderBrush
        {
            get { return (Brush)GetValue(FocusBorderBrushProperty); }
            set { SetValue(FocusBorderBrushProperty, value); }
        }

        #endregion

        /// <summary>
        /// Static constructor
        /// </summary>
        static SequenceBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SequenceBlock), new FrameworkPropertyMetadata(typeof(SequenceBlock)));
        }

        public override void OnApplyTemplate()
        {
           base.OnApplyTemplate();

            _text = Template.FindName("PART_TextHost", this) as SequenceTextDrawingVisualHost;
            if (_text == null)
                throw new ArgumentException("Missing PART_TextHost");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var fm = new FormattedText("W", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                                       new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize,
                                       Brushes.Black);
            CellWidth = fm.Width;
            CellHeight = fm.Height;

            if (!Double.IsInfinity(availableSize.Width))
            {
                Count = (int) Math.Floor(availableSize.Width/CellWidth);
            }

            return base.MeasureOverride(availableSize);
        }

        private int GetCellFromPoint(Point pt)
        {
            double xPos = pt.X;
            if (xPos < 0)
                xPos = 0;
            if (xPos > ActualWidth)
                xPos = ActualWidth - 1;

            return (int)(xPos / CellWidth);            
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_text);
            FocusedColumnIndex = Column + GetCellFromPoint(pt);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_text != null)
                _text.DrawVisualSequence(this);
            base.OnRender(drawingContext);
        }
    }
}