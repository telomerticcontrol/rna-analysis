using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Data.Interfaces;
using Bio.Data;

namespace Bio.Views.Structure.Models
{
    public class PhyloDInteraction
    {
        public int PredictorIndex { get; set; }
        public BioSymbol PredictorNucleotide { get; set; }
        public int TargetIndex { get; set; }
        public BioSymbol TargetNucleotide { get; set; }
        public double PValue { get; set; }
        public double QValue { get; set; }
    }

}
