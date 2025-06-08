using DoctorApp1.Models;
using DoctorApp1.Services;
using DoctorApp1.Views; // Added this to resolve the LoginPage namespace issue
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;

namespace DoctorApp1
{
    public partial class MainPage : ContentPage
    {
        public List<Patient> Patients { get; set; } = new List<Patient>(); // Initialize to avoid nullability warnings
        private string _loggedInEmail = string.Empty; // Initialize to avoid nullability warnings
        private DoctorUser _currentUser; // Store the logged-in user

        // 🔹 Constructor for login use
        public MainPage(string email)
        {
            InitializeComponent();
            _loggedInEmail = email.Trim().ToLower();

            // Retrieve the current user from the database
            _currentUser = App.Database.GetUserByEmail(_loggedInEmail);

            BindingContext = this;
            LoadPatients();
        }

        // 🔹 Default constructor
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
            await Navigation.PushAsync(new CalendarPage());
        }

        // 🔐 Logout functionality
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
                    Application.Current.Windows[0].Page = new NavigationPage(new LoginPage()); // Resolved namespace issue
                }
            });
        }

        // 🔹 Navigate to User Dashboard
        private async void OnUserDashboardClicked(object sender, EventArgs e)
        {
            // Fix: If _currentUser is null, try to reload from the database using _loggedInEmail
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
}




