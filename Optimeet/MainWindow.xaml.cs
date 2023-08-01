using FontAwesome.WPF;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Maps.MapControl.WPF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace Optimeet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <remarks>The methods are sorted by the application's UI button order,
    /// starting from CreateMeeting functions and ending in About function</remarks>
    public partial class MainWindow : Window
    {
        MapsHelper helper;
        MapLayer mapLayer;
        FileManager fm;
        static string[] Scopes = { CalendarService.Scope.CalendarEvents, CalendarService.Scope.Calendar };
        //Google API variables
        private CalendarService service;
        private UserCredential credential;
        private readonly string secretsPath = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet", "client_secrets.json");
        private readonly string credPath = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet", "token.json");
        //Used for picking a meeting location
        private Location FinalChoice; 
        private Location CreateContactLocation; //Used for saving a new contact

        public MainWindow()
        {
            InitializeComponent();
            //Initialize singleton objects
            helper = MapsHelper.GetInstance();
            fm = FileManager.GetInstance();
            //Create map UI layer
            mapLayer = new MapLayer();
            ApplicationMap.Children.Add(mapLayer);
            ResetViews();
            //Preload all menus
            LoadContactsUI();
            LoadMeetingsUI();
            LoadSettingsUI();
            //Try to sign into a google account
            SignInOutGoogleAccount(null, new RoutedEventArgs());
        }
        /// <summary>
        /// Main UI method which resets all of the elements and brings it back to the default state.
        /// </summary>
        private void ResetViews()
        {
            //Initiate the default screen
            CreateButton.Visibility = MeetingsButton.Visibility =
                InfoButton.Visibility = SettingsButton.Visibility =
                ContactsButton.Visibility = Visibility.Visible;
            CreateButton.IsEnabled = MeetingsButton.IsEnabled = InfoButton.IsEnabled =
                SettingsButton.IsEnabled = ContactsButton.IsEnabled = true;
            //Hide openable menus
            CreateMenu.Visibility = SaveMenu.Visibility =
                MeetingsMenu.Visibility = SuggestionsScroll.Visibility =
                ContactWindow.Visibility = SettingsMenu.Visibility =
                ContactCreateMenu.Visibility = Visibility.Collapsed;
            //Hide floating elements
            AddContactButton.Visibility = SearchContactWrapper.Visibility = Visibility.Collapsed;
            //Clear elements
            mapLayer.Children.Clear(); //Clear the pins from the map view
            ////Create Meeting menu elements
            ContactList.Children.Clear();
            MeetingSubject.Text = SearchContactName.Text = "";
            MeetingPlaceFilter.SelectedIndex = HoursBox.SelectedIndex = MinutesBox.SelectedIndex = 0;
            DateTimePicker1.SelectedDate = null;
            MeetingSaveButton.Content = "Show place suggestions";
            MeetingSaveButton.Tag = "";
            MeetingLoadingIcon.Visibility = Visibility.Collapsed;
            SuggestionsList.Children.Clear();
            ////Create Contact menu element
            CreateContact_Name.Text = SearchName.Text =
                CreateContact_Address.Text = CreateContact_Mail.Text = "";
        }

        //...............................CREATE MEETING FUNCTIONALITY...............................

        /// <summary>
        /// Opens the CreateMeeting menu and its elements while collapsing all of the other menus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateMeeting(object sender, RoutedEventArgs e)
        {
            //This makes sure there is no conflicting UI elements
            ResetViews();
            //Hides unwanted buttons and makes sure only CreateMeeting Menu UI elements are visibile on the left panel
            MeetingsButton.Visibility = SettingsButton.Visibility =
                ContactsButton.Visibility = InfoButton.Visibility = Visibility.Collapsed;
            CreateMenu.Visibility = SaveMenu.Visibility = Visibility.Visible;
            //Avoid conflicting duplicate CreateMeeting method instances running by disabling clickability
            CreateButton.IsEnabled = false;
            //Load contact list for the user to pick from
            AddContactList();
            foreach (CheckBox box in ContactList.Children)
                //Place a round icon on the map at the contact's location with their identifier 
                box.Click += (object sender2, RoutedEventArgs e2) =>
                {
                    Location l = ((Contact)box.Content).GetLocation();
                    if ((bool)box.IsChecked)
                    {
                        Pushpin p = new Pushpin()
                        {
                            Height = 20,
                            Width = 20,
                            Location = new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude)
                        };
                        //Create a template for a contact pushpin
                        ControlTemplate t = CreateTemplatePushpin(box);
                        p.Template = t;
                        //Set map's center to the pushpin's location
                        ApplicationMap.SetView(p.Location, 15f);
                        mapLayer.Children.Add(p);
                    }
                    else
                    {
                        //Remove the round icon from the map
                        var children = mapLayer.Children.GetEnumerator();
                        //Find the pushpin by comparing locations
                        while (children.MoveNext())
                        {
                            Pushpin p = (Pushpin)children.Current;
                            if (p.Location.Equals(new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude)))
                            {
                                mapLayer.Children.Remove(p);
                                return;
                            }
                        }
                    }
                };
            //If a Google account is signed in, this adds an option to integrate the meeting with Google Calendar
            if (service != null)
            {
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Name = "googleInvite" };
                TextBlock tb = new TextBlock() { Text = "Add to Google calendar?", VerticalAlignment = VerticalAlignment.Center };
                //Create ComboBox and append items to it
                ComboBox cb = new ComboBox() { Name = "calendar_options" };
                ComboBoxItem cbi1 = new ComboBoxItem() { Content = "No" };
                ComboBoxItem cbi2 = new ComboBoxItem() { Content = "Yes" };
                ComboBoxItem cbi3 = new ComboBoxItem() { Content = "Yes, invite participants too" };
                cb.Items.Add(cbi1);
                cb.Items.Add(cbi2);
                cb.Items.Add(cbi3);
                cb.SelectedIndex = 0;
                //Wrap it up in a StackPanel
                sp.Children.Add(tb);
                sp.Children.Add(cb);
                //Insert to createmenu
                CreateMenu.Children.Add(sp);
            }
        }
        /// <summary>
        /// Adds contacts to a list view in CreateMeeting Menu
        /// </summary>
        /// <param name="filter">Optional paramater allowing to add contacts of names starting with given string.</param>
        private void AddContactList(string filter = "")
        {
            Queue<Contact> contacts = null;
            if (filter.Equals(""))
                contacts = fm.Contacts.GetChildren();
            else
                contacts = fm.Contacts.GetChildren(filter);
            foreach (Contact person in contacts)
            {
                CheckBox box = new CheckBox();
                box.Content = person;
                ContactList.Children.Add(box);
            }
        }
        /// <summary>
        /// Show contacts for the specified entry in the TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContactLookup(object sender, RoutedEventArgs e)
        {
            switch (SearchButton.Tag)
            {
                //Show contacts for this specified query string
                case "Search":
                    SearchButton.Tag = "Reset";
                    SearchButtonImage.Source = new BitmapImage(new Uri(@"/Assets/close.png", UriKind.Relative));
                    Queue<Contact> results = fm.Contacts.GetChildren(SearchName.Text);
                    for (int i = 0; i < ContactList.Children.Count; i++)
                    {
                        var box = (CheckBox)ContactList.Children[i];
                        Contact c = (Contact)box.Content;
                        if (results.Count == 0 || !c.Name.Equals(results.Peek().Name))
                            box.Visibility = Visibility.Collapsed;
                        else
                            results.Dequeue();
                    }
                    break;
                //Show all contacts
                case "Reset":
                    SearchButton.Tag = "Search";
                    SearchButtonImage.Source = new BitmapImage(new Uri(@"/Assets/search.png", UriKind.Relative));
                    foreach (CheckBox box in ContactList.Children)
                        box.Visibility = Visibility.Visible;
                    break;
            }
        }
        /// <summary>
        /// Creates the UI for the round icon pushpin of a contact
        /// </summary>
        /// <param name="box"></param>
        /// <returns>A <see cref="ControlTemplate"/> instance</returns>
        private ControlTemplate CreateTemplatePushpin(CheckBox box)
        {
            ControlTemplate t = new ControlTemplate(typeof(Pushpin));
            FrameworkElementFactory elemFactory = new FrameworkElementFactory(typeof(Border));
            //Creates a Border element and makes it round
            elemFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(20));
            elemFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            elemFactory.SetValue(Border.BackgroundProperty, Brushes.PeachPuff);
            elemFactory.SetValue(HeightProperty, 20.0);
            elemFactory.SetValue(WidthProperty, 20.0);
            elemFactory.SetValue(Border.BorderBrushProperty, Brushes.White);
            elemFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            //Creates the TextBlock element assigned with the contact's identifier
            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
            textFactory.SetValue(TextBlock.TextProperty, ((Contact)box.Content).Name.Substring(0, 2));
            //Wrap the TextBlock inside Border
            elemFactory.AppendChild(textFactory);
            t.VisualTree = elemFactory;
            return t;
        }
        /// <summary>
        /// Aborts the meeting creation and returns the application to its default state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelMeeting(object sender, RoutedEventArgs e)
        {
            ResetViews();
            mapLayer.Children.Clear();
            //If a google account is signed in, remove Google Calendar integration option list
            if (service != null)
                CreateMenu.Children.RemoveAt(CreateMenu.Children.Count - 1);
        }
        /// <summary>
        /// Saves the meeting in the application's storage file and adds it to Meetings menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveMeeting(object sender, RoutedEventArgs e)
        {
            Meeting m;
            string subject = MeetingSubject.Text;
            //If date was not given, this will set it to 01/01/1970 00:00
            DateTime date = DateTime.MinValue;
            if (DateTimePicker1.SelectedDate != null)
            {
                date = (DateTime)DateTimePicker1.SelectedDate;
                date = date.AddHours(double.Parse(HoursBox.Text));
                date = date.AddMinutes(double.Parse(MinutesBox.Text));
            }
            //Add chosen attendees
            List<Contact> participants = new List<Contact>();
            foreach (CheckBox box in ContactList.Children)
                if (box.IsChecked == true)
                    participants.Add((Contact)box.Content);
            //Validate meeting, check the function for more info
            string error = ValidateMeeting(subject, date, participants);
            if (error == null)
            {
                //Meeting data was validated, continues on to create and store it
                m = new Meeting(subject, date, participants);
                //If a suggested place was chosen
                if (!((Button)sender).Tag.Equals(""))
                {
                    m.SubmitLocation(FinalChoice);
                    //If a Google account is signed in
                    if (service != null)
                    {
                        StackPanel sp = (StackPanel)CreateMenu.Children[CreateMenu.Children.Count - 1];
                        ComboBox cb = (ComboBox)sp.Children[1];
                        if (cb.SelectedIndex != 0)
                        {
                            //Add to google calendar and retrieve event id for future use
                            m.googleId = CreateEvent(m, cb.SelectedIndex == 2);
                        }
                        //Remove the Google Calendar integration options list as it is a dynamically-created element
                        CreateMenu.Children.RemoveAt(CreateMenu.Children.Count - 1);
                    }
                    //Adds the meeting to the existing list and saves it
                    fm.Meetings.Add(m);
                    fm.SaveMeetings();
                    //Reset UI
                    MeetingSaveButton.Tag = "";
                    mapLayer.Children.Clear();
                    SuggestionsList.Children.Clear();
                    ResetViews();
                    //Apply the addition of the meeting to the existing list in the Meetings menu
                    //Clear and reload the meeting list unto the UI
                    FutureList.Children.Clear();
                    UpcomingList.Children.Clear();
                    PastList.Children.Clear();
                    LoadMeetingsUI();
                    //Opens the meetings menu as a feedback
                    OpenCloseMeetings(sender, e);
                    //Additional feedback
                    MessageBox.Show("Meeting was saved successfully.");
                }
                else
                {
                    //Suggest button was pressed and still not ready to transform to a save button
                    CreateMenu.Visibility = Visibility.Collapsed;
                    MeetingLoadingIcon.Visibility = Visibility.Visible;
                    //Load suggestions list and let user choose
                    ShowSuggestions(m);
                    //Tag is used as a helper property to determine the suggestioned place that was chosen, if at all
                    MeetingSaveButton.Tag = "";
                    MeetingSaveButton.IsEnabled = false;
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
        /// <summary>
        /// Validates a meeting by making sure the subject is at least 3 letters long and not identical to other meeting,
        /// the date is set in the future and there are at least 2 attendees
        /// </summary>
        /// <param name="Title">Meeting's subject</param>
        /// <param name="date">Meeting's date and time</param>
        /// <param name="people">Meeting's attendees</param>
        /// <returns></returns>
        private string ValidateMeeting(string Title, DateTime date, List<Contact> people)
        {
            if (Title.Length <= 2)
                return "Please enter a longer subject";
            if (date.Equals(DateTime.MinValue) || DateTime.Compare(date, DateTime.Now) == -1)
                return "Please enter a valid date";
            if (people.Count < 2)
                return "Please add at least 2 participants";
            SortedSet<Meeting>.Enumerator m = fm.Meetings.GetEnumerator();
            while (m.MoveNext())
                if (m.Current.Title.Equals(Title))
                    return "A meeting with the same subject already exists, please type a different subject";
            return null;
        }
        /// <summary>
        /// Loads the suggested places unto the UI
        /// </summary>
        /// <param name="m">Meeting which the suugestions are based on</param>
        private async void ShowSuggestions(Meeting m)
        {
            //Get suggested places
            Location[] results = await m.SuggestLocations(MeetingPlaceFilter.Text);
            //Iterate through the places and build the UI for them 
            for (int i = 0; i < results.Length; i++)
            {
                //Build the box
                //UI First row
                Image img = new Image();
                img.Source = await helper.BitmapImageFromUrl(results[i].PhotoReference);
                //UI second row
                TextBlock PlaceTitle = new TextBlock();
                PlaceTitle.Text = (i + 1) + ": " + results[i].Name;
                PlaceTitle.Margin = new Thickness(0, 5, 0, 0);
                //UI third row
                TextBlock PlaceAddress = new TextBlock();
                PlaceAddress.Text = results[i].Address;
                //UI fourth row
                StackPanel reviews = new StackPanel();
                reviews.Orientation = Orientation.Horizontal;
                Image fa = new Image();
                fa.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Star, Brushes.Gray);
                fa.Width = 10;
                fa.Height = 10;
                TextBlock description = new TextBlock();
                description.Text = results[i].Rating + " (" + results[i].ReviewCount + " reviews)";
                description.Margin = new Thickness(3, 0, 0, 0);
                //Encapsulate fourth row elements in stackpanel
                reviews.Children.Add(fa);
                reviews.Children.Add(description);
                //Encapsulate everything in a stackpanel
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Vertical;
                panel.Children.Add(img);
                panel.Children.Add(PlaceTitle);
                panel.Children.Add(PlaceAddress);
                panel.Children.Add(reviews);
                img.Width = panel.Width;
                img.Height = 85;
                panel.Children[0] = img;
                //Wrap stackpanel in button and add to list
                Button b = new Button();
                b.Content = panel;
                b.Name = "option" + i;
                b.Template = (ControlTemplate)FindResource("NoMouseOverButtonTemplate");
                b.Background = Brushes.LightGray;
                b.Margin = new Thickness(10, 0, 0, 0);
                SuggestionsList.Children.Add(b);
                Pushpin p = new Pushpin();
                p.Content = new TextBlock()
                {
                    Text = (i + 1).ToString()
                };
                //Show place on map
                p.Location = new Microsoft.Maps.MapControl.WPF.Location(results[i].Latitude, results[i].Longitude);
                mapLayer.Children.Add(p);
                //Assign the location to a new local variable, making b.Click behave
                //accordingly to this specific location
                Location currentLoc = results[i];
                b.Click += (object sender, RoutedEventArgs args) =>
                {
                    //Highlight button and remember choice
                    Highlight(b);
                    FinalChoice = currentLoc;
                    MeetingSaveButton.Tag = currentLoc.Address;
                    //Center around choice
                    ApplicationMap.SetView(p.Location, 17.6f);
                    foreach (Button btn in SuggestionsList.Children)
                    {
                        if (btn != b)
                            Unhighlight(btn);
                    }
                    //Proceed to saving by enabling save button
                    MeetingSaveButton.IsEnabled = true;
                };
                //Center around first suggested place
                if (i == 0)
                    ApplicationMap.SetView(p.Location, 17.6f);
            }
            //After creating all buttons and the UI being ready, transform suggestion button
            //back to a save button functionality, still not enabled because no choice was picked.
            MeetingSaveButton.Content = "Save";
            MeetingSaveButton.Tag = "";
            //Prevents a bug from showing previous suggestions of a meeting that was cancelled
            if (CreateButton.IsEnabled)
            {
                SuggestionsList.Children.Clear();
                mapLayer.Children.Clear();
                return;
            }
            //Make necessary UI changes
            MeetingLoadingIcon.Visibility = Visibility.Collapsed;
            SuggestionsScroll.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Creates a Google Calendar event in the Google account's primary calendar.
        /// </summary>
        /// <param name="m">The meeting which the event will be based on</param>
        /// <param name="invite">A boolean value that determines whether attendees should be invited or not</param>
        /// <returns>The Google Calendar event unique ID</returns>
        /// <remarks>(Optional) Invites attendees of the given meeting via Google Calendar and mail</remarks>
        private string CreateEvent(Meeting m, bool invite)
        {
            //If attendees are invited, convert Contact list to a EventAttendee list
            List<EventAttendee> eventAttendees = new List<EventAttendee>();
            if (invite)
                foreach (Contact c in m.GetAttendees())
                    if (c.Email != null)
                        eventAttendees.Add(new EventAttendee
                        {
                            DisplayName = c.Name,
                            Email = c.Email
                        });
            //Create event
            var myEvent = new Event
            {
                Summary = m.Title,
                Location = m.GetLocation().ToString(),
                ColorId = "6",

                Start = new EventDateTime()
                {
                    DateTime = m.GetMeetingDate(),
                    TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault().ToString() //This converts windows timezone to a readable IANA format
                },
                End = new EventDateTime()
                {
                    DateTime = m.GetMeetingDate().AddMinutes(fm.Settings[FileManager.SETTING_3][1]), //Chooses duration of meeting based on defined application settings
                    TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault().ToString() //This converts windows timezone to a readable IANA format
                },
                Attendees = eventAttendees,
                Description = "This meeting was created using Optimeet, visit https://github.com/Excustic/Optimeet for more information."
            };
            //Insert the event to the account's primary Google Calendar
            var Event = service.Events.Insert(myEvent, "primary");
            Event.SendNotifications = true;
            var results = Event.Execute();
            //Adds visual feedback
            MessageBox.Show("The meeting was successfully inserted to your google calendar");
            return results.Id;
        }
        /// <summary>
        /// Initializes the the hours ComboBox associated with CreateMeeting menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Initializes the the minutes ComboBox associated with CreateMeeting menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        //...............................MEETINGS MENU FUNCTIONALITY...............................

        /// <summary>
        /// Open or closes the Meeting menu UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCloseMeetings(object sender, RoutedEventArgs e)
        {
            switch (MeetingsMenu.Visibility)
            {
                case Visibility.Visible:
                    ResetViews();
                    break;
                case Visibility.Collapsed:
                    ResetViews();
                    MeetingsMenu.Visibility = Visibility.Visible;
                    break;
            }

        }
        /// <summary>
        /// Loads meetings unto Meetings menu's UI and divides them by past, upcoming, future meetings
        /// </summary>
        private void LoadMeetingsUI()
        {
            SortedSet<Meeting> MeetingSet = fm.Meetings;
            if (MeetingSet.Count == 0)
                return;
            //Division to 3 queues of meetings
            Queue<Meeting> UpcomingMeetings = new Queue<Meeting>(),
                FutureMeetings = new Queue<Meeting>(),
                PastMeetings = new Queue<Meeting>();
            //A helper object which will be compared to for sorting, defined by the upcoming meeting setting
            Meeting LimitMeeting = new Meeting("limit upcoming", DateTime.Now.AddDays(fm.Settings[FileManager.SETTING_4][1] * 7), null);
            //iterate through meetings
            SortedSet<Meeting>.Enumerator AllMeetings = MeetingSet.GetEnumerator();
            AllMeetings.MoveNext(); //First current is null
            for (int i = 0; i < MeetingSet.Count; i++)
            {
                switch (AllMeetings.Current.CompareTo(LimitMeeting))
                {
                    //Meeting is earlier than upcoming deadline
                    case -1:
                        //Finds out if meeting was in the past or is an upcoming one and appends the meeting correspondingly
                        DateTime temp = LimitMeeting.GetMeetingDate();
                        LimitMeeting.SetMeetingDate(DateTime.Now);
                        if (AllMeetings.Current.CompareTo(LimitMeeting) == -1)
                            PastMeetings.Enqueue(AllMeetings.Current);
                        else UpcomingMeetings.Enqueue(AllMeetings.Current);
                        LimitMeeting.SetMeetingDate(temp);
                        break;
                    //Meeting is same as upcoming deadline
                    case 0:
                        UpcomingMeetings.Enqueue(AllMeetings.Current);
                        break;
                    //Meeting is later than upcoming deadline
                    case 1:
                        FutureMeetings.Enqueue(AllMeetings.Current);
                        break;
                }
                AllMeetings.MoveNext();
            }
            //Update counters
            FutureCount.Text = FutureMeetings.Count.ToString();
            UpcomingCount.Text = UpcomingMeetings.Count.ToString();
            PastCount.Text = PastMeetings.Count.ToString();
            //Add lists to UI
            CreateMeetingChildren(FutureMeetings, FutureList);
            CreateMeetingChildren(UpcomingMeetings, UpcomingList);
            CreateMeetingChildren(PastMeetings, PastList, true);
        }
        /// <summary>
        /// Create the UI elements for every meeting for its designated queue 
        /// </summary>
        /// <param name="queue">The meeting queue from which meeting information is extracted</param>
        /// <param name="target">The designated category list for the meetins (past, upcoming, future)</param>
        /// <param name="past">A boolean paramater used for removal of invitation button if the meeting happened already</param>
        private void CreateMeetingChildren(Queue<Meeting> queue, StackPanel target, bool past = false)
        {
            int meetings = queue.Count + 1;
            while (queue.Count > 0)
            {
                Meeting m = queue.Dequeue();
                // Make an attendees list seperated by commas
                List<Contact> people = m.GetAttendees();
                string attendees = "";
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
                TextBox tbMeetingAttendees = new TextBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    TextWrapping = TextWrapping.NoWrap,
                    Width = 200,
                    Text = attendees,
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
                //Add Image icons
                Image iPin = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/pin.png", UriKind.Relative)),
                    Height = 40,
                    Width = 40,
                    Margin = new Thickness(0, 0, 10, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                //When pin icon pressed, center the map around contact's location
                iPin.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    Location l = m.GetLocation();
                    Pushpin p = new Pushpin()
                    {
                        Location = new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude)
                    };
                    mapLayer.AddChild(p, p.Location);
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
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                //Delete the meeting
                iDelete.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (MessageBox.Show("Delete Meeting", "Do you want to perform this action?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        fm.Meetings.Remove(m);
                        ResetViews();
                        //Apply the change and reload UI
                        FutureList.Children.Clear();
                        UpcomingList.Children.Clear();
                        PastList.Children.Clear();
                        fm.SaveMeetings();
                        LoadMeetingsUI();
                        OpenCloseMeetings(sender, e);
                        //If Google account is signed in, remove the event from account's calendar
                        if (service != null)
                        {
                            DeleteFromCalendar(m);
                        }
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
                //If mail icon pressed, send invitation to attendees via Google Calendar service
                iMail.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (MessageBox.Show("Send mail invitation to attendees", "Do you want to perform this action?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        CreateEvent(m, true);
                    }
                };
                //Encapsulate data in stackpanels
                StackPanel spTitle = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                //Meeting UI first row
                spTitle.Children.Add(tbMeetingName);
                if (!past) //Hiding mail button for past meetings
                    spTitle.Children.Add(iMail);
                spTitle.Children.Add(iDelete);
                //Meeting UI second row
                StackPanel spAttendees = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                spAttendees.Children.Add(iPeople);
                spAttendees.Children.Add(tbMeetingAttendees);
                //Meeting UI third row
                StackPanel spDate = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                spDate.Children.Add(iClock);
                spDate.Children.Add(tbMeetingDate);
                //Meeting UI fourth row
                StackPanel spAddress = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                spAddress.Children.Add(iAddress);
                spAddress.Children.Add(tbMeetingAddress);
                //Encapsulate all rows in one stackpanel
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
                //Combine rows with pin icon in a stackpanel
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
        /// <summary>
        /// Deletes a Google Calendar event from the account's primary calendar
        /// </summary>
        /// <param name="m">Meeting of which the event is based on</param>
        private void DeleteFromCalendar(Meeting m)
        {
            try
            {
                service.Events.Delete("primary", m.googleId).Execute();
            }
            catch (Exception)
            {
                //No event found
            }

        }
        /// <summary>
        /// Opens the future meetings list UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFutureM(object sender, MouseEventArgs e)
        {
            RotateArrow((Image)e.Source);
            if (FutureArrow.Tag.Equals("Expanded"))
                FutureList.Visibility = Visibility.Visible;
            else if (FutureArrow.Tag.Equals("Collapsed"))
                FutureList.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Opens the upcoming meetings list UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowUpcomingM(object sender, MouseEventArgs e)
        {
            RotateArrow((Image)e.Source);
            if (UpcomingArrow.Tag.Equals("Expanded"))
                UpcomingList.Visibility = Visibility.Visible;
            else if (UpcomingArrow.Tag.Equals("Collapsed"))
                UpcomingList.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Opens the past meetings list UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowPastM(object sender, MouseEventArgs e)
        {
            RotateArrow((Image)e.Source);
            if (PastArrow.Tag.Equals("Expanded"))
                PastList.Visibility = Visibility.Visible;
            else if (PastArrow.Tag.Equals("Collapsed"))
                PastList.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Rotates the arrow icon beside the meetings' lists TextBlocks
        /// </summary>
        /// <param name="i"></param>
        private void RotateArrow(Image i)
        {
            //Makes the arrow rotate around its center
            RotateTransform trans = new RotateTransform(0, 0.5, 0.5);
            i.RenderTransform = trans;
            //keyframes are initialized in case switch statement fails
            EasingDoubleKeyFrame frame1 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 0)), new CircleEase());
            EasingDoubleKeyFrame frame2 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 450)), new CircleEase());
            DoubleKeyFrameCollection frames = new DoubleKeyFrameCollection();
            //Checks if the arrow is in open or closed state
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

        //...............................CONTACTS MENU FUNCTIONALITY...............................

        /// <summary>
        /// Creates the UI elements for every contact
        /// </summary>
        /// <param name="name">Optional parameter used for showing contacts that start with a
        /// specific string</param>
        private void LoadContactsUI(string name = "")
        {
            //Load stored contacts
            Trie<Contact> trContacts = fm.Contacts;
            Queue<Contact> qContacts = trContacts.GetChildren(name);
            //Used for iterating through contact queue
            int contacts = qContacts.Count + 1;
            while (qContacts.Count > 0)
            {
                Contact c = qContacts.Dequeue();
                //Build the box, starting from left to Right
                StackPanel ContactPanel = new StackPanel()
                {
                    Width = 600,
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left
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
                    Text = c.Name.Substring(0, 2)
                };
                //Texts section
                StackPanel ContactPanelTexts = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                TextBlock tbContactName = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 22,
                    Text = c.Name
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
                ContactPanelTexts.Children.Add(tbContactName);
                ContactPanelTexts.Children.Add(tbContactAddress);
                ContactPanelTexts.Children.Add(tbContactMail);
                //Image icons
                Image icEdit = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/edit.png", UriKind.Relative)),
                    Height = 25,
                    Width = 25,
                    Margin = new Thickness(20, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                //Edit button will essentially call createcontact function but will inject the contact's detail beforehand
                icEdit.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    Image ic = sender as Image;
                    ic.Tag = c.Name + "," + c.Email + "," + c.GetLocation().Address;
                    CreateContact(ic, e);

                };
                Image icDelete = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/Assets/delete.png", UriKind.Relative)),
                    Height = 25,
                    Width = 25,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                //Delete button will remove the contact and reload UI
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
                //Encapsulate everything in one stackpanel
                ContactPanel.Children.Add(ContactBorder);
                ContactPanel.Children.Add(ContactPanelTexts);
                ContactPanel.Children.Add(icEdit);
                ContactPanel.Children.Add(icDelete);
                //Wrap it in a button and add to list
                Button b = new Button
                {
                    Content = ContactPanel,
                    Name = "bt_" + (contacts - qContacts.Count),
                    Template = (ControlTemplate)FindResource("NoMouseOverButtonTemplate"),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(30, 20, 0, 0)
                };
                ContactBook.Children.Add(b);
            }
        }
        /// <summary>
        /// Opens and closes the Contact Book window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCloseContactsBook(object sender, RoutedEventArgs e)
        {
            Visibility vis;
            if (ContactWindow.Visibility.Equals(Visibility.Visible))
                vis = Visibility.Collapsed;
            else
            {
                ResetViews();
                vis = Visibility.Visible;
            }
            ContactWindow.Visibility =
                SearchContactWrapper.Visibility =
                AddContactButton.Visibility = vis;
        }
        /// <summary>
        /// Opens a new window with a filling form on the left pane 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateContact(object sender, RoutedEventArgs e)
        {
            OpenCloseContactsBook(sender, e);
            ResetViews();
            //Hide all of the menu buttons
            MeetingsButton.Visibility = SettingsButton.Visibility =
                InfoButton.Visibility = CreateMenu.Visibility =
                SaveMenu.Visibility = ContactsButton.Visibility =
                CreateButton.Visibility = Visibility.Collapsed;

            //if edit was pressed
            Image ic = sender as Image;
            if (ic != null && ic.Tag != null)
            {
                string[] args = ic.Tag.ToString().Split(',');
                CreateContact_Name.Text = args[0];
                CreateContact_Mail.Text = args[1];
                CreateContact_Address.Text = args[2];
                ContactSaveButton.Tag = args[0];
            }

            ContactCreateMenu.Visibility = Visibility.Visible;


        }
        /// <summary>
        /// Resets the application to its default state, called only when cancel button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelContact(object sender, RoutedEventArgs e)
        {
            ResetViews();
        }
        /// <summary>
        /// Saves contact information filled in the CreateContact form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveContact(object sender, RoutedEventArgs e)
        {
            string name = CreateContact_Name.Text;
            string mail = CreateContact_Mail.Text;
            try
            {
                //Validate contact's details
                if (CreateContactLocation.Address == null)
                {
                    throw new Exception("Please fill in or enter a valid address");
                }
                Contact contact = new Contact(name, mail);
                contact.SetLocation(CreateContactLocation);
                //If the contact that is being saved already exists and was edited
                if (ContactSaveButton.Tag != null && !ContactSaveButton.Tag.Equals(""))
                {
                    fm.Contacts.Edit(ContactSaveButton.Tag.ToString(), contact);
                    ContactSaveButton.Tag = "";
                }
                //If the contact is a new one
                else
                    fm.Contacts.Add(contact.Name, contact);
                fm.SaveContacts();
                //Clear data in Create Contact window
                CreateContact_Address.Text = CreateContact_Mail.Text = CreateContact_Name.Text = "";
                CreateContact_List.Items.Clear();
                //Reset UI
                ResetViews();
                //Apply the addition and reopen contact menu
                ContactBook.Children.Clear();
                LoadContactsUI();
                OpenCloseContactsBook(sender, e);
            }
            catch (Exception error)
            {
                string caption = "Could not save contact";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Exclamation;
                MessageBox.Show(error.Message, caption, button, icon);
            }

        }
        /// <summary>
        /// Brings autocomplete suggestions for the entered query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void JumpToAddress(object sender, MouseButtonEventArgs e)
        {
            if (CreateContact_Address.Text.Length > 3)
            {
                CreateContact_List.Items.Clear();
                List<string> suggestions = await helper.SearchLocation(CreateContact_Address.Text);
                if (suggestions != null)
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
        /// <summary>
        /// Creates a pushpin for the chosen autocomplete entry and centers map around it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                mapLayer.Children.Add(p);
            }
        }
        /// <summary>
        /// Show contacts for the specified entry in the TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchContact(object sender, RoutedEventArgs e)
        {
            switch (SearchContactButton.Tag)
            {
                case "Search":
                    SearchContactButton.Tag = "Reset";
                    SearchContactImage.Source = new BitmapImage(new Uri(@"/Assets/close.png", UriKind.Relative));
                    if (!SearchContactName.Text.Equals(""))
                    {
                        ContactBook.Children.Clear();
                        LoadContactsUI(SearchContactName.Text);
                    }
                    break;
                case "Reset":
                    SearchContactButton.Tag = "Search";
                    SearchContactImage.Source = new BitmapImage(new Uri(@"/Assets/search.png", UriKind.Relative));
                    ContactBook.Children.Clear();
                    LoadContactsUI();
                    break;
            }
        }

        //...............................SETTINGS MENU FUNCTIONALITY...............................

        /// <summary>
        /// A UI method for creating the elements imported from <see cref="FileManager.Settings"/> under the settings menu 
        /// </summary>
        private void LoadSettingsUI()
        {
            int count = 0;
            foreach (KeyValuePair<string, int[]> setting in fm.Settings)
            {
                //Setting name
                TextBlock t = new TextBlock()
                {
                    Text = setting.Key,
                    Margin = new Thickness(0, 10, 0, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Name = "block_" + count
                };
                //A dock which contains slider and a textbox
                DockPanel d = new DockPanel()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10)
                };
                //Textbox which represents the value of the slider
                TextBox val = new TextBox()
                {
                    Text = setting.Value[1].ToString(),
                    Width = 40,
                    TextAlignment = TextAlignment.Center,
                    Name = "val_" + count
                };
                //Slider which gets min-max-default values from setting array
                Slider sl = new Slider()
                {
                    Minimum = setting.Value[0],
                    Maximum = setting.Value[2],
                    Value = setting.Value[1],
                    TickFrequency = setting.Value[3],
                    TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                    IsSnapToTickEnabled = true,
                    Name = "slResults_" + count,
                    Tag = setting.Key
                };
                //Assigns the textbox new value every time the slider position is changed
                sl.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> arg) =>
                {
                    int[] arr = fm.Settings[sl.Tag.ToString()];
                    arr[1] = (int)sl.Value;
                    fm.Settings[sl.Tag.ToString()] = arr;
                    val.Text = arr[1].ToString();
                    fm.SaveSettings();
                };
                //Slider on the left side, textbox on the right handside. Wrap it up in a dock.
                DockPanel.SetDock(val, Dock.Right);
                d.Children.Add(val);
                d.Children.Add(sl);
                //Add the elements to settings menu in the order accordingly
                SettingsMenu.Children.Add(t);
                SettingsMenu.Children.Add(d);
                //Continue to next setting
                count++;
            }
        }
        /// <summary>
        /// Open or closes Settings menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCloseSettings(object sender, RoutedEventArgs e)
        {
            if (SettingsMenu.Visibility == Visibility.Collapsed)
                ResetViews();
            SettingsMenu.Visibility = SettingsMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        /// <summary>
        /// Signs in and out of a Google Account to add or remove Google Calendar services
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>The method can be called in three cases. First case is when the user manually
        /// presses the GoogleSignIn button to sign in, second case is when the user presses
        /// the GoogleSignIn button to sign out, third case is when the method's being called on application's
        /// startup to automatically sign in to an account using an existing token.
        /// In first case, if the method fails to find an existing token,
        /// it will open up a page in the web browser with OAuth 2.0 Google authenctication</remarks>
        private async void SignInOutGoogleAccount(object sender, RoutedEventArgs e)
        {
            Button b = (Button)e.Source;
            //Sign out of account
            if (b != null && b.Tag.ToString().Equals("SignOut"))
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet"));
                    foreach (var subDir in dir.GetDirectories())
                        if (subDir.Name.Equals("t"+credPath.Split('t')[credPath.Split('t').Length-1]))
                        {
                            subDir.Attributes = FileAttributes.Normal;
                            subDir.Delete(true);
                        }
                }
                catch (Exception)
                {
                    //Could not find directory or file, so nothing left to do
                    return;
                }
                finally
                {
                    //Change back the GoogleSignIn button functionality to sign in mode
                    b.Tag = "SignIn";
                    GoogleBlock.Text = "Sign in with Google";
                }
            }
            //Sign in to account
            else
            {
                try
                {
                    //Retrieve Google OAuth2.0 secret keys
                    using (var stream = new FileStream(secretsPath, FileMode.Open, FileAccess.Read))
                    {
                        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = GoogleClientSecrets.FromStream(stream).Secrets,
                            Scopes = Scopes,
                            DataStore = new FileDataStore("Store")
                        });
                        string path = Directory.GetFiles(credPath)[0];
                        //Retrieve user credentials if they exist
                        using (var stream2 = new StreamReader(path))
                        {
                            string data = stream2.ReadToEnd();
                            JObject jtoken = JsonConvert.DeserializeObject<JObject>(data);
                            var token = new TokenResponse
                            {
                                AccessToken = jtoken["access_token"].ToString(),
                                RefreshToken = jtoken["refresh_token"].ToString()
                            };
                            credential = new UserCredential(flow, Environment.UserName, token);
                        }
                    }
                }
                //User credentials were not found
                catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException || ex is IndexOutOfRangeException)
                {
                    //Automatic sign in attempt --> No user found --> stop 
                    if (b == null)
                        return;
                    //User sign attempt --> Create new user --> log in to service
                    using (var stream = new FileStream(secretsPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true));
                    }
                }
                //Create the Google Calendar service
                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Optimeet Calendar",
                });
                //Switch the GoogleSignIn button to sign out mode
                GoogleSignInButton.Tag = "SignOut";
                GoogleBlock.Text = "Sign out of Google account";
                //Show message on user attempt
                if (b != null)
                    MessageBox.Show("Successfully loaded google account: " + credential.UserId);
            }

        }


        //...............................ABOUT BUTTON FUNCTIONALITY...............................

        /// <summary>
        /// Opens project's page in user's default web browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Excustic/Optimeet",
                UseShellExecute = true
            });
        }

        //...............................ADDITIONAL UI FUNCTIONALITY...............................

        /// <summary>
        /// Highlights a button in light gray colour
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Highlight(object sender, MouseEventArgs e)
        {
            Button s = (Button)e.Source;
            s.Background = Brushes.LightGray;
        }
        /// <summary>
        /// Unhighlights a button, making its background transparent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Unhighlight(object sender, MouseEventArgs e)
        {
            Button s = (Button)e.Source;
            s.Background = Brushes.Transparent;
        }
        /// <summary>
        /// Highlights a button in light blue colour.
        /// </summary>
        /// <param name="s"></param>
        private void Highlight(Button s)
        {
            s.Background = Brushes.LightBlue;
        }
        /// <summary>
        /// Unhiglights a button, making its background transparent
        /// </summary>
        /// <param name="s"></param>
        private void Unhighlight(Button s)
        {
            s.Background = Brushes.LightGray;
        }
        /// <summary>
        /// Closes application window and terminates the process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Maximizes the application window while not changing the scale of any elements apart from windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Minimizes the application window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Drags the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
