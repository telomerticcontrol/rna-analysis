using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Bio.Data;
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Controls;

namespace Bio.Views.Alignment.Text
{
    /// <summary>
    /// This class wraps the biological sequence and presents it as a line of text.
    /// </summary>
    internal class SequenceTextStore : TextSource
    {
        #region Data
        private readonly SequenceColorSelector _selector;
        private readonly IList<IBioSymbol> _data;
        private readonly double _fontSize;
        private readonly FontFamily _fontFamily;
        #endregion

        public int LastRenderColumn { get; set; }

        /// <summary>
        /// Constructor that builds the text store
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="sequence"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        public SequenceTextStore(SequenceColorSelector selector, IList<IBioSymbol> sequence, FontFamily fontFamily, double fontSize)
        {
            _selector = selector;
            _data = sequence;
            _fontFamily = fontFamily;
            _fontSize = fontSize;
        }

        /// <summary>
        /// Retrieves a <see cref="T:System.Windows.Media.TextFormatting.TextRun"/> starting at a specified <see cref="T:System.Windows.Media.TextFormatting.TextSource"/> position.
        /// </summary>
        /// <returns>
        /// A value that represents a <see cref="T:System.Windows.Media.TextFormatting.TextRun"/>, or an object derived from <see cref="T:System.Windows.Media.TextFormatting.TextRun"/>.
        /// </returns>
        /// <param name="textSourceCharacterIndex">Specifies the character index position in the <see cref="T:System.Windows.Media.TextFormatting.TextSource"/> where the <see cref="T:System.Windows.Media.TextFormatting.TextRun"/> is retrieved.</param>
        public override TextRun GetTextRun(int textSourceCharacterIndex)
        {
            // Return an end-of-paragraph if no more text source.
            if (textSourceCharacterIndex >= LastRenderColumn || _data.Count <= textSourceCharacterIndex)
                return new TextEndOfParagraph(1);

            // If it's a nucleotide then it can be shaded differently on a per-symbol basis
            // so just render one character; for gap/missing sets render them together.
            var symbol = _data[textSourceCharacterIndex];
            bool canMergeDuplicates;
            var textAttributes = _selector.GetSequenceAttributes(_data, textSourceCharacterIndex, out canMergeDuplicates);

            // Gather the symbols we will be rendering as a group
            var charData = new List<char> {symbol.Value};
            if (symbol.Type != BioSymbolType.Nucleotide || canMergeDuplicates)
            {
                int firstDiff, renderCount = Math.Min(LastRenderColumn, _data.Count);
                for (firstDiff = textSourceCharacterIndex + 1; firstDiff < renderCount; firstDiff++)
                {
                    var checkSymbol = _data[firstDiff];
                    if (checkSymbol.Value != symbol.Value)
                        break;
                    charData.Add(checkSymbol.Value);
                }
            }

            int count = charData.Count;
            Debug.Assert(LastRenderColumn - textSourceCharacterIndex >= count);

            // Return the text characters
            return new TextCharacters(charData.ToArray(), 0, count, 
                new SimpleTextRunProperties(_fontFamily, _fontSize, textAttributes));
        }

        /// <summary>
        /// Retrieves the text span immediately before the specified <see cref="T:System.Windows.Media.TextFormatting.TextSource"/> position.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Media.TextFormatting.CultureSpecificCharacterBufferRange"/> value that represents the text span immediately before <paramref name="textSourceCharacterIndexLimit"/>.
        /// </returns>
        /// <param name="textSourceCharacterIndexLimit">An <see cref="T:System.Int32"/> value that specifies the character index position where text retrieval stops.</param>
        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
        {
            return new TextSpan<CultureSpecificCharacterBufferRange>(
                textSourceCharacterIndexLimit,
                new CultureSpecificCharacterBufferRange(System.Globalization.CultureInfo.CurrentUICulture,
                                                        new CharacterBufferRange(new char[0], 0, textSourceCharacterIndexLimit)));
        }

        /// <summary>
        /// Retrieves a value that maps a <see cref="T:System.Windows.Media.TextFormatting.TextSource"/> character index to a <see cref="T:System.Windows.Media.TextEffect"/> character index.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Int32"/> value that represents the <see cref="T:System.Windows.Media.TextEffect"/> character index.
        /// </returns>
        /// <param name="textSourceCharacterIndex">An <see cref="T:System.Int32"/> value that specifies the <see cref="T:System.Windows.Media.TextFormatting.TextSource"/> character index to map.</param>
        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
        {
            return textSourceCharacterIndex;
        }
    }
}