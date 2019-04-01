using System.Diagnostics;
using JulMar.Windows.Mvvm;

namespace Bio.Views.Alignment.ViewModels
{
    public class SplitPaneViewModel : ViewModel
    {
        private int _visibleColumns, _firstColumn;
        private readonly AlignmentViewModel _parent;

        /// <summary>
        /// Primary (main) view model
        /// </summary>
        public AlignmentViewModel Parent
        {
            get { return _parent;  }
        }

        /// <summary>
        /// This gets/sets the number of visible columns based on the window size.
        /// </summary>
        public int VisibleColumns
        {
            get { return _visibleColumns; }
            set
            {
                if (_visibleColumns != value)
                {
                    Debug.Assert(value >= 0);
                    _visibleColumns = value;
                    OnPropertyChanged("VisibleColumns", "NotVisibleColumns");
                    _parent.SplitViewDimensionsChanged(this);
                }
            }
        }

        /// <summary>
        /// Returns the total number of columns we can scroll
        /// </summary>
        public int NotVisibleColumns
        {
            get { return _parent.TotalColumns > VisibleColumns ? _parent.TotalColumns - VisibleColumns : 0; }
        }

        /// <summary>
        /// Current (focused) column
        /// </summary>
        public int FocusedColumnIndex
        {
            get { return _parent.FocusedColumnIndex;  }
            set 
            {
                _parent.SetActiveView(this); 
                _parent.FocusedColumnIndex = value;
            }
        }

        /// <summary>
        /// Called by the parent view model when the focused column index shifts.
        /// </summary>
        internal void OnFocusedColumnIndexChanged()
        {
            OnPropertyChanged("FocusedColumnIndex"); 
        }

        /// <summary>
        /// The current (visible) left column
        /// </summary>
        public int FirstColumn
        {
            get { return _firstColumn; }
            set
            {
                if (_firstColumn == value)
                    return;
                _firstColumn = value;
                if (_firstColumn < 0)
                    _firstColumn = 0;
                else if (_firstColumn > (_parent.TotalColumns - VisibleColumns))
                    _firstColumn = (_parent.TotalColumns - VisibleColumns);
                OnPropertyChanged("FirstColumn");
                _parent.SplitViewDimensionsChanged(this);
            }
        }

        public SplitPaneViewModel(AlignmentViewModel parent)
        {
            _parent = parent;
        }

    }
}
