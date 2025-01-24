using FeedFetcher.ViewModel;
using System.Windows;

namespace FeedFetcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = MainViewModel.Instance;
        }
    }
}