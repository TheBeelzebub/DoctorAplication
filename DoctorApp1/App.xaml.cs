using DoctorApp1.Services;
using DoctorApp1.Views; // Needed for LoginPage and MainPage
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // For Preferences
using System.IO;
using System.Linq;

namespace DoctorApp1
{
    public partial class App : Application
    {
        public static DatabaseService Database { get; private set; } = null!;
        private readonly AppointmentNotificationService notificationService;

        // Inject AppointmentNotificationService via constructor
        public App(AppointmentNotificationService notificationService)
        {
            InitializeComponent();

            this.notificationService = notificationService;

            // Initialize the database
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "doctorapp.db");
            Database = new DatabaseService(dbPath);

            // Schedule notifications for all upcoming appointments
            RescheduleAllNotifications();
        }

        private void RescheduleAllNotifications()
        {
            var appointments = Database.GetAppointments();
            var patients = Database.GetPatients();

            foreach (var appt in appointments)
            {
                var patient = patients.FirstOrDefault(p => p.PatientID == appt.PatientID);
                if (patient != null)
                {
                    notificationService.ScheduleNotification(appt, patient);
                }
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Check if user is already logged in
            string? email = Preferences.Get("LoggedInEmail", null);

            Page startupPage;
            if (!string.IsNullOrEmpty(email))
            {
                startupPage = new MainPage(email); // Go directly to the main page
            }
            else
            {
                startupPage = new LoginPage(); // Show login screen
            }

            return new Window(new NavigationPage(startupPage));
        }
    }
}
