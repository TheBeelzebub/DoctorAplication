using System;
using System.Collections.Generic;
using DoctorApp1.Models;
using Microsoft.Maui.Controls;

namespace DoctorApp1
{
    public partial class PatientDetailPage : ContentPage
    {
        public Patient Patient { get; set; }
        public List<MedicalHistory> MedicalHistory { get; set; }
        public List<Medicine> Medicines { get; set; }
        public List<UploadedFile> UploadedFiles { get; set; }

        public PatientDetailPage(int patientId)
        {
            InitializeComponent();
            LoadPatientData(patientId);
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPatientData(Patient.PatientID);
        }

        private void LoadPatientData(int patientId)
        {
            Patient = App.Database.GetPatient(patientId);
            MedicalHistory = App.Database.GetMedicalHistory(patientId);
            Medicines = App.Database.GetMedicines(patientId);
            UploadedFiles = App.Database.GetUploadedFiles(patientId);

            OnPropertyChanged(nameof(Patient));
            OnPropertyChanged(nameof(MedicalHistory));
            OnPropertyChanged(nameof(Medicines));
            OnPropertyChanged(nameof(UploadedFiles));
        }

        private async void OnUploadFileClicked(object sender, EventArgs e)
        {
            try
            {
                var file = await FilePicker.PickAsync();
                if (file != null)
                {
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, file.FileName);
                    using (var stream = await file.OpenReadAsync())
                    using (var fileStream = File.OpenWrite(filePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    var uploadedFile = new UploadedFile
                    {
                        PatientID = Patient.PatientID,
                        FilePath = filePath,
                        FileType = Path.GetExtension(filePath),
                        UploadDate = DateTime.Now
                    };

                    App.Database.AddUploadedFile(uploadedFile);
                    UploadedFiles = App.Database.GetUploadedFiles(Patient.PatientID);
                    OnPropertyChanged(nameof(UploadedFiles));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"File upload failed: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PatientDetailEditPage(Patient.PatientID));
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to delete {Patient.FullName}'s record?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                App.Database.DeletePatient(Patient);
                await Navigation.PopAsync(); // Return to patient list
            }
        }
    }
}