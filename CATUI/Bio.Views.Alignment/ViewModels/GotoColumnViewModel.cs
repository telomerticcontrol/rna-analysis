using System;
using System.Collections.Generic;
using JulMar.Windows.Mvvm;

namespace Bio.Views.Alignment.ViewModels
{
    /// <summary>
    /// This view model is used to drive the "jump to" dialog
    /// </summary>
    public class GotoColumnRowViewModel : SimpleViewModel
    {
        private int _position;
        private AlignmentEntityViewModel _selectedReferenceSequence;

        /// <summary>
        /// Reference sequences
        /// </summary>
        public IList<AlignmentEntityViewModel> ReferenceSequences { get; set; }

        /// <summary>
        /// Selected sequence
        /// </summary>
        public AlignmentEntityViewModel SelectedReferenceSequence
        {
            get { return _selectedReferenceSequence; }
            set { _selectedReferenceSequence = value; OnPropertyChanged("SelectedReferenceSequence"); }
        }

        /// <summary>
        /// Row or column
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Minimum allowed position
        /// </summary>
        public int MinPosition { get; set; }

        /// <summary>
        /// Maximum allowed position
        /// </summary>
        public int MaxPosition { get; set; }

        /// <summary>
        /// The position to jump to (return)
        /// </summary>
        public int Position
        {
            get { return _position; }
            set
            {
                int newValue = value;

                if (newValue < MinPosition)
                    newValue = MinPosition;
                if (newValue > MaxPosition)
                    newValue = MaxPosition;
                
                _position = newValue;
                OnPropertyChanged("Position");
            }
        }

        public GotoColumnRowViewModel()
        {
            MinPosition = 0;
            MaxPosition = Int32.MaxValue;
            Position = 0;
        }
    }
}
