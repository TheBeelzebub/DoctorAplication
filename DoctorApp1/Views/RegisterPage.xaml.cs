using DoctorApp1.Models;
using Microsoft.Maui.Controls;
using System;
using DoctorApp1.Converters;
using System.Security.Cryptography;
using System.Text;
using DoctorApp1.Services;
using System.Text.RegularExpressions;

namespace DoctorApp1.Views
{
    /// <summary>
    /// Represents the registration page of the application.
    /// </summary>
    public partial class RegisterPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterPage"/> class.
        /// </summary>
        public RegisterPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the registration process when the register button is clicked.
        /// Sends an OTP to the user's email for in-app email confirmation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string? email = EmailEntry.Text?.Trim();
            string? password = PasswordEntry.Text;
            string? confirmPassword = ConfirmPasswordEntry.Text;

            // Validate input fields
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Error", "All fields are required.", "OK");
                return;
            }

            if (!IsValidEmail(email))
            {
                await DisplayAlert("Error", "Please enter a valid email address.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // Check if the email is already registered
            var existingUser = App.Database.GetUserByEmail(email);
            if (existingUser != null)
            {
                await DisplayAlert("Error", "This email is already registered.", "OK");
                return;
            }

            // Generate OTP for email confirmation
            string emailOtp = GenerateOTP();

            // Send OTP email
            var emailService = new EmailService();
            await emailService.SendEmailAsync(email, "Confirm Your Email Address", $"Your OTP is: {emailOtp}");

            await DisplayAlert("Success", "Please check your email for the OTP to confirm your address.", "OK");

            // Prompt user to enter OTP
            string enteredOtp = await DisplayPromptAsync("Email Confirmation", "Enter the OTP sent to your email:");

            if (string.IsNullOrWhiteSpace(enteredOtp) || enteredOtp != emailOtp)
            {
                await DisplayAlert("Error", "Invalid or expired OTP. Please try again.", "OK");
                return;
            }

            // Add the user to the database only after OTP validation
            var newUser = new DoctorUser
            {
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                IsEmailConfirmed = true // Mark email as confirmed
            };

            App.Database.AddUser(newUser);

            await DisplayAlert("Success", "Your email has been confirmed. You can now log in.", "OK");

            // Go back to login screen
            await Navigation.PopAsync();
        }

        /// <summary>
        /// Validates the email format using a regular expression.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email is valid, otherwise false.</returns>
        private bool IsValidEmail(string email)
        {
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // Simple email regex
            return Regex.IsMatch(email, emailRegex);
        }

        /// <summary>
        /// Generates a 6-digit OTP.
        /// </summary>
        /// <returns>A 6-digit OTP as a string.</returns>
        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        /// <summary>
        /// Navigates back to the login page when the back button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Return to login
        }
    }
}

