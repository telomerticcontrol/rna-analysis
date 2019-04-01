using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using JulMar.Windows.Interfaces;
using JulMar.Windows.Mvvm;

namespace Bio.Views.Alignment.Options
{
    /// <summary>
    /// The view model used for our display options
    /// </summary>
    public class DisplayOptionsViewModel : ViewModel
    {
        /// <summary>
        /// This represents a nucelotide color
        /// </summary>
        public class NucleotideColor : SimpleViewModel
        {
            private string _symbol;
            private string _color;

            /// <summary>
            /// Symbol (A,E,I,etc.)
            /// </summary>
            public string Symbol
            {
                get { return _symbol; }
                set
                {
                    if (string.IsNullOrEmpty(value))
                        throw new ArgumentException("Value cannot be empty");

                    _symbol = value; 
                    OnPropertyChanged("Symbol");
                }
            }

            /// <summary>
            /// Color to use
            /// </summary>
            public string Color
            {
                get { return _color; }
                set
                {
                    string newValue = value;
                    if (string.IsNullOrEmpty(newValue))
                        newValue = "Black";

                    _color = newValue; 
                    OnPropertyChanged("Color");
                }
            }

            /// <summary>
            /// Default constructor for new items
            /// </summary>
            public NucleotideColor()
            {
                _symbol = "?";
                _color = "Black";
            }
        }

        /// <summary>
        /// Exposes color properties to the view
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class PropertyExposer<T>
        {
            private readonly Func<T> _getOp;
            private readonly Action<T> _setOp;

            public string Name { get; set; }    
            public T Value
            {
                get { return _getOp(); }
                set { _setOp(value); }
            }

            public PropertyExposer(string name, Func<T> getOp, Action<T> setOp)
            {
                Name = name;
                _getOp = getOp;
                _setOp = setOp;
            }
        }

        private string _resourceKey;
        private int _minGroupingRange;

        /// <summary>
        /// Option panes that may be changed
        /// </summary>
        public List<OptionPane> Menu { get; private set; }

        /// <summary>
        /// Nucleotide colors
        /// </summary>
        public ObservableCollection<NucleotideColor> NucleotideColors { get; private set; }

        /// <summary>
        /// Exposes the properties of this object dynamically
        /// </summary>
        public List<PropertyExposer<string>> ColorOptions { get; private set; }

        /// <summary>
        /// All colors we allow
        /// </summary>
        public List<string> AllColors { get; private set; }

        /// <summary>
        /// Resource key used to locate templats
        /// </summary>
        public string ResourceKey 
        {
            get { return _resourceKey; }
            set { _resourceKey = value; OnPropertyChanged("ResourceKey"); }
        }

        /// <summary>
        /// Command to reset the options
        /// </summary>
        public ICommand Reset { get; private set; }

        /// <summary>
        /// Save command
        /// </summary>
        public ICommand Save { get; private set; }

        /// <summary>
        /// Add a new reference sequence color
        /// </summary>
        public ICommand AddRefSeqColor { get; private set; }

        /// <summary>
        /// Remove a reference sequence color
        /// </summary>
        public ICommand RemoveRefSeqColor { get; private set; }

        /// <summary>
        /// Add a new nucleotide color
        /// </summary>
        public ICommand AddNucleotideColor { get; private set; }

        /// <summary>
        /// Remove a nucelotide color
        /// </summary>
        public ICommand RemoveNucleotideColor { get; private set; }

        /// <summary>
        /// List of font names
        /// </summary>
        public string[] FontNames
        {
            get { return Fonts.SystemFontFamilies.Select(ff => ff.ToString()).OrderBy(s => s).ToArray(); }
        }

        private static readonly string[] KnownFonts = {"Consolas", "Bitstream Vera Sans Mono", "Lucida Console", "Courier", "Courier New", "Global Monospace"};
        
        /// <summary>
        /// List of fixed font names
        /// </summary>
        public string[] FixedFontNames
        {
            get
            {
                var fontNames = Fonts.SystemFontFamilies.Select(ff => ff.ToString()).Where(s => KnownFonts.Contains(s)).OrderBy(s => s).ToList();
                fontNames.Add("Global Monospace");
                return fontNames.ToArray();
            }
        }
        
