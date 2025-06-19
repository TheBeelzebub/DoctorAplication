using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using DoctorApp1.Models;
using System.Collections.Generic;

namespace DoctorApp1.Views
{
    public partial class CalendarPage : ContentPage
    {
        public ObservableCollection<Appointment> Appointments { get; set; } = new();
        public ObservableCollection<Patient> Patients { get; set; } = new();

        public DateTime SelectedDate { get; set; }
        public DateTime SelectedMonth { get; set; }
        public string ModalTitle { get; set; }

        public enum CalendarMode { Day, Week }
        private CalendarMode currentMode = CalendarMode.Day;

        Appointment editingAppointment = null;

        Button DeleteButton;

        public List<string> Months { get; } =
            System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList();

        public List<int> Years { get; } =
            Enumerable.Range(DateTime.Today.Year - 10, 21).ToList();

        public CalendarPage()
        {
            InitializeComponent();

            MonthPicker.ItemsSource = Months;
            YearPicker.ItemsSource = Years;

            MonthPicker.SelectedIndex = DateTime.Today.Month - 1;
            YearPicker.SelectedItem = DateTime.Today.Year;

            MonthPicker.SelectedIndexChanged += OnMonthOrYearChanged;
            YearPicker.SelectedIndexChanged += OnMonthOrYearChanged;

            SelectedMonth = new DateTime((int)YearPicker.SelectedItem, MonthPicker.SelectedIndex + 1, 1);
            ReloadAppointments();
            DayRadio.IsChecked = true;

            AddDeleteButtonToModal();
        }

        void AddDeleteButtonToModal()
        {
            var border = (Border)AppointmentModal.Content;
            var stack = (StackLayout)border.Content;
            var footer = (StackLayout)stack.Children.Last();

            footer.Orientation = StackOrientation.Horizontal;

            DeleteButton = new Button
            {
                Text = "Delete",
                BackgroundColor = Colors.Red,
                TextColor = Colors.White,
                IsVisible = false,
                Margin = new Thickness(0, 0, 10, 0),
                HorizontalOptions = LayoutOptions.Start
            };
            DeleteButton.Clicked += OnDeleteModal;

            footer.Children.Insert(0, DeleteButton);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReloadAppointments();

            if (currentMode == CalendarMode.Day)
                LoadCalendar(SelectedMonth.Year, SelectedMonth.Month);
            else
                LoadCalendarWeeks(SelectedMonth.Year, SelectedMonth.Month);
        }

        void ReloadAppointments()
        {
            Appointments.Clear();
            foreach (var appt in App.Database.GetAppointments())
                Appointments.Add(appt);
        }

        void OnMonthOrYearChanged(object sender, EventArgs e)
        {
            if (MonthPicker.SelectedIndex == -1 || YearPicker.SelectedItem == null) return;
            int month = MonthPicker.SelectedIndex + 1;
            int year = (int)YearPicker.SelectedItem;
            SelectedMonth = new DateTime(year, month, 1);

            if (currentMode == CalendarMode.Day)
                LoadCalendar(year, month);
            else
                LoadCalendarWeeks(year, month);
        }

        void LoadCalendar(int year, int month)
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            CalendarFrame.WidthRequest = 800;
            CalendarGrid.WidthRequest = 800;
            CalendarUI.WidthRequest = CalendarFrame.WidthRequest;

