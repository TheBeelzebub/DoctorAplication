using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace DoctorApp1.Views
{
    public partial class CalendarPage : ContentPage
    {
        public ObservableCollection<Appointment> Appointments { get; set; } = new();
        public ObservableCollection<Patient> Patients { get; set; } = new();

        // For binding to selected date/month in XAML
        public DateTime SelectedDate { get; set; }
        public DateTime SelectedMonth { get; set; }
        public string ModalTitle { get; set; }

        // For modal editing
        Appointment editingAppointment = null;

        int currentYear;
        int currentMonth;

        public List<string> Months { get; } =
            System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList();

        public List<int> Years { get; } =
            Enumerable.Range(DateTime.Today.Year - 10, 21).ToList();

        public CalendarPage()
        {
            InitializeComponent();

            // Set ItemsSource for Pickers
            MonthPicker.ItemsSource = Months;
            YearPicker.ItemsSource = Years;

            // Set default selected values (today)
            MonthPicker.SelectedIndex = DateTime.Today.Month - 1;
            YearPicker.SelectedItem = DateTime.Today.Year;

            // Events
            MonthPicker.SelectedIndexChanged += OnMonthOrYearChanged;
            YearPicker.SelectedIndexChanged += OnMonthOrYearChanged;

            // Set initial SelectedMonth value and load calendar
            SelectedMonth = new DateTime((int)YearPicker.SelectedItem, MonthPicker.SelectedIndex + 1, 1);
            LoadCalendar(SelectedMonth.Year, SelectedMonth.Month);
        }

        void OnMonthOrYearChanged(object sender, EventArgs e)
        {
            if (MonthPicker.SelectedIndex == -1 || YearPicker.SelectedItem == null) return;
            int month = MonthPicker.SelectedIndex + 1;
            int year = (int)YearPicker.SelectedItem;
            SelectedMonth = new DateTime(year, month, 1);
            LoadCalendar(year, month);
        }

        void LoadCalendar(int year, int month)
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            // 7 columns for days
            for (int i = 0; i < 7; i++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            // 1 row for day names, 6 for weeks
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Day names
            for (int r = 0; r < 6; r++)
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) }); // weeks

            // Add day-of-week labels to row 0
            string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            for (int i = 0; i < 7; i++)
            {
                var dayLabel = new Label
                {
                    Text = days[i],
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Colors.White
                };
                CalendarGrid.Add(dayLabel, i, 0);
            }

            var firstDay = new DateTime(year, month, 1);
            int startDayOfWeek = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            int row = 1;
            int col = startDayOfWeek;

            // Fill empty cells before the first day
            for (int i = 0; i < startDayOfWeek; i++)
            {
                CalendarGrid.Add(new BoxView { BackgroundColor = Colors.Transparent }, i, 1);
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayAppointments = Appointments.Where(a => a.StartTime.Date == date.Date).ToList();

                // --- APPOINTMENT LISTVIEW WITH GRID TEMPLATE ---
                var listView = new ListView
                {
                    ItemsSource = dayAppointments,
                    HasUnevenRows = true,
                    SeparatorVisibility = SeparatorVisibility.None,
                    ItemTemplate = new DataTemplate(() =>
                    {
                        var grid = new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = GridLength.Star },
                                new ColumnDefinition { Width = GridLength.Auto },
                                new ColumnDefinition { Width = GridLength.Auto }
                            },
                            VerticalOptions = LayoutOptions.Center
                        };

                        var stack = new StackLayout();
                        var timeLabel = new Label { FontAttributes = FontAttributes.Bold };
                        timeLabel.SetBinding(Label.TextProperty, new Binding("StartTime", stringFormat: "{0:HH:mm}"));
                        var notesLabel = new Label();
                        notesLabel.SetBinding(Label.TextProperty, "Notes");
                        var patientIdLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
                        patientIdLabel.SetBinding(Label.TextProperty, new Binding("PatientID", stringFormat: "Patient ID: {0}"));

                        stack.Children.Add(timeLabel);
                        stack.Children.Add(notesLabel);
                        stack.Children.Add(patientIdLabel);

                        grid.Add(stack, 0, 0);

                        var editButton = new Button { Text = "Edit", Padding = new Thickness(8, 0) };
                        editButton.SetBinding(Button.CommandParameterProperty, ".");
                        editButton.Clicked += OnEditAppointmentClicked;
                        grid.Add(editButton, 1, 0);

                        var deleteButton = new Button { Text = "Delete", Padding = new Thickness(8, 0), TextColor = Colors.Red };
                        deleteButton.SetBinding(Button.CommandParameterProperty, ".");
                        deleteButton.Clicked += OnDeleteAppointmentClicked;
                        grid.Add(deleteButton, 2, 0);

                        return new ViewCell { View = grid };
                    })
                };
                // --- END APPOINTMENT LISTVIEW ---

                // Add a border and a day label
                var border = new Border
                {
                    Stroke = Colors.Gray,
                    Padding = 2,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 2,
                        Children =
                        {
                            new Label
                            {
                                Text = day.ToString(),
                                FontSize = 10,
                                HorizontalTextAlignment = TextAlignment.Center
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

        // This method can be expanded to show appointments for the selected day, etc.
        void OnDayTapped(int year, int month, int day)
        {
            SelectedDate = new DateTime(year, month, day);
            LoadCalendar(year, month);
            // You could also display only appointments for SelectedDate
        }

        void OnDayViewClicked(object sender, EventArgs e)
        {
            // TODO: Implement day view switching logic
        }

        void OnWeekViewClicked(object sender, EventArgs e)
        {
            // TODO: Implement week view switching logic
        }

        void OnAddAppointmentClicked(object sender, EventArgs e)
        {
            AppointmentModal.IsVisible = true;
            ModalTitle = "Add Appointment";
            PatientPicker.ItemsSource = Patients.Select(p => p.Name).ToList();
            PatientPicker.SelectedIndex = 0;
            StartTimePicker.Time = TimeSpan.FromHours(9);
            EndTimePicker.Time = TimeSpan.FromHours(10);
            NotesEditor.Text = "";
            editingAppointment = null;
        }

        void OnEditAppointmentClicked(object sender, EventArgs e)
        {
            // Example - assume CommandParameter is set to the appointment
            if (sender is Button btn && btn.CommandParameter is Appointment appt)
            {
                AppointmentModal.IsVisible = true;
                ModalTitle = "Edit Appointment";
                PatientPicker.ItemsSource = Patients.Select(p => p.Name).ToList();
                PatientPicker.SelectedIndex = Patients.IndexOf(Patients.FirstOrDefault(p => p.Id == appt.PatientID));
                StartTimePicker.Time = appt.StartTime.TimeOfDay;
                EndTimePicker.Time = appt.EndTime.TimeOfDay;
                NotesEditor.Text = appt.Notes;
                editingAppointment = appt;
            }
        }

        void OnDeleteAppointmentClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Appointment appt)
            {
                Appointments.Remove(appt);
                LoadCalendar(currentYear, currentMonth);
            }
        }

        void OnCancelModal(object sender, EventArgs e)
        {
            AppointmentModal.IsVisible = false;
        }

        void OnSaveModal(object sender, EventArgs e)
        {
            string patientName = PatientPicker.SelectedItem as string;
            var patient = Patients.FirstOrDefault(p => p.Name == patientName);
            if (patient == null)
            {
                DisplayAlert("Error", "Please select a patient.", "OK");
                return;
            }

            var startDateTime = SelectedDate.Date.Add(StartTimePicker.Time);
            var endDateTime = SelectedDate.Date.Add(EndTimePicker.Time);

            if (editingAppointment == null)
            {
                var newAppt = new Appointment
                {
                    Id = Appointments.Count > 0 ? Appointments.Max(a => a.Id) + 1 : 1,
                    PatientID = patient.Id,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    Notes = NotesEditor.Text ?? ""
                };
                Appointments.Add(newAppt);
            }
            else
            {
                editingAppointment.PatientID = patient.Id;
                editingAppointment.StartTime = startDateTime;
                editingAppointment.EndTime = endDateTime;
                editingAppointment.Notes = NotesEditor.Text ?? "";
            }

            AppointmentModal.IsVisible = false;
            LoadCalendar(currentYear, currentMonth);
        }

        // If you want to show appointments for the selected day (for a CollectionView, etc)
        public ObservableCollection<Appointment> AppointmentsForSelectedDate =>
            new(Appointments.Where(a => a.StartTime.Date == SelectedDate.Date));

        // You may want to implement INotifyPropertyChanged in a full MVVM setup for property updates
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int PatientID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Notes { get; set; }
    }

    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}