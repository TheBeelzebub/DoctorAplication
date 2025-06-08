using DoctorApp1.Models;
using Microsoft.Maui.Controls;
using DoctorApp1.Services;
using DoctorApp1.Converters;
using System;

namespace DoctorApp1.Views
{
    public partial class UserDashboardPage : ContentPage
    {
        private readonly DoctorUser _currentUser;

        public UserDashboardPage(DoctorUser currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser; // Pass the logged-in user to the dashboard
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            string currentPassword = CurrentPasswordEntry.Text?.Trim();
            string newPassword = NewPasswordEntry.Text?.Trim();
            string confirmNewPassword = ConfirmNewPasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                await DisplayAlert("Error", "All fields are required.", "OK");
                return;
            }

            if (PasswordHelper.HashPassword(currentPassword) != _currentUser.PasswordHash)
            {
                await DisplayAlert("Error", "Current password is incorrect.", "OK");
                return;
            }

            if (newPassword != confirmNewPassword)
            {
                await DisplayAlert("Error", "New passwords do not match.", "OK");
                return;
            }

            // Update the password in the database
            _currentUser.PasswordHash = PasswordHelper.HashPassword(newPassword);
            App.Database.UpdateUser(_currentUser);

            await DisplayAlert("Success", "Password changed successfully.", "OK");
        }

        private async void OnDeleteAccountClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Delete Account", "Are you sure you want to delete your account? This action cannot be undone.", "Yes", "No");
            if (!confirm)
                return;

            // Delete the user from the database
            App.Database.DeleteUser(_currentUser);

            await DisplayAlert("Account Deleted", "Your account has been deleted.", "OK");

            // Log the user out and navigate to the login page
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new NavigationPage(new LoginPage());
                }
            });
        }
    }
}
