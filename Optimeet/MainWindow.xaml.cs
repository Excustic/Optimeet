using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Optimeet.Meeting;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace Optimeet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MapsHelper helper;
        public MainWindow()
        {
            InitializeComponent();
            ResetViews();
            helper = MapsHelper.GetInstance();
            //DateTime d = new DateTime(2023, 5, 2, 21, 30, 0);
            //List<Person> p = new List<Person>();
            //p.Add(new Person("Ben"), new Location { Latitude = 32.0267904f, Longitude = 34.7579567f }));
            //p.Add(new Person("Michael", new Location { Latitude = 32.0496376f, Longitude = 34.8059143f }));
            //p.Add(new Person("Leon", new Location { Latitude = 31.9770394f, Longitude = 34.7695861f }));
            //p.Add(new Person("Evgeniy", new Location { Latitude = 32.432225f, Longitude = 34.920580f }));
            //Meeting m = new Meeting("Ben's birthday", d, p);
            //m.SuggestLocations("Restaurant");
        }

        private void ResetViews()
        {
            //Initiate the default screen
            CreateButton.Visibility = Visibility.Visible;
            CreateButton.IsEnabled = true;
            HistoryButton.Visibility = Visibility.Visible;
            HistoryButton.IsEnabled = true;
            InfoButton.Visibility = Visibility.Visible;
            InfoButton.IsEnabled = true;
            SettingsButton.Visibility = Visibility.Visible;
            SettingsButton.IsEnabled = true;
            //Hide openable menus
            CreateMenu.Visibility = Visibility.Collapsed;
            SaveMenu.Visibility = Visibility.Collapsed;
            SuggestionsScroll.Visibility = Visibility.Collapsed;
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

        private void CreateMeeting(object sender, RoutedEventArgs e)
        {
            HistoryButton.Visibility = Visibility.Collapsed;
            SettingsButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;
            CreateMenu.Visibility = Visibility.Visible;
            SaveMenu.Visibility = Visibility.Visible;
            CreateButton.IsEnabled = false;
            AddContactList();
        }

        private void AddContactList()
        {
            List<Person> p = new List<Person>();
            Person p1 = new Person("Ben");
            p1.SetLocation(32.0267904f, 34.7579567f);
            Person p2 = new Person("Michael");
            p2.SetLocation(32.0496376f, 34.8059143f);
            Person p3 = new Person("Leon");
            p3.SetLocation(31.9770394f, 34.7695861f);
            Person p4 = new Person("Evgeniy");
            p4.SetLocation(32.432225f, 34.920580f);
            p.Add(p1);
            p.Add(p2);
            p.Add(p3);
            p.Add(p4);
            foreach (Person person in p)
            {
                CheckBox box = new CheckBox();
                box.Content = person;
                ContactList.Children.Add(box);
            }
        }

        private void Highlight(object sender, MouseEventArgs e)
        {
            Button s = (Button)e.Source;
            s.Background = Brushes.LightGray;
        }

        private void Unhighlight(object sender, MouseEventArgs e)
        {
            Button s = (Button)e.Source;
            s.Background = Brushes.Transparent;
        }

        private void Highlight(Button s)
        {
            s.Background = Brushes.LightBlue;
        }

        private void Unhighlight(Button s)
        {
            s.Background = Brushes.LightGray;
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

        private void OpenPastMeetings(object sender, RoutedEventArgs e)
        {

        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {

        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {

        }

        private void CancelMeeting(object sender, RoutedEventArgs e)
        {
            ResetViews();

        }

        private void SaveMeeting(object sender, RoutedEventArgs e)
        {
            Meeting m;
            String subject = MeetingSubject.Text;
            DateTime date = DateTime.MinValue;
            if (DateTimePicker1.SelectedDate != null)
            {
                date = (DateTime)DateTimePicker1.SelectedDate;
                date = date.AddHours(double.Parse(HoursBox.Text));
                date = date.AddMinutes(double.Parse(MinutesBox.Text));
            }
            List<Person> participants = new List<Person>();
            foreach(CheckBox box in ContactList.Children)
                if (box.IsChecked == true)
                    participants.Add((Person)box.Content);
            string error = ValidateMeeting(subject, date, participants);
            if (error == null)
            {
                m = new Meeting(subject, date, participants);
                CreateMenu.Visibility = Visibility.Collapsed;
                MeetingLoadingIcon.Visibility = Visibility.Visible;
                ShowSuggestions(m);
            }
            else
            {
                string caption = "Could not save meeting";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Exclamation;
                MessageBox.Show(error, caption, button, icon);
            }
        }


        private async void ShowSuggestions(Meeting m)
        {
        Location[] results = await m.SuggestLocations(MeetingPlaceFilter.Text);
            for (int i = 0; i < results.Length; i++)
            {
                //Build the box
                Image img = new Image();
                img.Source = await helper.BitmapImageFromUrl(results[i].PhotoReference); 
                TextBlock PlaceTitle = new TextBlock();
                PlaceTitle.Text = results[i].Name;
                PlaceTitle.Margin = new Thickness(0, 5, 0, 0);
                TextBlock PlaceAddress = new TextBlock();
                PlaceAddress.Text = results[i].Address;
                StackPanel reviews = new StackPanel();
                reviews.Orientation = Orientation.Horizontal;
                Image fa = new Image();
                fa.Source=ImageAwesome.CreateImageSource(FontAwesomeIcon.Star, Brushes.Gray);
                fa.Width = 10;
                fa.Height = 10;
                TextBlock description = new TextBlock();
                description.Text = results[i].Rating + " (" + results[i].ReviewCount + " reviews)";
                description.Margin = new Thickness(3, 0, 0, 0);
                reviews.Children.Add(fa);
                reviews.Children.Add(description);
                //Add children
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Vertical;
                panel.Children.Add(img);
                panel.Children.Add(PlaceTitle);
                panel.Children.Add(PlaceAddress);
                panel.Children.Add(reviews);
                img.Width = panel.Width;
                img.Height = 85;
                panel.Children[0] = img;
                //Encapsulate in button and add to list
                Button b = new Button();
                b.Content = panel;
                b.Name = "option" + i;
                b.Template = (ControlTemplate)FindResource("NoMouseOverButtonTemplate");
                b.Background = Brushes.LightGray;
                b.Margin = new Thickness(10,0,0,0);
                SuggestionsList.Children.Add(b);
            }
            foreach(Button b in SuggestionsList.Children)
            {
                b.Click += (object sender, RoutedEventArgs args)=> {
                    Highlight(b); 
                    foreach(Button btn in SuggestionsList.Children)
                    {
                        if (btn != b)                  
                            Unhighlight(btn);
                    }
                        };
            }
        MeetingSaveButton.Content = "Save";
        MeetingLoadingIcon.Visibility = Visibility.Collapsed;
        SuggestionsScroll.Visibility = Visibility.Visible;
        }

        private string ValidateMeeting(string Title, DateTime date, List<Person> people)
        {
            if (Title.Length <= 2)
                return "Please enter a longer subject";
            if (date.Equals(DateTime.MinValue) || DateTime.Compare(date, DateTime.Now) == -1)
                return "Please enter a valid date";
            if (people.Count < 2)
                return "Please add at least 2 participants";
            return null;
        }
    }
}
