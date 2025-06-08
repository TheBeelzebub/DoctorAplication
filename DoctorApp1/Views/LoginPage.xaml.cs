using System;
using DoctorApp1.Models;
using DoctorApp1.Services;
using DoctorApp1.Converters;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DoctorApp1.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Auto-login if remembered
            if (Preferences.ContainsKey("LoggedInEmail"))
            {
                string? email = Preferences.Get("LoggedInEmail", null); // Use nullable type
                if (!string.IsNullOrEmpty(email))
                {
                    // Replace the root page with AppShell
                    SetRootPage(new AppShell());
                }
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string? email = EmailEntry.Text?.Trim();
            string? password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Login Failed", "Email and password are required.", "OK");
                return;
            }

            var user = App.Database.GetUserByEmail(email);

            if (user == null || PasswordHelper.HashPassword(password) != user.PasswordHash)
            {
                await DisplayAlert("Login Failed", "Incorrect email or password.", "OK");
                return;
            }

            if (RememberMeCheckBox.IsChecked)
            {
                Preferences.Set("LoggedInEmail", email);
            }

            // Successful login
            await Navigation.PushAsync(new MainPage(email.Trim().ToLower()));

        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ResetPasswordPage());
        }

        /// <summary>
        /// Replaces the root page of the application with the specified page.
        /// </summary>
        /// <param name="page">The new root page.</param>
        private void SetRootPage(Page page)
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = page; // Use Windows[0].Page to set the root page
                }
            });
        }
    }
}
