using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Controls;
using Bio.Views.Alignment.Text;

namespace Bio.Views.Alignment.Internal
{
    /// <summary>
    /// This visual draws the string of text onto the display.
    /// </summary>
    public class SequenceTextDrawingVisual : DrawingVisual
    {
        private readonly SequenceTextStore _textStore;
        private readonly SimpleTextParagraphProperties _paragraphProps;

        public SequenceTextDrawingVisual(SequenceColorSelector scs, IList<IBioSymbol> sequence, FontFamily fontFamily, double fontSize)
        {
            _textStore = new SequenceTextStore(scs, sequence, fontFamily, fontSize);
            _paragraphProps = new SimpleTextParagraphProperties(fontFamily, fontSize);
        }

        public double FontSize
        {
            get { return _paragraphProps.DefaultTextRunProperties.FontRenderingEmSize;  }
        }

        public FontFamily FontFamily
        {
            get { return _paragraphProps.DefaultTextRunProperties.Typeface.FontFamily;  }
        }

        public void Render(int startPos, int count)
        {
            DrawingContext dc = RenderOpen();
            if (_textStore != null && count > 0)
            {
                _textStore.LastRenderColumn = startPos+count;

                // Create a TextFormatter object.
                TextFormatter formatter = TextFormatter.Create();

                // Format each line of text from the text store and draw it.
                // Create a textline from the text store using the TextFormatter object.
                using (var myTextLine = formatter.FormatLine(_textStore, startPos, 0, _paragraphProps, null))
                {
                    // Draw the formatted text into the drawing context.
                    myTextLine.Draw(dc, new Point(), InvertAxes.None);
                }
            }
            dc.Close();
        }
    }
}