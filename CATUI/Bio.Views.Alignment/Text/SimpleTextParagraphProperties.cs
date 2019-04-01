using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Bio.Views.Alignment.Text
{
    class SimpleTextParagraphProperties : TextParagraphProperties
    {
        private readonly TextRunProperties _props;

        public SimpleTextParagraphProperties(FontFamily fontName, double fontSize)
        {
            _props = new SimpleTextRunProperties(fontName, fontSize);
        }

        public override FlowDirection FlowDirection
        {
            get { return FlowDirection.LeftToRight; }
        }

        public override TextAlignment TextAlignment
        {
            get { return TextAlignment.Left; }
        }

        public override double LineHeight
        {
            get { return 0.0; }
        }

        public override bool FirstLineInParagraph
        {
            get { return false; }
        }

        public override TextRunProperties DefaultTextRunProperties
        {
            get { return _props; }
        }

        public override TextWrapping TextWrapping
        {
            get { return TextWrapping.NoWrap; }
        }

        public override TextMarkerProperties TextMarkerProperties
        {
            //get { return new TextSimpleMarkerProperties(TextMarkerStyle.Square, 5, 1, new GenericTextParagraphProperties()); }
            get { return null; }
        }

        public override double Indent
        {
            get { return 0.0; }
        }
    }
}