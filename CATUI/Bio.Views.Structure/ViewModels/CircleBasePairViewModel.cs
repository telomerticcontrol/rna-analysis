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

using Bio.Data.Interfaces;

namespace Bio.Views.Structure.ViewModels
{
    public class CircleBasePairViewModel : CircleElementBaseViewModel
    {
        private const double MAX_RADIUS_FACTOR = 0.90;
        private readonly IBioSymbol _fivePrimeNt;
        private readonly IBioSymbol _threePrimeNt;
        private readonly IBasePairEntity _basePairEntity;

        public int FivePrimeIndex { get { return _basePairEntity.FivePrimeIndex + 1; } }
        public int ThreePrimeIndex { get { return _basePairEntity.ThreePrimeIndex + 1; } }
        public string BasePair { get { return string.Format("{0}-{1}", _fivePrimeNt, _threePrimeNt); } }
        public string Connector { get; private set; }

        public CircleBasePairViewModel(CircleSequenceViewModel sequence, IBasePairEntity basePair)
        {
            _basePairEntity = basePair;
            _fivePrimeNt = sequence.Sequence.RawData[_basePairEntity.FivePrimeIndex];
            _threePrimeNt = sequence.Sequence.RawData[_basePairEntity.ThreePrimeIndex];
            Connector = sequence.ComputeBinaryConnector(FivePrimeIndex, ThreePrimeIndex, MAX_RADIUS_FACTOR);
        }

        public override string ToString()
        {
            return string.Format("BasePair: ({0},{1}) {2}", FivePrimeIndex, ThreePrimeIndex, BasePair);
        }
    }
}
