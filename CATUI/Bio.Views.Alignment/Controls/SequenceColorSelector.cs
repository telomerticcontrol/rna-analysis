using System.Collections.Generic;
using System.Windows.Media;
using Bio.Data.Interfaces;

namespace Bio.Views.Alignment.Controls
{
    public abstract class SequenceColorSelector
    {
        public abstract TextAttributes GetSequenceAttributes(IList<IBioSymbol> symbols, int start, out bool canMergeDuplicates);
        public abstract Color GetSymbolColor(IBioSymbol symbol);
        public abstract Brush GetSymbolBrush(IBioSymbol symbol);
    }
}
