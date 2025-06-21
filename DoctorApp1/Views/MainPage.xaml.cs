using DoctorApp1.Models;
using DoctorApp1.Services;
using DoctorApp1.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


namespace DoctorApp1
{
    public partial class MainPage : ContentPage
    {
        public List<Models.Patient> Patients { get; set; } = new List<Models.Patient>();
        private string _loggedInEmail = string.Empty;
        private DoctorUser _currentUser;

        // Constructor for login use
        public MainPage(string email)
        {
            InitializeComponent();
            _loggedInEmail = email.Trim().ToLower();

            // Retrieve the current user from the database
            _currentUser = App.Database.GetUserByEmail(_loggedInEmail);

            BindingContext = this;
            LoadPatients();
        }

        // Default constructor
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadPatients();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPatients();
        }

        private void LoadPatients()
        {
            var allPatients = App.Database.GetPatients();
            // Patients = allPatients.Where(p => p.DoctorEmail == _loggedInEmail).ToList(); // If filtering by doctor
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
            if (e.SelectedItem is Models.Patient selectedPatient)
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

            // Remove the saved login email
            Preferences.Remove("LoggedInEmail");

            // Replace the root page with the login page
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