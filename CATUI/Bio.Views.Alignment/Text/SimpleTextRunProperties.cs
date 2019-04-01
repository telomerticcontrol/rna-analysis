using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Bio.Views.Alignment.Controls;

namespace Bio.Views.Alignment.Text
{
    class SimpleTextRunProperties : TextRunProperties
    {
        private readonly TextDecorationCollection _textDecorations = new TextDecorationCollection();
        private readonly TextEffectCollection _textEffects = new TextEffectCollection();
        private readonly FontFamily _fontFamily;
        private readonly double _fontSize;
        private readonly TextAttributes _textAttributes;
        private Typeface _typefaceInfo;

        public SimpleTextRunProperties(FontFamily fontName, double fontSize) : this(fontName,fontSize,new TextAttributes())
        {
        }

        public SimpleTextRunProperties(FontFamily fontName, double fontSize, TextAttributes textAttributes)
        {
            _fontFamily = fontName;
            _fontSize = fontSize;
            _textAttributes = textAttributes;
        }

        public override Brush BackgroundBrush
        {
            get { return _textAttributes.Background; }
        }

        public override System.Globalization.CultureInfo CultureInfo
        {
            get { return System.Globalization.CultureInfo.CurrentUICulture; }
        }

        public override double FontHintingEmSize
        {
            get { return _fontSize; }
        }

        public override double FontRenderingEmSize
        {
            get { return _fontSize; }
        }

        public override Brush ForegroundBrush
        {
            get { return _textAttributes.Foreground; }
        }

        public override TextDecorationCollection TextDecorations
        {
            get { return _textDecorations; }
        }

        public override TextEffectCollection TextEffects
        {
            get { return _textEffects;  }
        }

        public override Typeface Typeface
        {
            get {
                return _typefaceInfo ??
                       (_typefaceInfo =
                        new Typeface(_fontFamily, _textAttributes.FontStyle, _textAttributes.FontWeight,
                                     FontStretches.Normal));
            }
        }
    }
}