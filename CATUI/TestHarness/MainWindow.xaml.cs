using System.Windows;
using System.Windows.Input;
using Bio.Data.Providers.FastA;
using Bio.Views.Alignment.ViewModels;

namespace TestHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            FastAFile fastAFile = new FastAFile();
            fastAFile.Initialize(@"C:\Users\Mark\Documents\Work\University of Texas\data\Alignment\16S.A.alnfasta");
            fastAFile.Load();

            var bev = new BirdsEyeViewModel(fastAFile.Entities, null) { VisibleColumns = 100, VisibleRows = 100, FirstColumn = 0, TopRow = 0 };
            DataContext = bev;

            InitializeComponent();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            BirdsEyeViewModel bev = (BirdsEyeViewModel) DataContext;
            bev.Dispose();

            base.OnClosed(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }
    }
}
