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

using Bio.Data.Providers;
using Bio.Views.ViewModels;
using Bio.Views.Interfaces;

namespace Bio.Views
{
    /// <summary>
    /// This represents a located BioView
    /// </summary>
    public class BioViewProviderFactory<T> : IBioViewProvider where T : BioViewModel, new()
    {
        readonly BioViewAttribute _attribute;

        /// <summary>
        /// The description of this visual
        /// </summary>
        public string Description { get { return _attribute.Description; } }

        /// <summary>
        /// The formats that can be rendered by this visual
        /// </summary>
        public BioFormatType SupportedFormats { get { return _attribute.SupportedFormats; } }

        /// <summary>
        /// The PackUri to the icon to display for this visual
        /// </summary>
        public string ImageUrl { get { return _attribute.ImageUrl; } }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        public BioViewProviderFactory()
        {
            var atts = GetType().GetCustomAttributes(typeof (BioViewAttribute), true);
            if (atts != null && atts.Length == 1)
                _attribute = (BioViewAttribute) atts[0];
        }

        /// <summary>
        /// Method to create the visual
        /// </summary>
        /// <returns></returns>
        public BioViewModel Create()
        {
            return new T { ImageUrl = ImageUrl };
        }
    }
}