/* 
* Copyright (c) 2009, The University of Texas at Austin
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without modification, 
* are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, 
* this list of conditions and the following disclaimer.
*
* 2. Redistributions in binary form must reproduce the above copyright notice, 
* this list of conditions and the following disclaimer in the documentation and/or other materials 
* provided with the distribution.
*
* Neither the name of The University of Texas at Austin nor the names of its contributors may be 
* used to endorse or promote products derived from this software without specific prior written 
* permission.
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
* BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF 
* THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Windows.Media;
using System.Globalization;
using System.Windows;
using Bio.Data.Interfaces;

namespace Bio.Views.Structure.ViewModels
{
    public class SSSymbolViewModel : SSElementBaseViewModel
    {
        public IBioSymbol Symbol { get; private set; }
        public int Index { get; private set; }
        public Brush Color { get; set; }
        public Typeface Typeface { get; set; }
        public double FontSize { get; set; }
        public bool Visible { get; set; }

        public double CenterX
        {
            get { return X + (0.5 * Width); }
        }

        public double CenterY
        {
            get { return Y + (0.5 * Height); }
        }

        public SSSymbolViewModel(IBioSymbol symbol, int index, 
            FontFamily fontFace, FontStyle fontStyle, FontWeight fontWeight, Brush color, double fontSize, double centerX, double centerY)
        {
            Symbol = symbol;
            Index = index;
            Color = color;
            FontSize = fontSize;
            Typeface = new Typeface(fontFace, fontStyle, fontWeight, FontStretches.Normal);
            var renderedElement = new FormattedText(symbol.ToString().Substring(0, 1),
                CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, Typeface, FontSize, Brushes.Black);
            Width = renderedElement.WidthIncludingTrailingWhitespace;
            Height = renderedElement.Height;
            X = centerX - (0.5 * Width);
            Y = centerY - (0.5 * renderedElement.Baseline);
        }

        public override string ToString()
        {
            return string.Format("Nt: {0}, Pos:({1},{2})", Index, X, Y);
        }
    }
}
