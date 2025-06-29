using DoctorApp1.Models;
using DoctorApp1.Services;
using DoctorApp1.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DoctorApp1
{
    public partial class MainPage : ContentPage
    {
        private bool _hasShownTodayAppointmentsPopup = false;
        public List<Patient> Patients { get; set; } = new List<Patient>();

        // Changed from List<Appointment> to ObservableCollection<string> to hold display strings
        public ObservableCollection<string> MissedAppointments { get; set; } = new ObservableCollection<string>();

        private string _loggedInEmail = string.Empty;
        private DoctorUser _currentUser;

        private readonly AppointmentNotificationService notificationService;

        // Constructor for login use, now accepting notification service
        public MainPage(string email, AppointmentNotificationService notificationService)
        {
            InitializeComponent();
            _loggedInEmail = email.Trim().ToLower();

            _currentUser = App.Database.GetUserByEmail(_loggedInEmail);

            this.notificationService = notificationService;

            BindingContext = this;
            LoadPatients();
        }

        // Default constructor with notification service injection
        public MainPage(AppointmentNotificationService notificationService)
        {
            InitializeComponent();

            this.notificationService = notificationService;

            BindingContext = this;
            LoadPatients();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            LoadPatients();

            if (_hasShownTodayAppointmentsPopup)
                return;

            _hasShownTodayAppointmentsPopup = true; // mark it as shown

            var upcomingToday = GetTodaysUpcomingAppointments();

            if (upcomingToday.Any())
            {
                ShowTodaysAppointmentsPopup(upcomingToday);
            }
            else
            {
                ShowNoAppointmentsPopup();
            }
        }

        private void ShowNoAppointmentsPopup()
        {
            MissedAppointments.Clear();
            MissedAppointments.Add("You have no upcoming appointments today.");
            MissedNotificationsPopup.IsVisible = true;
        }

        private void ShowTodaysAppointmentsPopup(List<Appointment> appointments)
        {
            MissedAppointments.Clear();

            foreach (var appt in appointments)
            {
                var patient = App.Database.GetPatients().FirstOrDefault(p => p.PatientID == appt.PatientID);
                string fullName = patient?.FullName ?? "Unknown";

                MissedAppointments.Add($"Appointment with {fullName} at {appt.StartTime:HH:mm}");
            }

            MissedNotificationsPopup.IsVisible = true;
        }

        private List<Appointment> GetTodaysUpcomingAppointments()
        {
            var now = DateTime.Now;
            var appointments = App.Database.GetAppointments();

            return appointments
                .Where(a =>
                    a.StartTime.Date == now.Date &&
                    a.StartTime > now)
                .OrderBy(a => a.StartTime)
                .ToList();
        }

        private void ShowMissedNotificationsPopup(List<Appointment> missedAppointments)
        {
            MissedAppointments.Clear();

            foreach (var appt in missedAppointments)
            {
                var patient = App.Database.GetPatients().FirstOrDefault(p => p.PatientID == appt.PatientID);
                string fullName = patient?.FullName ?? "Unknown";

                MissedAppointments.Add($"Appointment with {fullName} at {appt.StartTime:HH:mm, MMM dd}");
            }

            MissedNotificationsPopup.IsVisible = true;
        }

        private void OnCloseMissedPopupClicked(object sender, EventArgs e)
        {
            foreach (var displayText in MissedAppointments)
            {

            }

            MissedAppointments.Clear();
            MissedNotificationsPopup.IsVisible = false;
        }

        private void LoadPatients()
        {
            var allPatients = App.Database.GetPatients();
            Patients = allPatients;
            OnPropertyChanged(nameof(Patients));
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue;
            Patients = App.Database.GetPatients()
                                   .Where(p => p.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                              p.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                   .ToList();
            OnPropertyChanged(nameof(Patients));
        }

        private async void OnPatientSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Patient selectedPatient)
            {
                await Navigation.PushAsync(new PatientDetailPage(selectedPatient.PatientID));
                ((ListView)sender).SelectedItem = null;
            }
        }

        private async void OnAddPatientClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddPatientPage());
        }

        private async void OnOpenCalendarClicked(object sender, EventArgs e)
        {
            var serviceProvider = Application.Current.Handler?.MauiContext?.Services;
            var calendarPage = serviceProvider?.GetService<CalendarPage>();

            if (calendarPage != null)
            {
                await Navigation.PushAsync(calendarPage);
            }
            else
            {
                await DisplayAlert("Error", "Unable to open Calendar page.", "OK");
            }
        }

        // Logout functionality
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "Cancel");
            if (!confirm)
                return;

            Preferences.Remove("LoggedInEmail");

            Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new NavigationPage(new LoginPage());
                }
            });
        }

        // Navigate to User Dashboard
        private async void OnUserDashboardClicked(object sender, EventArgs e)
        {
            if (_currentUser == null && !string.IsNullOrEmpty(_loggedInEmail))
            {
                _currentUser = App.Database.GetUserByEmail(_loggedInEmail);
            }
            if (_currentUser != null)
            {
                await Navigation.PushAsync(new UserDashboardPage(_currentUser));
            }
            else
            {
                await DisplayAlert("Error", "Unable to load user data. Please log in again.", "OK");
            }
        }

        // Clear first missed appointment notification (example handler)
        private void OnClearMissedAppointmentClicked(object sender, EventArgs e)
        {
            if (MissedAppointments.Count > 0)
            {
                MissedAppointments.RemoveAt(0);
                OnPropertyChanged(nameof(MissedAppointments));
            }
        }
    }

    public static class ViewExtensions
    {
        public static Task WidthRequestTo(this VisualElement view, double width, uint length = 250, Easing easing = null)
        {
            var tcs = new TaskCompletionSource<bool>();
            new Animation(v => view.WidthRequest = v, view.WidthRequest, width)
                .Commit(view, "WidthRequestTo", 16, length, easing ?? Easing.Linear, (v, c) => tcs.SetResult(true));
            return tcs.Task;
        }
    }
}
