using DoctorApp1.Models;
using DoctorApp1.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace DoctorApp1.Views
{
    /// <summary>
    /// Page for viewing, adding, editing, and deleting patient appointments in a calendar view.
    /// </summary>
    public partial class CalendarPage : ContentPage
    {
        /// <summary>
        /// Gets or sets the currently selected date in the calendar.
        /// </summary>
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Gets the collection of appointments for the selected date or week.
        /// </summary>
        public ObservableCollection<Appointment> AppointmentsForSelectedDate { get; set; } = new();

        /// <summary>
        /// Gets the list of patients for selection in the appointment modal.
        /// </summary>
        public List<Patient> Patients { get; set; } = new();

        // Modal state
        private Appointment _editingAppointment = null;

        /// <summary>
        /// Gets or sets the title of the appointment modal.
        /// </summary>
        public string ModalTitle { get; set; } = "Add Appointment";

        /// <summary>
        /// Gets or sets the default start time for the modal.
        /// </summary>
        public TimeSpan ModalStartTime { get; set; } = new TimeSpan(9, 0, 0);

        /// <summary>
        /// Gets or sets the default end time for the modal.
        /// </summary>
        public TimeSpan ModalEndTime { get; set; } = new TimeSpan(10, 0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarPage"/> class.
        /// </summary>
        public CalendarPage()
        {
            InitializeComponent();
            BindingContext = this;

            LoadPatients();
            LoadAppointmentsForDate(SelectedDate);

            datePicker.DateSelected += (s, e) =>
            {
                SelectedDate = e.NewDate;
                LoadAppointmentsForDate(SelectedDate);
            };
        }

        /// <summary>
        /// Loads the list of patients from the database and sets up the patient picker.
        /// </summary>
        private void LoadPatients()
        {
            Patients = App.Database.GetPatients();
            var patientPicker = this.FindByName<Picker>("PatientPicker");
            if (patientPicker != null)
            {
                patientPicker.ItemsSource = Patients;
                patientPicker.ItemDisplayBinding = new Binding("FullName");
            }
        }

        /// <summary>
        /// Loads appointments for the specified date and updates the observable collection.
        /// </summary>
        /// <param name="date">The date to filter appointments by.</param>
        private void LoadAppointmentsForDate(DateTime date)
        {
            var allAppointments = App.Database.GetAppointments();
            var patients = App.Database.GetPatients();
            var filtered = allAppointments
                .Where(a => a.StartTime.Date == date.Date)
                .OrderBy(a => a.StartTime)
                .ToList();

            AppointmentsForSelectedDate.Clear();
            foreach (var appt in filtered)
            {
                // Set PatientName for display (requires [Ignore] property in Appointment)
                var patient = patients.FirstOrDefault(p => p.PatientID == appt.PatientID);
                appt.GetType().GetProperty("PatientName")?.SetValue(appt, patient?.FullName ?? "Unknown");
                AppointmentsForSelectedDate.Add(appt);
            }
        }

        /// <summary>
        /// Shows the modal for adding or editing an appointment.
        /// </summary>
        /// <param name="title">The modal title.</param>
        /// <param name="appt">The appointment to edit, or null to add new.</param>
        private void ShowAppointmentModal(string title, Appointment appt = null)
        {
            ModalTitle = title;
            _editingAppointment = appt;
            var appointmentModal = this.FindByName<ContentView>("AppointmentModal");
            var patientPicker = this.FindByName<Picker>("PatientPicker");
            var startTimePicker = this.FindByName<TimePicker>("StartTimePicker");
            var endTimePicker = this.FindByName<TimePicker>("EndTimePicker");
            var notesEditor = this.FindByName<Editor>("NotesEditor");

            if (appointmentModal != null)
                appointmentModal.IsVisible = true;

            // Set modal fields
            if (appt == null)
            {
                if (patientPicker != null) patientPicker.SelectedIndex = -1;
                if (startTimePicker != null) startTimePicker.Time = new TimeSpan(9, 0, 0);
                if (endTimePicker != null) endTimePicker.Time = new TimeSpan(10, 0, 0);
                if (notesEditor != null) notesEditor.Text = "";
            }
            else
            {
                var patientIndex = Patients.FindIndex(p => p.PatientID == appt.PatientID);
                if (patientPicker != null) patientPicker.SelectedIndex = patientIndex;
                if (startTimePicker != null) startTimePicker.Time = appt.StartTime.TimeOfDay;
                if (endTimePicker != null) endTimePicker.Time = appt.EndTime.TimeOfDay;
                if (notesEditor != null) notesEditor.Text = appt.Notes;
            }
        }

        /// <summary>
        /// Handles the Cancel button in the appointment modal.
        /// </summary>
        private void OnCancelModal(object sender, EventArgs e)
        {
            var appointmentModal = this.FindByName<ContentView>("AppointmentModal");
            if (appointmentModal != null)
                appointmentModal.IsVisible = false;
        }

        /// <summary>
        /// Handles the Save button in the appointment modal.
        /// </summary>
        private void OnSaveModal(object sender, EventArgs e)
        {
            var patientPicker = this.FindByName<Picker>("PatientPicker");
            var startTimePicker = this.FindByName<TimePicker>("StartTimePicker");
            var endTimePicker = this.FindByName<TimePicker>("EndTimePicker");
            var notesEditor = this.FindByName<Editor>("NotesEditor");
            var appointmentModal = this.FindByName<ContentView>("AppointmentModal");

            if (patientPicker == null || patientPicker.SelectedIndex < 0)
            {
                DisplayAlert("Error", "Please select a patient.", "OK");
                return;
            }
            var patient = Patients[patientPicker.SelectedIndex];
            var start = SelectedDate.Date + (startTimePicker?.Time ?? new TimeSpan(9, 0, 0));
            var end = SelectedDate.Date + (endTimePicker?.Time ?? new TimeSpan(10, 0, 0));
            var notes = notesEditor?.Text ?? "";

            if (_editingAppointment == null)
            {
                var newAppt = new Appointment
                {
                    PatientID = patient.PatientID,
                    StartTime = start,
                    EndTime = end,
                    Notes = notes
                };
                App.Database.AddAppointment(newAppt);
            }
            else
            {
                _editingAppointment.PatientID = patient.PatientID;
                _editingAppointment.StartTime = start;
                _editingAppointment.EndTime = end;
                _editingAppointment.Notes = notes;
                App.Database.UpdateAppointment(_editingAppointment);
            }

            if (appointmentModal != null)
                appointmentModal.IsVisible = false;
            LoadAppointmentsForDate(SelectedDate);
        }

        /// <summary>
        /// Handles the Add Appointment button click.
        /// </summary>
        private void OnAddAppointmentClicked(object sender, EventArgs e)
        {
            ShowAppointmentModal("Add Appointment");
        }

        /// <summary>
        /// Handles the Edit button click for an appointment.
        /// </summary>
        private void OnEditAppointmentClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Appointment appt)
            {
                ShowAppointmentModal("Edit Appointment", appt);
            }
        }

        /// <summary>
        /// Handles the Delete button click for an appointment.
        /// </summary>
        private async void OnDeleteAppointmentClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Appointment appt)
            {
                bool confirm = await DisplayAlert("Delete", "Delete this appointment?", "Yes", "No");
                if (!confirm) return;

                App.Database.DeleteAppointment(appt);
                LoadAppointmentsForDate(SelectedDate);
            }
        }

        /// <summary>
        /// Switches to day view and reloads appointments for the selected day.
        /// </summary>
        private void OnDayViewClicked(object sender, EventArgs e)
        {
            LoadAppointmentsForDate(SelectedDate);
        }

        /// <summary>
        /// Switches to week view and loads appointments for the selected week.
        /// </summary>
        private void OnWeekViewClicked(object sender, EventArgs e)
        {
            var allAppointments = App.Database.GetAppointments();
            var patients = App.Database.GetPatients();
            var startOfWeek = SelectedDate.Date.AddDays(-(int)SelectedDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var filtered = allAppointments
                .Where(a => a.StartTime.Date >= startOfWeek && a.StartTime.Date < endOfWeek)
                .OrderBy(a => a.StartTime)
                .ToList();

            AppointmentsForSelectedDate.Clear();
            foreach (var appt in filtered)
            {
                var patient = patients.FirstOrDefault(p => p.PatientID == appt.PatientID);
                appt.GetType().GetProperty("PatientName")?.SetValue(appt, patient?.FullName ?? "Unknown");
                AppointmentsForSelectedDate.Add(appt);
            }
        }
    }
}


