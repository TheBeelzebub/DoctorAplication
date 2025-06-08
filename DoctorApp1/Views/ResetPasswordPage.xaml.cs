using System;
using Microsoft.Maui.Controls;
using DoctorApp1.Services;

namespace DoctorApp1.Views
{
    public partial class ResetPasswordPage : ContentPage
    {
        public ResetPasswordPage()
        {
            InitializeComponent();
        }

        private async void OnSendTokenClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Error", "Please enter your email.", "OK");
                return;
            }

            var user = App.Database.GetUserByEmail(email);
            if (user == null)
            {
                await DisplayAlert("Error", "No account found with this email.", "OK");
                return;
            }

            string token = Guid.NewGuid().ToString(); // Generate a unique token
            user.PasswordResetToken = token;
            user.TokenExpiration = DateTime.Now.AddHours(1); // Token valid for 1 hour
            App.Database.UpdateUser(user);

            var emailService = new EmailService();
            string resetLink = $"https://your-app.com/reset-password?token={token}"; // Replace with your reset URL
            await emailService.SendEmailAsync(email, "Reset Your Password", $"Click <a href='{resetLink}'>here</a> to reset your password.");

            await DisplayAlert("Success", "A reset link has been sent to your email.", "OK");

        // Navigate to next step
        await Navigation.PushAsync(new NewPasswordPage(email));

        }



        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
