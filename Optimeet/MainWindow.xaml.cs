using FontAwesome.WPF;
using Google.Apis.Calendar.v3.Data;
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
using System.Windows.Media.Animation;
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
        private Location CreateContactLocation;


        public Location FinalChoice { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            helper = MapsHelper.GetInstance();
            fm = FileManager.GetInstance();
            mapLayer = new MapLayer();
            ApplicationMap.Children.Add(mapLayer);
            ResetViews();
            LoadContactsUI();
            LoadMeetingsUI();
        }


        private void ResetViews()
        {
            //Initiate the default screen
            CreateButton.Visibility = Visibility.Visible;
            CreateButton.IsEnabled = true;
            MeetingsButton.Visibility = Visibility.Visible;
            MeetingsButton.IsEnabled = true;
            InfoButton.Visibility = Visibility.Visible;
            InfoButton.IsEnabled = true;
            SettingsButton.Visibility = Visibility.Visible;
            SettingsButton.IsEnabled = true;
            //Hide openable menus
            CreateMenu.Visibility = Visibility.Collapsed;
            SaveMenu.Visibility = Visibility.Collapsed;
            MeetingsMenu.Visibility = Visibility.Collapsed;
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
            MeetingsButton.Visibility = Visibility.Collapsed;
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
                         ControlTemplate t = CreateTemplatePushpin(box);
                         p.Template = t;
                         ApplicationMap.SetView(p.Location, 15f);
                         mapLayer.Children.Add(p);
                     }
                 };
        }

        private ControlTemplate CreateTemplatePushpin(CheckBox box)
        {
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
            return t;
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

        private void OpenMeetings(object sender, RoutedEventArgs e)
        {
            switch (MeetingsMenu.Visibility)
            {
                case Visibility.Visible:
                    MeetingsMenu.Visibility = Visibility.Collapsed;
                    break;
                case Visibility.Collapsed:
                    MeetingsMenu.Visibility = Visibility.Visible;
                    break;
            }
                
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
                    m.SubmitLocation(FinalChoice);
                    fm.Meetings.Add(m);
                    fm.SaveMeetings();
                    MeetingSaveButton.Tag = "";
                    ApplicationMap.Children.Clear();
                    ResetViews();
                    //Apply the addition
                    FutureList.Children.Clear();
                    UpcomingList.Children.Clear();
                    PastList.Children.Clear();
                    LoadMeetingsUI();
                    OpenMeetings(sender, e);
                    MessageBox.Show("Meeting was saved successfully.");
                }
                else
                {
                    CreateMenu.Visibility = Visibility.Collapsed;
                    MeetingLoadingIcon.Visibility = Visibility.Visible;
                    ShowSuggestions(m);
                    MeetingSaveButton.Tag = "";
                }
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
            //Make a field in scope so it can be used later in click without worrying about 'i' changing
            Location currentLoc = results[i];
            b.Click += (object sender, RoutedEventArgs args) => {
                Highlight(b);
                FinalChoice = currentLoc;
                MeetingSaveButton.Tag = currentLoc.Address;
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
            MeetingSaveButton.Tag = "";
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
        private void LoadMeetingsUI()
        {
            SortedSet<Meeting> MeetingSet = fm.Meetings;
            Queue<Meeting> UpcomingMeetings = new Queue<Meeting>(),
                FutureMeetings = new Queue<Meeting>(),
                PastMeetings = new Queue<Meeting>();
            Meeting LimitMeeting = new Meeting("limit upcoming", DateTime.Now.AddDays(14), null);
            SortedSet<Meeting>.Enumerator AllMeetings = MeetingSet.GetEnumerator();
            AllMeetings.MoveNext(); //First current is null
            for (int i = 0; i < MeetingSet.Count; i++)
            {
                switch(AllMeetings.Current.CompareTo(LimitMeeting))
                {
                    case -1:
                        LimitMeeting.SetMeetingDate(DateTime.Now);
                        if (AllMeetings.Current.CompareTo(LimitMeeting) == -1)
                            PastMeetings.Enqueue(AllMeetings.Current);
                        else UpcomingMeetings.Enqueue(AllMeetings.Current);
                        break;
                    case 0:
                        LimitMeeting.SetMeetingDate(DateTime.Now);
                        if (AllMeetings.Current.CompareTo(LimitMeeting) == -1)
                            PastMeetings.Enqueue(AllMeetings.Current);
                        else UpcomingMeetings.Enqueue(AllMeetings.Current);
                        break;
                    case 1:
                        FutureMeetings.Enqueue(AllMeetings.Current);
                        break;
                }
                LimitMeeting.SetMeetingDate(DateTime.Now.AddDays(14));
                AllMeetings.MoveNext(); 
            }
            //Update counters
            FutureCount.Text = FutureMeetings.Count.ToString();
            UpcomingCount.Text = UpcomingMeetings.Count.ToString();
            PastCount.Text = PastMeetings.Count.ToString();
            //Add lists to UI
            CreateMeetingChildren(FutureMeetings, FutureList);
            CreateMeetingChildren(UpcomingMeetings, UpcomingList);
            CreateMeetingChildren(PastMeetings, PastList);
        }

        private void CreateMeetingChildren(Queue<Meeting> queue, StackPanel target)
        {
            int meetings = queue.Count + 1;
            while (queue.Count > 0)
            {
                Meeting m = queue.Dequeue();
                // Make an attendees list seperated by commas
                List<Contact> people = m.GetPeople();
                string attendees ="";
                foreach (Contact c in people)
                    attendees += c.Name + ", ";
                attendees = attendees.Substring(0, attendees.Length - 2);
                //Build the box view
                //Prepare text blocks
                TextBlock tbMeetingName = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    FontSize = 18,
                    Text = m.Title
                };
                TextBlock tbMeetingAttendees = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    Text = attendees
                };
                TextBlock tbMeetingDate = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    Text = m.GetMeetingDate().ToString("f")
                };
                TextBlock tbMeetingAddress = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    Text = m.GetLocation().Address
                };
                // icons
                Image iPin = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/pin.png", UriKind.Relative)),
                    Height = 40,
                    Width = 40,
                    Margin = new Thickness(0, 0, 10, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                iPin.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    Location l = m.GetLocation();
                    ApplicationMap.SetView(new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude), 18f);
                };
                Image iClock = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/clock.jpg", UriKind.Relative)),
                    Height = 20,
                    Width = 20,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Image iPeople = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/people.png", UriKind.Relative)),
                    Height = 20,
                    Width = 20,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Image iAddress = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/address.png", UriKind.Relative)),
                    Height = 20,
                    Width = 20,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Image iDelete = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/delete.png", UriKind.Relative)),
                    Height = 25,
                    Width = 25,
                    Margin = new Thickness(10,0,0,0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                iDelete.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (MessageBox.Show("Delete Meeting", "Do you want to perform this action?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        fm.Meetings.Remove(m);
                        ResetViews();
                        //Apply the addition
                        FutureList.Children.Clear();
                        UpcomingList.Children.Clear();
                        PastList.Children.Clear();
                        fm.SaveMeetings();
                        LoadMeetingsUI();
                        OpenMeetings(sender, e);
                    }
                };
                Image iMail = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/mail.jpg", UriKind.Relative)),
                    Height = 25,
                    Width = 25,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                iMail.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (MessageBox.Show("Send mail invitation to attendees", "Do you want to perform this action?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        
                    }
                };
                //Encapsulate data in stackpanels
                StackPanel spTitle = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                spTitle.Children.Add(tbMeetingName);
                spTitle.Children.Add(iMail);
                spTitle.Children.Add(iDelete);
                StackPanel spAttendees = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                spAttendees.Children.Add(iPeople);
                spAttendees.Children.Add(tbMeetingAttendees);
                StackPanel spDate = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                spDate.Children.Add(iClock);
                spDate.Children.Add(tbMeetingDate);

                StackPanel spAddress = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                spAddress.Children.Add(iAddress);
                spAddress.Children.Add(tbMeetingAddress);

                StackPanel spTexts = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                spTexts.Children.Add(spTitle);
                spTexts.Children.Add(spAttendees);
                spTexts.Children.Add(spDate);
                spTexts.Children.Add(spAddress);

                StackPanel spParent = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                spParent.Children.Add(iPin);
                spParent.Children.Add(spTexts);

                //Encapsulate in button and add to list
                Button b = new Button
                {
                    Content = spParent,
                    Name = "bt_" + (meetings - queue.Count),
                    Template = (ControlTemplate)FindResource("NoMouseOverButtonTemplate"),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                target.Children.Add(b);
            }
        }

        private void LoadContactsUI()
        {
            Trie<Contact> trContacts = fm.Contacts;
            Queue<Contact> qContacts = trContacts.GetChildren();
            int contacts = qContacts.Count+1;
            while(qContacts.Count > 0)
            { 
                Contact c = qContacts.Dequeue();
                //Build the box
                TextBlock tbContactName = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20,0,0,0),
                    FontSize = 22,
                    Text = c.Name
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
                    Image ic = sender as Image;
                    ic.Tag = c.Name+","+c.Email+","+c.GetLocation().Address;
                    CreateContact(ic, e);

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
                       {
                            fm.Contacts.Delete(c.Name);
                            ContactBook.Children.Clear();
                            fm.SaveContacts();
                            LoadContactsUI();
                       } 
                };
                StackPanel ContactPanel2 = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                TextBlock tbContactAddress = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    Text = c.GetLocation().Address
                };
                TextBlock tbContactMail = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),

                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    Text = c.Email
                };
                ContactPanel2.Children.Add(tbContactName);
                ContactPanel2.Children.Add(tbContactAddress);
                ContactPanel2.Children.Add(tbContactMail);
                ContactPanel.Children.Add(ContactBorder);
                ContactPanel.Children.Add(ContactPanel2);
                ContactPanel.Children.Add(icEdit);
                ContactPanel.Children.Add(icDelete);
                //Encapsulate in button and add to list
                Button b = new Button
                {
                    Content = ContactPanel,
                    Name = "bt_" + (contacts-qContacts.Count),
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
            MeetingsButton.Visibility = Visibility.Collapsed;
            SettingsButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;
            CreateMenu.Visibility = Visibility.Collapsed;
            SaveMenu.Visibility = Visibility.Collapsed;
            CreateButton.Visibility = Visibility.Collapsed;

            //if edit was pressed
            Image ic = sender as Image;
            if (ic!=null && ic.Tag!=null)
            {
                string[] args = ic.Tag.ToString().Split(',');
                CreateContact_Name.Text = args[0];
                CreateContact_Mail.Text = args[1];
                CreateContact_Address.Text = args[2];
                ContactSaveButton.Tag = args[0];
            }

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
            try
            {
                if(CreateContactLocation.Address == null)
                {
                    throw new Exception("Please fill in or enter a valid address");
                }
                Contact contact = new Contact(name, mail);
                contact.SetLocation(CreateContactLocation);
                fm.Contacts.Add(contact.Name, contact);
                //if the contact that is being saved exists already and was edited
                if (ContactSaveButton.Tag != null && !ContactSaveButton.Tag.Equals(""))
                {
                    fm.Contacts.Delete(ContactSaveButton.Tag.ToString());
                    ContactSaveButton.Tag = "";
                }
                fm.SaveContacts();
                ApplicationMap.Children.Clear();
                //Clear data in Create Contact window
                CreateContact_Address.Text = "";
                CreateContact_Mail.Text = "";
                CreateContact_List.Items.Clear();
                CreateContact_Name.Text = "";
                //Get back to menu
                ResetViews();
                //Apply the addition and reopen contact menu
                ContactBook.Children.Clear();
                LoadContactsUI();
                ShowContactsBook(sender, e);
            }
            catch (Exception error)
            {
                string caption = "Could not save contact";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Exclamation;
                MessageBox.Show(error.Message, caption, button, icon);
            }
            
        }

        private async void JumpToAddress(object sender, MouseButtonEventArgs e)
        {
            if (CreateContact_Address.Text.Length > 3)
            {
                CreateContact_List.Items.Clear();
                List<string> suggestions = await helper.SearchLocation(CreateContact_Address.Text);
                foreach (string suggestion in suggestions)
                {
                    ListBoxItem temp = new ListBoxItem() { Content = suggestion };
                    temp.Selected += CreateContact_List_Selected;
                    CreateContact_List.Items.Add(temp);
                }
            }
            else
                CreateContact_List.Items.Clear();
        }

        private async void CreateContact_List_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = e.Source as ListBoxItem;
            if (item != null)
            {
                CreateContact_Address.Text = item.Content.ToString();
                CreateContactLocation = await helper.Geocode(CreateContact_Address.Text);
                CreateContactLocation.Address = CreateContact_Address.Text;
                Pushpin p = new Pushpin()
                {
                    Location = new Microsoft.Maps.MapControl.WPF.Location(CreateContactLocation.Latitude, CreateContactLocation.Longitude)
                };
                ApplicationMap.SetView(p.Location, 17f);
                ApplicationMap.Children.Add(p);            
            }
        }

        private void ShowFutureM(object sender, MouseEventArgs e)
        {
            RotateArrow((Image)e.Source);
            if (FutureArrow.Tag.Equals("Expanded"))
                FutureList.Visibility = Visibility.Visible;
            else if (FutureArrow.Tag.Equals("Collapsed"))
                FutureList.Visibility = Visibility.Collapsed;            
        }

        private void ShowUpcomingM(object sender, MouseEventArgs e)
        {
            RotateArrow((Image)e.Source);
            if (UpcomingArrow.Tag.Equals("Expanded"))
                UpcomingList.Visibility = Visibility.Visible;
            else if (UpcomingArrow.Tag.Equals("Collapsed"))
                UpcomingList.Visibility = Visibility.Collapsed;
        }

        private void ShowPastM(object sender, MouseEventArgs e)
        {
            RotateArrow((Image)e.Source);
            if (PastArrow.Tag.Equals("Expanded"))
                PastList.Visibility = Visibility.Visible;
            else if (PastArrow.Tag.Equals("Collapsed"))
                PastList.Visibility = Visibility.Collapsed;
        }
        private void RotateArrow(Image i)
        {
            RotateTransform trans = new RotateTransform(0, 0.5, 0.5);
            i.RenderTransform = trans;
            EasingDoubleKeyFrame frame1 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 0)), new CircleEase());
            EasingDoubleKeyFrame frame2 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 450)), new CircleEase());
            DoubleKeyFrameCollection frames = new DoubleKeyFrameCollection();
            switch (i.Tag)
            {
                case "Collapsed":
                    frame1 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 0)), new CircleEase());
                    frame2 = new EasingDoubleKeyFrame(90, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 450)), new CircleEase());
                    i.Tag = "Expanded";
                    break;
                case "Expanded":
                    frame1 = new EasingDoubleKeyFrame(90, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 0)), new CircleEase());
                    frame2 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 450)), new CircleEase());
                    i.Tag = "Collapsed";
                    break;
            }
            frames.Add(frame1);
            frames.Add(frame2);
            DoubleAnimationUsingKeyFrames rotateAnim = new DoubleAnimationUsingKeyFrames()
            {
                KeyFrames = frames
            };
            trans.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
        }

        public void createGoogleCalendarEvent()
        {
            
        }
    }
}
