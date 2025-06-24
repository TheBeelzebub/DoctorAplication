using DoctorApp1.Models;
using DoctorApp1.Services;
using DoctorApp1.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoctorApp1
{
    public partial class MainPage : ContentPage
    {
        public List<Patient> Patients { get; set; } = new List<Patient>();
        public List<Appointment> MissedAppointments { get; set; } = new List<Appointment>();

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

            notificationService.MarkMissedNotifications();

            var missedAppointments = notificationService.GetMissedAppointments();

            if (missedAppointments.Any())
            {
                ShowMissedNotificationsPopup(missedAppointments);
            }
        }

        private void ShowMissedNotificationsPopup(List<Appointment> missedAppointments)
        {
            // 🔧 Save to global field so it can be cleared later
            MissedAppointments = missedAppointments;

            MissedAppointmentsStack.Children.Clear();

            foreach (var appt in missedAppointments)
            {
                var patient = App.Database.GetPatients().FirstOrDefault(p => p.PatientID == appt.PatientID);
                string fullName = patient?.FullName ?? "Unknown";

                var label = new Label
                {
                    Text = $"Appointment with {fullName} at {appt.StartTime:HH:mm, MMM dd}",
                    FontSize = 16,
                    Margin = new Thickness(0, 5)
                };

                MissedAppointmentsStack.Children.Add(label);
            }

            MissedNotificationsPopup.IsVisible = true;
        }

        private void OnCloseMissedPopupClicked(object sender, EventArgs e)
        {
            foreach (var appt in MissedAppointments)
            {
                notificationService.ClearMissedNotification(appt.AppointmentID);
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
                var appointment = MissedAppointments[0];
                notificationService.ClearMissedNotification(appointment.AppointmentID);
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
