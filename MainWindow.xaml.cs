using FeedFetcher.Utilities;
using FeedFetcher.ViewModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FeedFetcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow wd { get; set; }
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = MainViewModel.Instance;
            wd = this;
            if (!CV(string.Empty))
            {
                HomeGrid.Visibility = Visibility.Collapsed;
                this.Background = new SolidColorBrush(Colors.Transparent);
                LicenseBorder.Visibility = Visibility.Visible;
                LicenseBorder.Background = new SolidColorBrush(Color.FromRgb(91, 140, 144));
                Status.Visibility = Visibility.Visible;
                Status.Text = "Failed To Validate...";
                return;
            }
            else
            {
                Status.Visibility = Visibility.Collapsed;
                Status.Text = string.Empty;
                LicenseBorder.Visibility = Visibility.Collapsed;
                HomeGrid.Visibility = Visibility.Visible;
                this.Background = new SolidColorBrush(Color.FromRgb(91,140,144));
                CHK();
            }
        }
        private void WhileClosing(object sender, CancelEventArgs e)
        {
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch { }
        }
        private void CHK()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    if (!CV(string.Empty))
                    {
                        tokenSource.Cancel();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            HomeGrid.Visibility = Visibility.Collapsed;
                            LicenseBorder.Visibility = Visibility.Visible;
                            Status.Visibility = Visibility.Visible;
                            Status.Text = "Failed To Validate...";
                            Restart();
                        });
                    }
                    await Task.Delay(TimeSpan.FromMinutes(5), tokenSource.Token);
                }
            });
        }

        public void Restart()
        {
            ProcessStartInfo proc = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true
            };

            try
            {
                Process.Start(proc);
                Environment.Exit(0); // Close the current non-admin process
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start with admin privileges: " + ex.Message);
            }
        }
        public bool CV(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key) && !File.Exists(IGConstants.LicenseFile))
                    return false;
                if (File.Exists(IGConstants.LicenseFile) && string.IsNullOrEmpty(key))
                {
                    var text = File.ReadAllBytes(IGConstants.LicenseFile);
                    if (text.Length == 0)
                        return false;
                    return SU.vld(text);
                }
                else
                {
                    return SU.vld(Encoding.UTF8.GetBytes(key), true);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void lvd(string vd)
        {
            try
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LValid.Text = string.Format(Properties.Resources.LVD ?? "License Valid Upto => {0}", vd);
                    });
                }
                catch { }
            }
            catch { }
        }
        private void Vl(object sender, RoutedEventArgs e)
        {
            try
            {
                var IsValid = CV(LicenseKey.Text.ToString());
                if (IsValid)
                {
                    LicenseBorder.Visibility = Visibility.Collapsed;
                    this.Background = new SolidColorBrush(Color.FromRgb(91, 140, 144));
                    HomeGrid.Visibility = Visibility.Visible;
                    Status.Visibility = Visibility.Collapsed;
                    Status.Text = string.Empty;
                    CHK();
                }
                else
                {
                    this.Background = new SolidColorBrush(Colors.Transparent);
                    LicenseBorder.Visibility = Visibility.Visible;
                    LicenseBorder.Background = new SolidColorBrush(Color.FromRgb(91, 140, 144));
                    HomeGrid.Visibility = Visibility.Collapsed;
                    Status.Visibility = Visibility.Visible;
                    Status.Text = "Failed to validate...";
                }
            }
            catch { }
        }
    }
}