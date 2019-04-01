using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Bio.Data;
using Bio.Data.Interfaces;
using Bio.Views.Alignment.Controls;
using Bio.Views.Alignment.Options;
using Bio.Views.Alignment.ViewModels;

namespace Bio.Views.Alignment.Internal
{
    /// <summary>
    /// This class is used to color nucleotides.
    /// </summary>
    public class NucleotideColorSelector : SequenceColorSelector
    {
        private readonly AlignmentViewModel _mainVm;
        private readonly RuntimeOptionsViewModel _options;

        public NucleotideColorSelector(AlignmentViewModel mainVm, RuntimeOptionsViewModel options)
        {
            _options = options;
            _mainVm = mainVm;
        }

        public override TextAttributes GetSequenceAttributes(IList<IBioSymbol> symbols, int start, out bool canMergeDuplicates)
        {
            TextAttributes defaultAttributes = new TextAttributes();
            IBioSymbol symbol = symbols[start];
            canMergeDuplicates = true;

            if (_mainVm.SelectedReferenceSequences.Count > 0 && 
                symbol.Type != BioSymbolType.None && symbol.Type != BioSymbolType.Missing)
            {
                if (_mainVm.SelectedReferenceSequences.Where(rs => rs.AlignedData == symbols).FirstOrDefault() == null)
                {
                    canMergeDuplicates = false;
                    var rs = _mainVm.SelectedReferenceSequences.FirstOrDefault(seq => seq.AlignedData[start].Value == symbol.Value);
                    if (rs != null)
                        defaultAttributes.Background = rs.ReferenceSequenceColor;
                }
            }

            _options.GetAttributes(symbol, ref defaultAttributes);
            return defaultAttributes;
        }

        public override Brush GetSymbolBrush(IBioSymbol symbol)
        {
            TextAttributes ta = new TextAttributes();
            _options.GetAttributes(symbol, ref ta);
            return ta.Foreground;
        }

        public override Color GetSymbolColor(IBioSymbol symbol)
        {
            TextAttributes ta = new TextAttributes();
            _options.GetAttributes(symbol, ref ta);
            if (ta.Foreground is SolidColorBrush)
            {
                SolidColorBrush scb = (SolidColorBrush) ta.Foreground;
                return scb.Color;
            }
            return Colors.Black;
        }
    }
}