using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Optimeet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GeocodeHelper helper;
        public MainWindow()
        {
            InitializeComponent();
            helper = GeocodeHelper.GetInstance();
            Location l = new Location();
            l.Latitude = 32.064230f;
            l.Longitude = 34.777988f;
            helper.TopNLocations(l, 10, "restaurant");
        }

        private void ComboBoxStartUpHrs(object sender, RoutedEventArgs e)
        {
            int[] hrs = new int[24];
            for (int i = 0; i < hrs.Length; i++)
            {
                hrs[i] = i;
            }
            var combo = sender as ComboBox;
            combo.ItemsSource = hrs;
            combo.SelectedIndex = 0;
        }

        private void ComboBoxStartUpMins(object sender, RoutedEventArgs e)
        {
            int[] mins = new int[60];
            for (int i = 0; i < mins.Length; i++)
            {
                mins[i] = i;
            }
            var combo = sender as ComboBox;
            combo.ItemsSource = mins;
            combo.SelectedIndex = 0;
        }
        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void MaximizeWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {

                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void MinimizeWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void CreateMeeting(object sender, MouseButtonEventArgs e)
        {
            HistoryButton.Visibility = Visibility.Collapsed;
            SettingsButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;

        }

        private void Highlight(object sender, MouseEventArgs e)
        {
            Border s = (Border)e.Source;
            s.Background = Brushes.LightGray;
        }

        private void Unhighlight(object sender, MouseEventArgs e)
        {
            Border s = (Border)e.Source;
            s.Background = Brushes.Transparent;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