        /// <summary>
        /// List of font sizes
        /// </summary>
        public int[] FontSizes { get; private set; }

        /// <summary>
        /// Selected font size (everything except alignment)
        /// </summary>
        public int SelectedFontSize { get; set; }

        /// <summary>
        /// Alignment editor font size
        /// </summary>
        public int AlignmentFontSize { get; set; }

        /// <summary>
        /// Selected font
        /// </summary>
        public string SelectedFont { get; set; }

        /// <summary>
        /// Alignment editor font family
        /// </summary>
        public string AlignmentFontFamily { get; set; }

        /// <summary>
        /// Whether to show grouping level for taxonomy groups
        /// </summary>
        public bool ShowTaxonomyGroupLevel { get; set; }

        /// <summary>
        /// True/False to open with grouping
        /// </summary>
        public bool OpenWithGrouping { get; set; }

        /// <summary>
        /// Minimum grouping range
        /// </summary>
        public int MinGroupingRange
        {
            get { return _minGroupingRange; }
            set
            {
                int newValue = Math.Max(0, Math.Min(100, value));
                _minGroupingRange = newValue;
                OnPropertyChanged("MinGroupingRange");
            }
        }

        /// <summary>
        /// True/False to show row numbers
        /// </summary>
        public bool ShowRowNumbers { get; set; }

        /// <summary>
        /// True to sort non-grouped items by Taxonomy
        /// </summary>
        public bool SortByTaxonomy { get; set; }

        /// <summary>
        /// Reference sequence colors
        /// </summary>
        public ObservableCollection<string> ReferenceSequenceColors { get; private set; }

        /// <summary>
        /// Selection color for selected row
        /// </summary>
        public string SelectionColor { get; set; }

        /// <summary>
        /// Text Selection color for selected row
        /// </summary>
        public string SelectionTextColor { get; set; }

        /// <summary>
        /// Color for separator rows
        /// </summary>
        public string SeparatorColor { get; set; }

        /// <summary>
        /// Color used to fill background of alignment view
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Color used to border for selected items
        /// </summary>
        public string SelectionBorderColor { get; set; }

        /// <summary>
        /// Color used to fill the focused item
        /// </summary>
        public string FocusedColor { get; set; }

        /// <summary>
        /// Color used to fill the focused item
        /// </summary>
        public string FocusedBorderColor { get; set; }

        /// <summary>
        /// Color used to fill the focused item
        /// </summary>
        public string ForegroundTextColor { get; set; }

        /// <summary>
        /// Color used to color line numbers
        /// </summary>
        public string LineNumberTextColor { get; set; }

        /// <summary>
        /// Color used to fill the focus rectangle
        /// </summary>
        public string FocusRectColor { get; set; }

        /// <summary>
        /// Color used to stroke the focus rectangle
        /// </summary>
        public string FocusRectBorderColor { get; set; }

        /// <summary>
        /// Color used for the group headers
        /// </summary>
        public string GroupHeaderBackgroundColor { get; set; }

        /// <summary>
        /// Color used for the group header text
        /// </summary>
        public string GroupHeaderTextColor { get; set;  }

