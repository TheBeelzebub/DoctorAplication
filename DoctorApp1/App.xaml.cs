using DoctorApp1.Services;
using DoctorApp1.Views; // Needed for LoginPage and MainPage
using Microsoft.Maui.Controls;

namespace DoctorApp1
{
    public partial class App : Application
    {
        public static DatabaseService Database { get; private set; } = null!;

        public App()
        {
            InitializeComponent();

            // Initialize the database
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "doctorapp.db");
            Database = new DatabaseService(dbPath);

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
