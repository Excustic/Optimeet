using FontAwesome.WPF;
using Microsoft.Maps.MapControl.WPF;
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
        MapLayer mapLayer;
        FileManager fm;

        public int FinalChoice { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            helper = MapsHelper.GetInstance();
            fm = FileManager.GetInstance();
            mapLayer = new MapLayer();
            ApplicationMap.Children.Add(mapLayer);
            ResetViews();
            LoadContactsUI();
            //Contact p1 = new Contact("Ben");
            //p1.SetLocation(32.0267904f, 34.7579567f);
            //Contact p2 = new Contact("Michael");
            //p2.SetLocation(32.0496376f, 34.8059143f);
            //Contact p3 = new Contact("Leon");
            //p3.SetLocation(31.9770394f, 34.7695861f);
            //Contact p4 = new Contact("Evgeniy");
            //p4.SetLocation(32.432225f, 34.920580f);
            //Contact p5 = new Contact("Masha");
            //p5.SetLocation(34.421225f, 36.921580f);
            //Contact p6 = new Contact("Leonid");
            //p6.SetLocation(32.233225f, 31.922580f);
            //Contact p7 = new Contact("Michael Stern");
            //p7.SetLocation(33.472115f, 35.220580f);
            //fm.Contacts.Add(p1.Name, p1);
            //fm.Contacts.Add(p2.Name, p2);
            //fm.Contacts.Add(p3.Name, p3);
            //fm.Contacts.Add(p4.Name, p4);
            //fm.Contacts.Add(p5.Name, p5);
            //fm.Contacts.Add(p6.Name, p6);
            //fm.Contacts.Add(p7.Name, p7);
            //fm.SaveContacts();
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
            ContactWindow.Visibility = Visibility.Collapsed;
            SettingsMenu.Visibility = Visibility.Collapsed;
            ContactCreateMenu.Visibility = Visibility.Collapsed;
            //Hide floating elements
            ContactBookBackButton.Visibility = Visibility.Collapsed;
            AddContactButton.Visibility = Visibility.Collapsed;
            //Clear elements
            mapLayer.Children.Clear();
            ContactList.Children.Clear();
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
            ResetViews();
            HistoryButton.Visibility = Visibility.Collapsed;
            SettingsButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;
            CreateMenu.Visibility = Visibility.Visible;
            SaveMenu.Visibility = Visibility.Visible;
            CreateButton.IsEnabled = false;
            AddContactList();
            foreach (CheckBox box in ContactList.Children)
                box.Click += (object sender2, RoutedEventArgs e2) =>
                 {
                     if ((bool)box.IsChecked) {
                         Location l = ((Contact)box.Content).GetLocation();
                         Pushpin p = new Pushpin()
                         {
                             Height = 20,
                             Width = 20,
                             Location = new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude)
                         };
                         //Create a template for a contact pushpin
                         ControlTemplate t = new ControlTemplate(typeof(Pushpin));
                         FrameworkElementFactory elemFactory = new FrameworkElementFactory(typeof(Border));
                         elemFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(20));
                         elemFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
                         elemFactory.SetValue(Border.BackgroundProperty, Brushes.PeachPuff);
                         elemFactory.SetValue(HeightProperty, 20.0);
                         elemFactory.SetValue(WidthProperty, 20.0);
                         elemFactory.SetValue(Border.BorderBrushProperty, Brushes.White);
                         elemFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                         FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
                         textFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                         textFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                         textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                         textFactory.SetValue(TextBlock.TextProperty, ((Contact)box.Content).Name.Substring(0, 2));
                         elemFactory.AppendChild(textFactory);
                         t.VisualTree = elemFactory;
                         p.Template = t;
                         ApplicationMap.SetView(p.Location, 15f);
                         mapLayer.Children.Add(p);
                     }
                 };
        }

        private void AddContactList()
        {
            foreach (Contact person in fm.Contacts.GetChildren())
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

        private void OpenCloseSettings(object sender, RoutedEventArgs e)
        {
            SettingsMenu.Visibility = SettingsMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {

        }

        private void CancelMeeting(object sender, RoutedEventArgs e)
        {
            ResetViews();
            ApplicationMap.Children.Clear();
        }

        private void SaveMeeting(object sender, RoutedEventArgs e)
        {
            Meeting m;
            string subject = MeetingSubject.Text;
            DateTime date = DateTime.MinValue;
            if (DateTimePicker1.SelectedDate != null)
            {
                date = (DateTime)DateTimePicker1.SelectedDate;
                date = date.AddHours(double.Parse(HoursBox.Text));
                date = date.AddMinutes(double.Parse(MinutesBox.Text));
            }
            List<Contact> participants = new List<Contact>();
            foreach (CheckBox box in ContactList.Children)
                if (box.IsChecked == true)
                    participants.Add((Contact)box.Content);
            string error = ValidateMeeting(subject, date, participants);
            if (error == null)
            {
                m = new Meeting(subject, date, participants);
                if (!((Button)sender).Tag.Equals(""))
                {
                    fm.Meetings.Add(m);
                    fm.SaveMeetings();
                    MeetingSaveButton.Tag = "";
                    ResetViews();
                    MessageBox.Show("Meeting was saved successfully.");
                }
                CreateMenu.Visibility = Visibility.Collapsed;
                MeetingLoadingIcon.Visibility = Visibility.Visible;
                ShowSuggestions(m);
                MeetingSaveButton.Tag = FinalChoice;
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
            Pushpin p = new Pushpin();
            p.Content = new TextBlock()
            {
                Text = (i+1).ToString()
            };
            p.Location = new Microsoft.Maps.MapControl.WPF.Location(results[i].Latitude, results[i].Longitude);
            ApplicationMap.Children.Add(p);
            b.Click += (object sender, RoutedEventArgs args) => {
                Highlight(b);
                FinalChoice = i;
                ApplicationMap.SetView(p.Location, 17.6f);
                foreach (Button btn in SuggestionsList.Children)
                {
                    if (btn != b)
                        Unhighlight(btn);
                }
            };
            if(i==0)
                ApplicationMap.SetView(p.Location, 17.6f);

            }
            MeetingSaveButton.Content = "Save";
        MeetingLoadingIcon.Visibility = Visibility.Collapsed;
        SuggestionsScroll.Visibility = Visibility.Visible;
        }

        private string ValidateMeeting(string Title, DateTime date, List<Contact> people)
        {
            if (Title.Length <= 2)
                return "Please enter a longer subject";
            if (date.Equals(DateTime.MinValue) || DateTime.Compare(date, DateTime.Now) == -1)
                return "Please enter a valid date";
            if (people.Count < 2)
                return "Please add at least 2 participants";
            return null;
        }

        private void LoadContactsUI()
        {
            Trie<Contact> trContacts = fm.Contacts;
            Queue<Contact> qContacts = trContacts.GetChildren();
            while(qContacts.Count > 0)
            { 
                Contact c = qContacts.Dequeue();
                //Build the box
                TextBlock ContactDetails = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20,0,0,0),
                    FontSize = 22,
                    Text = c.ToString()
                };
                Border ContactBorder = new Border
                {
                    Height = 100,
                    Width = 100,
                    Background = Brushes.AntiqueWhite,
                    BorderBrush = Brushes.Transparent,
                    CornerRadius = new CornerRadius(80)
                };
                ContactBorder.Child = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 26,
                    Text = c.Name.Substring(0,2)
                };
                StackPanel ContactPanel = new StackPanel()
                {
                    Width=600,
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Image icEdit = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/edit.png", UriKind.Relative)),
                    Height = 25,
                    Width = 25,
                    Margin = new Thickness(20,0,0,0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                icEdit.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    
                };
                Image icDelete = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/delete.png", UriKind.Relative)),
                    Height = 25,
                    Width = 25,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                icDelete.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (MessageBox.Show("Delete Contact", "Do you want to perform this action?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        fm.Contacts.Delete(c.Name);
                };
                ContactPanel.Children.Add(ContactBorder);
                ContactPanel.Children.Add(ContactDetails);
                ContactPanel.Children.Add(icEdit);
                ContactPanel.Children.Add(icDelete);
                //Encapsulate in button and add to list
                Button b = new Button
                {
                    Content = ContactPanel,
                    Name = "bt_" + "",
                    Template = (ControlTemplate)FindResource("NoMouseOverButtonTemplate"),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(30, 20, 0, 0)
                };
                ContactBook.Children.Add(b);
            }
        }

        private void ShowContactsBook(object sender, RoutedEventArgs e)
        {
            ContactWindow.Visibility = Visibility.Visible;
            ContactBookBackButton.Visibility = Visibility.Visible;
            AddContactButton.Visibility = Visibility.Visible;
        }

        private void CloseContactBook(object sender, RoutedEventArgs e)
        {

            ContactWindow.Visibility = Visibility.Collapsed;
            ContactBookBackButton.Visibility = Visibility.Collapsed;
            AddContactButton.Visibility = Visibility.Collapsed;
        }

        private void CreateContact(object sender, RoutedEventArgs e)
        {
            CloseContactBook(sender, e);
            ResetViews();
            HistoryButton.Visibility = Visibility.Collapsed;
            SettingsButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;
            CreateMenu.Visibility = Visibility.Collapsed;
            SaveMenu.Visibility = Visibility.Collapsed;
            CreateButton.Visibility = Visibility.Collapsed;
            ContactCreateMenu.Visibility = Visibility.Visible;


        }

        private void CancelContact(object sender, RoutedEventArgs e)
        {
            ResetViews();
        }

        private void SaveContact(object sender, RoutedEventArgs e)
        {
            string name = CreateContact_Name.Text;
            string mail = CreateContact_Mail.Text;
            string address = CreateContact_Address.Text;
            if(mail.Equals(""))
            {
                try
                {
                    Contact contact = new Contact(name);
                    string[] loc = CreateContact_Address.Text.Split(',');
                    contact.SetLocation(float.Parse(loc[0]), float.Parse(loc[1]));
                }
                catch(Exception error)
                {
                    string caption = "Could not save contact";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Exclamation;
                    MessageBox.Show(error.Message, caption, button, icon);
                }
            }
            else
            {
                try
                {
                    Contact contact = new Contact(name, mail);
                    string[] loc = CreateContact_Address.Text.Split(',');
                    contact.SetLocation(float.Parse(loc[0]), float.Parse(loc[1]));
                }
                catch (Exception error)
                {
                    string caption = "Could not save contact";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Exclamation;
                    MessageBox.Show(error.Message, caption, button, icon);
                }
            }
        }
    }
}