        /// <summary>
        /// Constructor
        /// </summary>
        public DisplayOptionsViewModel()
        {
            FontSizes = new [] { 6,8,10,11,12,14,16,18,20,24,32,48 };

            Reset = new DelegatingCommand(OnResetOptions);
            Save = new DelegatingCommand(OnSave);
            AddRefSeqColor = new DelegatingCommand<string>(OnAddReferenceSeqColor, s => !string.IsNullOrEmpty(s));
            RemoveRefSeqColor = new DelegatingCommand<string>(OnRemoveReferenceSeqColor, s => !string.IsNullOrEmpty(s));
            AddNucleotideColor = new DelegatingCommand(OnAddNucleotideColor);
            RemoveNucleotideColor = new DelegatingCommand<NucleotideColor>(OnRemoveNucleotideColor, nc => nc != null);

            Menu = new List<OptionPane> {
               new OptionPane(this) { 
                    Name = "Display", 
                    Description = "Display options", 
                    Children = new List<OptionPane> {
                        new OptionPane(this) {
                             Name = "Fonts and Colors",
                             Description = "Change fonts and colors",
                             ResourceKey = "FontsAndColors"
                         },
                        new OptionPane(this) {
                             Name = "Sorting/Grouping",
                             Description = "Sorting and Grouping options",
                             ResourceKey = "Grouping"
                         },
                        new OptionPane(this) {
                            Name = "Reference Colors",
                            Description = "Change the reference sequence highlight colors",
                            ResourceKey = "ReferenceSeqColors"
                         },
                        new OptionPane(this) {
                            Name = "Nucleotide Colors",
                            Description = "Change the colors of the rendered nucleotide symbols",
                            ResourceKey = "NucleotideColors"
                         },
                    },
                },
           };

            Menu[0].Children[0].IsSelected = true;

            AllColors = typeof (Colors).GetProperties().Select(pi => pi.Name).ToList();
            ColorOptions = new List<PropertyExposer<string>>
               {
                   new PropertyExposer<string>("SelectionColor", () => SelectionColor, c => SelectionColor = c),
                   new PropertyExposer<string>("SelectionBorderColor", () => SelectionBorderColor, c => SelectionBorderColor = c),
                   new PropertyExposer<string>("SelectionTextColor", () => SelectionTextColor, c => SelectionTextColor = c),
                   new PropertyExposer<string>("SeparatorColor", () => SeparatorColor, c => SeparatorColor = c),
                   new PropertyExposer<string>("BackgroundColor", () => BackgroundColor, c => BackgroundColor = c),
                   new PropertyExposer<string>("FocusedColor", () => FocusedColor, c => FocusedColor = c),
                   new PropertyExposer<string>("FocusedBorderColor", () => FocusedBorderColor, c => FocusedBorderColor = c),
                   new PropertyExposer<string>("ForegroundTextColor", () => ForegroundTextColor, c => ForegroundTextColor = c),
                   new PropertyExposer<string>("LineNumberTextColor", () => LineNumberTextColor, c => LineNumberTextColor = c),
                   new PropertyExposer<string>("FocusRectColor", () => FocusRectColor, c => FocusRectColor = c),
                   new PropertyExposer<string>("FocusRectBorderColor", () => FocusRectBorderColor, c => FocusRectBorderColor = c),
                   new PropertyExposer<string>("GroupHeaderBackgroundColor", () => GroupHeaderBackgroundColor, c => GroupHeaderBackgroundColor = c),
                   new PropertyExposer<string>("GroupHeaderTextColor", () => GroupHeaderTextColor, c => GroupHeaderTextColor = c)
               };

            LoadOptions();
        }

        private void OnAddReferenceSeqColor(string color)
        {
            ReferenceSequenceColors.Add(color);
        }

        private void OnRemoveReferenceSeqColor(string color)
        {
            ReferenceSequenceColors.Remove(color);
        }

        private void OnAddNucleotideColor()
        {
            NucleotideColors.Add(new NucleotideColor());
        }

        private void OnRemoveNucleotideColor(NucleotideColor color)
        {
            NucleotideColors.Remove(color);
        }