            for (int i = 0; i < 7; i++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var firstDay = new DateTime(year, month, 1);
            int startDayOfWeek = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int totalCells = startDayOfWeek + daysInMonth;
            int weekRows = (int)Math.Ceiling(totalCells / 7.0);

            for (int r = 0; r < weekRows; r++)
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

            CalendarGrid.HeightRequest = (weekRows - 1) * 100 + 42;
            if (weekRows == 6)
                CalendarGrid.HeightRequest = CalendarGrid.HeightRequest - 19;
            CalendarFrame.HeightRequest = CalendarGrid.HeightRequest;

            string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            for (int i = 0; i < 7; i++)
            {
                var dayLabel = new Label
                {
                    Text = days[i],
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Color.FromArgb("#EEF2F7"),
                    FontSize = 14,
                    TextColor = Color.FromArgb("#334")
                };
                CalendarGrid.Add(dayLabel, i, 0);
            }

            int row = 1;
            int col = startDayOfWeek;

            for (int i = 0; i < startDayOfWeek; i++)
            {
                CalendarGrid.Add(new BoxView { BackgroundColor = Colors.Transparent }, i, 1);
            }

            int maxVisibleRows = 2;
            int rowHeight = 30;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayAppointments = Appointments
                    .Where(a => a.StartTime.Date == date.Date)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                var listView = new ListView
                {
                    ItemsSource = dayAppointments,
                    HasUnevenRows = false,
                    RowHeight = rowHeight,
                    SeparatorVisibility = SeparatorVisibility.None,
                    Margin = new Thickness(0, 2, 0, 0),
                    BackgroundColor = Colors.Transparent,
                    HeightRequest = Math.Min(dayAppointments.Count, maxVisibleRows) * rowHeight - 2,
                    VerticalOptions = LayoutOptions.Fill,
                    ItemTemplate = new DataTemplate(() =>
                    {
                        var stack = new StackLayout { Spacing = 1, Padding = new Thickness(1), Orientation = StackOrientation.Vertical };
                        var timeLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 10, TextColor = Color.FromArgb("#222") };
                        timeLabel.SetBinding(Label.TextProperty, new MultiBinding
                        {
                            Bindings = {
                                new Binding("StartTime"),
                                new Binding("EndTime")
                            },
                            StringFormat = "{0:HH:mm} - {1:HH:mm}"
                        });

                        var patientNameLabel = new Label { FontSize = 9, TextColor = Color.FromArgb("#444") };
                        patientNameLabel.SetBinding(Label.TextProperty, new Binding("PatientID", converter: new PatientNameConverter(this)));

                        stack.Children.Add(timeLabel);
                        stack.Children.Add(patientNameLabel);

                        return new ViewCell { View = stack };
                    })
                };

                listView.ItemTapped += OnAppointmentTapped;

                var border = new Border
                {
                    Stroke = Colors.LightGray,
                    Background = Color.FromArgb("#FFF"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 10 },
                    Padding = 3,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 2,
                        Children =
                        {
                            new Label
                            {
                                Text = day.ToString(),
                                FontSize = 10,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#4D90FE"),
                                Margin = new Thickness(0,0,0,0)
                            },
                            listView
                        }
                    }
                };

