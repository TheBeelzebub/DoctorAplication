using DoctorApp1.Models;
using DoctorApp1.Services;
using Microsoft.Maui.Controls;

namespace DoctorApp1
{
    public partial class AddPatientPage : ContentPage
    {
        public AddPatientPage()
        {
            InitializeComponent();
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            var patient = new Patient
            {
                FirstName = FirstNameEntry.Text,
                LastName = LastNameEntry.Text,
                DateOfBirth = DateOfBirthPicker.Date,
                ContactInfo = ContactInfoEntry.Text,
                Address = AddressEntry.Text,
                Occupation = OccupationEntry.Text,
                GeneralNotes = GeneralNotesEntry.Text
            };

            App.Database.AddPatient(patient);
            Navigation.PopAsync(); // Go back to the previous page
        }
    }
}