        private void LoadOptions()
        {
            SelectedFontSize = Properties.Settings.Default.SelectedFontSize;
            SelectedFont = Properties.Settings.Default.SelectedFont;
            AlignmentFontSize = Properties.Settings.Default.AlignmentFontSize;
            AlignmentFontFamily = Properties.Settings.Default.AlignmentFontFamily;
            MinGroupingRange = Properties.Settings.Default.MinGroupingRange;
            ShowRowNumbers = Properties.Settings.Default.ShowRowNumbers;
            SortByTaxonomy = Properties.Settings.Default.SortByTaxonomy;
            ShowTaxonomyGroupLevel = Properties.Settings.Default.ShowTaxonomyGroupLevel;
            ReferenceSequenceColors = new ObservableCollection<string>(Properties.Settings.Default.ReferenceSequenceColors.Cast<string>());
            SelectionColor = Properties.Settings.Default.SelectionColor;
            SelectionTextColor = Properties.Settings.Default.SelectionTextColor;
            SeparatorColor = Properties.Settings.Default.SeparatorColor;
            BackgroundColor = Properties.Settings.Default.BackgroundColor;
            OpenWithGrouping = Properties.Settings.Default.OpenWithGrouping;
            SelectionBorderColor = Properties.Settings.Default.SelectionBorderColor;
            FocusedColor = Properties.Settings.Default.FocusedColor;
            FocusedBorderColor = Properties.Settings.Default.FocusedBorderColor;
            ForegroundTextColor = Properties.Settings.Default.ForegroundTextColor;
            LineNumberTextColor = Properties.Settings.Default.LineNumberTextColor;
            FocusRectColor = Properties.Settings.Default.FocusRectColor;
            FocusRectBorderColor = Properties.Settings.Default.FocusedBorderColor;
            GroupHeaderBackgroundColor = Properties.Settings.Default.GroupHeaderBackgroundColor;
            GroupHeaderTextColor = Properties.Settings.Default.GroupHeaderTextColor;
            NucleotideColors = new ObservableCollection<NucleotideColor>(Properties.Settings.Default.NucleotideColors.Cast<string>().Select(nc => new NucleotideColor {Symbol = nc[0].ToString(), Color = nc.Substring(1) }));
        }

        /// <summary>
        /// Resets all options to defaults.
        /// </summary>
        private void OnResetOptions()
        {
            var msgVisualizer = Resolve<IMessageVisualizer>();
            if (msgVisualizer != null)
            {
                if (msgVisualizer.Show("Reset All Settings", 
                        "This will reset all colors, fonts and settings to the default values and cannot be undone.\r\n"+
                        "Are you sure you want to reset all settings?", MessageButtons.YesNo) == MessageResult.No)
                {
                    return;
                }
            }
            Properties.Settings.Default.Reset();
            LoadOptions();
            OnPropertyChanged();
        }

        /// <summary>
        /// Saves the options to persistent storage.
        /// </summary>
        public void OnSave()
        {
            Properties.Settings.Default.SelectedFontSize = SelectedFontSize;
            Properties.Settings.Default.SelectedFont = SelectedFont;
            Properties.Settings.Default.AlignmentFontSize = AlignmentFontSize;
            Properties.Settings.Default.AlignmentFontFamily = AlignmentFontFamily;

            Properties.Settings.Default.MinGroupingRange = MinGroupingRange;
            Properties.Settings.Default.ShowRowNumbers = ShowRowNumbers;
            Properties.Settings.Default.ShowTaxonomyGroupLevel = ShowTaxonomyGroupLevel;
            Properties.Settings.Default.SortByTaxonomy = SortByTaxonomy;
            Properties.Settings.Default.OpenWithGrouping = OpenWithGrouping;

            Properties.Settings.Default.ReferenceSequenceColors.Clear();
            Properties.Settings.Default.ReferenceSequenceColors.AddRange(ReferenceSequenceColors.ToArray());

            Properties.Settings.Default.SelectionColor = SelectionColor;
            Properties.Settings.Default.SelectionTextColor = SelectionTextColor;
            Properties.Settings.Default.SeparatorColor = SeparatorColor;
            Properties.Settings.Default.BackgroundColor = BackgroundColor;
            Properties.Settings.Default.SelectionBorderColor = SelectionBorderColor;
            Properties.Settings.Default.FocusedColor = FocusedColor;
            Properties.Settings.Default.FocusedBorderColor = FocusedBorderColor;
            Properties.Settings.Default.ForegroundTextColor = ForegroundTextColor;
            Properties.Settings.Default.LineNumberTextColor = LineNumberTextColor;
            Properties.Settings.Default.FocusRectColor = FocusRectColor;
            Properties.Settings.Default.FocusedBorderColor = FocusRectBorderColor;
            Properties.Settings.Default.GroupHeaderBackgroundColor = GroupHeaderBackgroundColor;
            Properties.Settings.Default.GroupHeaderTextColor = GroupHeaderTextColor;

            Properties.Settings.Default.NucleotideColors.Clear();
            Properties.Settings.Default.NucleotideColors.AddRange(NucleotideColors.Select(nc => string.Format("{0}{1}", nc.Symbol, nc.Color)).ToArray());

            Properties.Settings.Default.Save();
        }
    }
}