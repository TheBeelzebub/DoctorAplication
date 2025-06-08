using DoctorApp1.Converters;
using DoctorApp1.Services;
using Microsoft.Maui.Controls;
using System;

namespace DoctorApp1.Views
{
    public partial class NewPasswordPage : ContentPage
    {
        private readonly string _email;

        public NewPasswordPage(string email)
        {
            InitializeComponent();
            _email = email;
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            string newPassword = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            var user = App.Database.GetUserByEmail(_email);
            if (user == null)
            {
                await DisplayAlert("Error", "User not found.", "OK");
                return;
            }

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.TokenExpiration = null;
            App.Database.UpdateUser(user);

            await DisplayAlert("Success", "Password has been reset. You can now log in.", "OK");

            // Return to login screen
            await Navigation.PopToRootAsync();
        }
    }
}
