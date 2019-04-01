using System.Windows.Media;
using System.Globalization;
using System.Windows;

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

namespace Bio.Views.Structure.ViewModels
{
    public class SSTextLabelViewModel : SSElementBaseViewModel
    {
        private string _text;
        public string Text 
        {
            get { return _text; }
            set { _text = value; SetWidthAndHeight(); }
        }

        private FontFamily _fontFamily;
        public FontFamily FontFamily 
        {
            get { return _fontFamily; }
            set { _fontFamily = value; SetWidthAndHeight(); }
        }

        private FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            get { return _fontStyle; }
            set { _fontStyle = value; SetWidthAndHeight(); }
        }

        private FontWeight _fontWeight;
        public FontWeight FontWeight 
        {
            get { return _fontWeight; }
            set { _fontWeight = value; SetWidthAndHeight(); }
        }

        private double _fontSize;
        public double FontSize 
        {
            get { return _fontSize; }
            set { _fontSize = value; SetWidthAndHeight(); } 
        }

        private Brush _color;
        public Brush Color 
        {
            get { return _color; }
            set { _color = value; SetWidthAndHeight(); }
        }

        public SSTextLabelViewModel(double centerLeftX, double centerLeftY, string text, FontFamily fontFamily, 
            FontStyle fontStyle, FontWeight fontWeight, double fontSize, Brush color)
        {
            _text = text;
            _fontFamily = fontFamily;
            _fontStyle = fontStyle;
            _fontWeight = fontWeight;
            _fontSize = fontSize;
            _color = color;
            SetWidthAndHeight();
            X = centerLeftX;
            Y = centerLeftY - _currentBaseline;
        }

        private double _currentBaseline;

        private void SetWidthAndHeight()
        {
            Typeface type = new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal);
            FormattedText text = new FormattedText(Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                 type, FontSize, Color);
            Width = text.WidthIncludingTrailingWhitespace;
            Height = text.Height;
            _currentBaseline = text.Baseline;
        }
    }
}
