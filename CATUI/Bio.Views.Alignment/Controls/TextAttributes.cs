using System.Windows.Media;
using System.Windows;

namespace Bio.Views.Alignment.Controls
{
    public class TextAttributes
    {
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }

        public TextAttributes()
        {
            Background = Brushes.Transparent;
            Foreground = Brushes.Black;
            FontStyle = FontStyles.Normal;
            FontWeight = FontWeights.Normal;  
        }
    }
}