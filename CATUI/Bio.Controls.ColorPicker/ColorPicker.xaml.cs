using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;

namespace Bio.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker
    {
        public static readonly DependencyProperty CustomColorsProperty =
            DependencyProperty.Register("CustomColors", typeof(ColorCollection), typeof(ColorPicker), new UIPropertyMetadata(new ColorCollection(), OnCustomColorCollectionChanged));

        public ColorCollection CustomColors
        {
            get { return (ColorCollection)GetValue(CustomColorsProperty); }
            set { SetValue(CustomColorsProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker),
                new UIPropertyMetadata(Colors.Black, OnColorChanged));
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorBrushProperty = DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush),
                        typeof(ColorPicker), new UIPropertyMetadata(Brushes.Black));
        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            private set { SetValue(ColorBrushProperty, value); }
        }

        public ColorPicker()
        {
            InitializeComponent();

            knownColorsLb.ItemsSource = typeof (Brushes).GetProperties().Select(color => color.Name);
            customColorsLb.ItemsSource = CustomColors;
            css.ColorChanged += (s, e) => { Color = css.Color; };
        }

        private static void OnColorChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker cp = (ColorPicker) dpo;
            Color newColor = (Color) e.NewValue;
            cp.ColorBrush = new SolidColorBrush(newColor);
            if (cp.css != null && cp.css.Color != newColor)
                cp.css.Color = newColor;
        }

        private static void OnCustomColorCollectionChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker) dpo).customColorsLb.ItemsSource = (ColorCollection) e.NewValue;
        }

        private void OnColorListChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox) sender;
            string color = (lb.SelectedItem != null) ? lb.SelectedItem.ToString() : null;
            if (!string.IsNullOrEmpty(color))
            {
                css.Color = (Color) new ColorConverter().ConvertFrom(color);
            }
        }

        private void OnAddToCustomColors(object sender, RoutedEventArgs e)
        {
            CustomColors.Add(css.Color);            
        }
    }
}
