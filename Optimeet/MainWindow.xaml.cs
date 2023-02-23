using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Optimeet.Meeting;

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
            Date d = new Date { Day=2, Month=5, Year=2023, Hour=21, Minute=30};
            List<Person> p = new List<Person>();
            p.Add(new Person("Ben", new Location { Latitude = 32.0267904f, Longitude = 34.7579567f }));
            p.Add(new Person("Michael", new Location { Latitude = 32.0496376f, Longitude = 34.8059143f }));
            p.Add(new Person("Leon", new Location { Latitude = 31.9770394f, Longitude = 34.7695861f }));
            p.Add(new Person("Evgeniy", new Location { Latitude = 32.432225f, Longitude = 34.920580f }));
            Meeting m = new Meeting("Ben's birthday", d, p);
            m.SuggestLocations("Restaurant");
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
