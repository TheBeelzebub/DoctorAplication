using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoctorApp1.Models;
using DoctorApp1.Services;
using Microsoft.Maui.Controls;

namespace DoctorApp1
{
    public partial class PatientDetailPage : ContentPage
    {
        public Patient Patient { get; set; }
        public List<MedicalHistory> MedicalHistory { get; set; }
        public List<Medicine> Medicines { get; set; }
        public List<UploadedFile> UploadedFiles { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

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

            // Reset to view mode whenever data loads
            IsEditing = false;

            // Notify all properties changed
            OnPropertyChanged(nameof(Patient));
            OnPropertyChanged(nameof(MedicalHistory));
            OnPropertyChanged(nameof(Medicines));
            OnPropertyChanged(nameof(UploadedFiles));
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            // No need to create a new instance - we'll bind directly
            IsEditing = true;

            // Force UI refresh
            OnPropertyChanged(nameof(Patient));
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

        private void OnSaveClicked(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Patient.FirstName) || string.IsNullOrWhiteSpace(Patient.LastName))
            {
                DisplayAlert("Validation Error", "First and last name are required", "OK");
                return;
            }

            App.Database.UpdatePatient(Patient);
            LoadPatientData(Patient.PatientID); // Refresh data
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            LoadPatientData(Patient.PatientID); // Reload original data
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
    }
}