                CalendarGrid.Add(border, col, row);

                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }
            }
        }

        void LoadCalendarWeeks(int year, int month)
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();
            CalendarGrid.HeightRequest = 440;
            CalendarFrame.HeightRequest = 440;

            int weekColumnWidth = 150;
            int gridHeight = 400;
            int rowHeight = gridHeight / 7;

            var dayNames = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 30,
                Children =
        {
            new Label { Text = "Sun", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Mon", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Tue", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Wed", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Thu", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Fri", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Sat", WidthRequest = weekColumnWidth, HorizontalTextAlignment = TextAlignment.Center },
        }
            };

            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            int numberOfWeeks = GetNumberOfWeeks(year, month);

            for (int week = 0; week < numberOfWeeks; week++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = weekColumnWidth });

            CalendarGrid.WidthRequest = weekColumnWidth * numberOfWeeks + 22;
            if (CalendarFrame != null)
                CalendarFrame.WidthRequest = weekColumnWidth * numberOfWeeks + 24;
            CalendarUI.WidthRequest = CalendarFrame.WidthRequest;

            var firstDay = new DateTime(year, month, 1);
            int startDayOfWeek = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int daysBefore = startDayOfWeek;
            int totalCells = daysBefore + daysInMonth;

            for (int week = 0; week < numberOfWeeks; week++)
            {
                int cellStart = week * 7;
                int weekFirstDay = Math.Max(1, cellStart - daysBefore + 1);
                int weekLastDay = Math.Min(daysInMonth, (cellStart + 6) - daysBefore + 1);

                int daysThisWeek = weekLastDay - weekFirstDay + 1;
                int boxTopOffset = (weekFirstDay + daysBefore - 1) % 7 * rowHeight;
                int boxHeight = daysThisWeek * rowHeight;

                var weekAppointments = Appointments
                    .Where(a => a.StartTime.Day >= weekFirstDay && a.StartTime.Day <= weekLastDay
                                && a.StartTime.Month == month && a.StartTime.Year == year)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                var listView = new ListView
                {
                    ItemsSource = weekAppointments,
                    HasUnevenRows = false,
                    RowHeight = 38,
                    SeparatorVisibility = SeparatorVisibility.None,
                    BackgroundColor = Color.FromArgb("#FFF"),
                    VerticalOptions = LayoutOptions.Start,
                    HeightRequest = boxHeight,
                    ItemTemplate = new DataTemplate(() =>
                    {
                        var stack = new StackLayout { Spacing = 2, Padding = new Thickness(1), Orientation = StackOrientation.Vertical };
                        var timeLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 13, TextColor = Color.FromArgb("#222") };
                        timeLabel.SetBinding(Label.TextProperty, new MultiBinding
                        {
                            Bindings = {
                        new Binding("StartTime"),
                        new Binding("EndTime")
                    },
                            StringFormat = "{0:dd MMM HH:mm} - {1:HH:mm}"
                        });

                        var patientNameLabel = new Label { FontSize = 12, TextColor = Color.FromArgb("#444") };
                        patientNameLabel.SetBinding(Label.TextProperty, new Binding("PatientID", converter: new PatientNameConverter(this)));

                        stack.Children.Add(timeLabel);
                        stack.Children.Add(patientNameLabel);

                        return new ViewCell { View = stack };
                    })
                };

                listView.ItemTapped += OnAppointmentTapped;

                var weekStack = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
            {
                new Label
                {
                    Text = $"Week {week + 1}",
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromArgb("#4D90FE"),
                    Margin = new Thickness(0,0,0,4)
                },
                listView
            }
                };

                var border = new Border
                {
                    Stroke = Colors.LightGray,
                    Background = Color.FromArgb("#FFF"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 10 },
                    Padding = 3,
                    VerticalOptions = LayoutOptions.Start,
                    Content = weekStack,
                    TranslationY = boxTopOffset
                };

                CalendarGrid.Add(border, week, 0);
            }

            int GetNumberOfWeeks(int y, int m)
            {
                var fd = new DateTime(y, m, 1);
                int sd = (int)fd.DayOfWeek;
                int d = DateTime.DaysInMonth(y, m);
                int tot = sd + d;
                return (int)Math.Ceiling(tot / 7.0);
            }
        }

        int GetNumberOfWeeks(int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            int startDay = (int)firstDay.DayOfWeek;
            int days = DateTime.DaysInMonth(year, month);
            int total = startDay + days;
            return (int)Math.Ceiling(total / 7.0);
        }

        class PatientNameConverter : IValueConverter
        {
            private readonly CalendarPage _page;
            public PatientNameConverter(CalendarPage page) => _page = page;

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is int patientId)
                {
                    var patient = _page.Patients.FirstOrDefault(p => p.PatientID == patientId)
                               ?? App.Database.GetPatients().FirstOrDefault(p => p.PatientID == patientId);
                    return patient != null ? patient.FullName : $"Patient {patientId}";
                }
                return "";
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
        }

        void OnDayTapped(int year, int month, int day)
        {
            SelectedDate = new DateTime(year, month, day);
            LoadCalendar(year, month);
        }

        void OnDayViewClicked(object sender, EventArgs e)
        {
            currentMode = CalendarMode.Day;

            ReloadAppointments();
            LoadCalendar(SelectedMonth.Year, SelectedMonth.Month);
        }

        void OnWeekViewClicked(object sender, EventArgs e)
        {
            currentMode = CalendarMode.Week;

            ReloadAppointments();
            LoadCalendarWeeks(SelectedMonth.Year, SelectedMonth.Month);
        }

        void OnAddAppointmentClicked(object sender, EventArgs e)
        {
            PatientPicker.ItemsSource = App.Database.GetPatients();
            PatientPicker.ItemDisplayBinding = new Binding("FullName");
            PatientPicker.SelectedIndex = 0;

            AppointmentModal.IsVisible = true;
            ModalTitle = "Add Appointment";
            StartTimePicker.Time = TimeSpan.FromHours(9);
            EndTimePicker.Time = TimeSpan.FromHours(10);
            NotesEditor.Text = "";
            editingAppointment = null;
            DeleteButton.IsVisible = false;
        }

        void OnAppointmentTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Appointment tappedAppointment)
            {
                editingAppointment = tappedAppointment;

                PatientPicker.ItemsSource = App.Database.GetPatients();
                PatientPicker.ItemDisplayBinding = new Binding("FullName");
                var patientList = (IEnumerable<Patient>)PatientPicker.ItemsSource;
                var patient = patientList.FirstOrDefault(p => p.PatientID == tappedAppointment.PatientID);
                PatientPicker.SelectedItem = patient;

                DatePicker.Date = tappedAppointment.StartTime.Date;
                StartTimePicker.Time = tappedAppointment.StartTime.TimeOfDay;
                EndTimePicker.Time = tappedAppointment.EndTime.TimeOfDay;
                NotesEditor.Text = tappedAppointment.Notes;

                ModalTitle = "Edit Appointment";
                AppointmentModal.IsVisible = true;
                DeleteButton.IsVisible = true;

                if (sender is ListView lv)
                    lv.SelectedItem = null;
            }
        }

        void OnCancelModal(object sender, EventArgs e)
        {
            AppointmentModal.IsVisible = false;
        }

        void OnSaveModal(object sender, EventArgs e)
        {
            var selectedPatient = PatientPicker.SelectedItem as Patient;
            if (selectedPatient == null)
            {
                DisplayAlert("Error", "Please select a patient.", "OK");
                return;
            }

            if (editingAppointment == null)
            {
                var appointment = new Appointment
                {
                    PatientID = selectedPatient.PatientID,
                    StartTime = DatePicker.Date.Add(StartTimePicker.Time),
                    EndTime = DatePicker.Date.Add(EndTimePicker.Time),
                    Notes = NotesEditor.Text
                };

                int newAppointmentId = App.Database.AddAppointment(appointment);
                appointment.AppointmentID = newAppointmentId;

                // Schedule notification
#if WINDOWS
                var notificationId = $"appt_{appointment.AppointmentID}";
                DoctorApp1.Platforms.Windows.NotificationService.ScheduleToast(
                    notificationId,
                    "Appointment Reminder",
                    $"You have an appointment with {selectedPatient.FullName} at {appointment.StartTime:HH:mm}.",
                    appointment.StartTime.AddMinutes(-10)); // Notify 10 mins before
#endif
            }
            else
            {
                editingAppointment.PatientID = selectedPatient.PatientID;
                editingAppointment.StartTime = DatePicker.Date.Add(StartTimePicker.Time);
                editingAppointment.EndTime = DatePicker.Date.Add(EndTimePicker.Time);
                editingAppointment.Notes = NotesEditor.Text;

                App.Database.UpdateAppointment(editingAppointment);

                // Update notification
#if WINDOWS
                var notificationId = $"appt_{editingAppointment.AppointmentID}";
                DoctorApp1.Platforms.Windows.NotificationService.ScheduleToast(
                    notificationId,
                    "Appointment Reminder",
                    $"You have an appointment with {selectedPatient.FullName} at {editingAppointment.StartTime:HH:mm}.",
                    editingAppointment.StartTime.AddMinutes(-10));
#endif
            }

            ReloadAppointments();
            AppointmentModal.IsVisible = false;

            if (currentMode == CalendarMode.Day)
                LoadCalendar(SelectedMonth.Year, SelectedMonth.Month);
            else
                LoadCalendarWeeks(SelectedMonth.Year, SelectedMonth.Month);
        }

        void OnDeleteModal(object sender, EventArgs e)
        {
            if (editingAppointment != null)
            {
                App.Database.DeleteAppointment(editingAppointment);

                // Remove notification
#if WINDOWS
                var notificationId = $"appt_{editingAppointment.AppointmentID}";
                DoctorApp1.Platforms.Windows.NotificationService.RemoveScheduledToast(notificationId);
#endif

                ReloadAppointments();
                AppointmentModal.IsVisible = false;

                if (currentMode == CalendarMode.Day)
                    LoadCalendar(SelectedMonth.Year, SelectedMonth.Month);
                else
                    LoadCalendarWeeks(SelectedMonth.Year, SelectedMonth.Month);
            }
        }

        private void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked)
            {
                string selectedMode = radioButton.Content.ToString();

                if (selectedMode == "Day")
                {
                    currentMode = CalendarMode.Day;
                    LoadCalendar(SelectedMonth.Year, SelectedMonth.Month);
                }
                else if (selectedMode == "Week")
                {
                    currentMode = CalendarMode.Week;
                    LoadCalendarWeeks(SelectedMonth.Year, SelectedMonth.Month);
                }
            }
        }

        public ObservableCollection<Appointment> AppointmentsForSelectedDate =>
            new(Appointments.Where(a => a.StartTime.Date == SelectedDate.Date));
    }
}