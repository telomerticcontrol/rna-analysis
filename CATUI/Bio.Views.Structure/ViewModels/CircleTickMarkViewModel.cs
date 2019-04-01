using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Bio.Views.Structure.ViewModels
{
    public class CircleTickMarkViewModel : CircleElementBaseViewModel
    {
        private double _size;
        public double Size
        {
            get { return _size; }
            private set { _size = value; }
        }

        public CircleTickMarkViewModel(double xpos, double ypos, 
            int index, double size)
        {
            X = xpos;
            Y = ypos;
            Size = size;
        }
    }
}
