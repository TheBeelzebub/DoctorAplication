using System;
using DoctorApp1.Models;
using Microsoft.Maui.Controls;

namespace DoctorApp1
{
    public partial class PatientDetailEditPage : ContentPage
    {
        public Patient Patient { get; set; }

        public PatientDetailEditPage(int patientId)
        {
            InitializeComponent();
            Patient = App.Database.GetPatient(patientId);
            BindingContext = this;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Patient.FirstName) || string.IsNullOrWhiteSpace(Patient.LastName))
            {
                await DisplayAlert("Validation Error", "First and last name are required", "OK");
                return;
            }

            App.Database.UpdatePatient(Patient);
            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}