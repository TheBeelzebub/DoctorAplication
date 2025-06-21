using DoctorApp1.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApp1.Services
{
    public class DatabaseService
    {
        private SQLiteConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Patient>();
            _database.CreateTable<MedicalHistory>();
            _database.CreateTable<Medicine>();
            _database.CreateTable<UploadedFile>();
            _database.CreateTable<DoctorUser>();
            _database.CreateTable<Appointment>();

        }

        // File Uploads
        public List<UploadedFile> GetUploadedFiles(int patientId)
        {
            return _database.Table<UploadedFile>()
                            .Where(f => f.PatientID == patientId)
                            .ToList();
        }
        public void AddUploadedFile(UploadedFile file)
        {
            _database.Insert(file);
        }


        // Patient CRUD
        public List<Patient> GetPatients() => _database.Table<Patient>().ToList();
        public Patient GetPatient(int id) => _database.Table<Patient>().FirstOrDefault(p => p.PatientID == id);
        public void AddPatient(Patient patient) => _database.Insert(patient);
        public void UpdatePatient(Patient patient) => _database.Update(patient);
        public void DeletePatient(Patient patient) => _database.Delete(patient);

        // MedicalHistory CRUD
        public List<MedicalHistory> GetMedicalHistory(int patientId) => _database.Table<MedicalHistory>().Where(m => m.PatientID == patientId).ToList();
        public void AddMedicalHistory(MedicalHistory history) => _database.Insert(history);
        public void UpdateMedicalHistory(MedicalHistory history) => _database.Update(history);
        public void DeleteMedicalHistory(MedicalHistory history) => _database.Delete(history);

        // Medicine CRUD
        public List<Medicine> GetMedicines(int patientId) => _database.Table<Medicine>().Where(m => m.PatientID == patientId).ToList();
        public void AddMedicine(Medicine medicine) => _database.Insert(medicine);
        public void UpdateMedicine(Medicine medicine) => _database.Update(medicine);
        public void DeleteMedicine(Medicine medicine) => _database.Delete(medicine);

        // Appointment CRUD
        public List<Appointment> GetAppointments()
        {
            return _database.Table<Appointment>().ToList();
        }

        public List<Appointment> GetAppointmentsForPatient(int patientId)
        {
            return _database.Table<Appointment>().Where(a => a.PatientID == patientId).ToList();
        }

        public Appointment GetAppointment(int appointmentId)
        {
            return _database.Table<Appointment>().FirstOrDefault(a => a.AppointmentID == appointmentId);
        }

        public void AddAppointment(Appointment appointment)
        {
            _database.Insert(appointment);
        }

        public void UpdateAppointment(Appointment appointment)
        {
            _database.Update(appointment);
        }

        public void DeleteAppointment(Appointment appointment)
        {
            _database.Delete(appointment);
        }


        public DoctorUser GetUserByEmail(string email)
        {
            var processedEmail = email.Trim().ToLower();
            return _database.Table<DoctorUser>().FirstOrDefault(u => u.Email.ToLower() == processedEmail);
        }


        public void AddUser(DoctorUser user)
        {
            _database.Insert(user);
        }

        public void UpdateUser(DoctorUser user)
        {
            _database.Update(user);
        }

        public void DeleteUser(DoctorUser user)
        {
            _database.Delete(user);
        }

        public void ConfirmEmail(string token)
        {
            var user = _database.Table<DoctorUser>().FirstOrDefault(u => u.PasswordResetToken == token && u.TokenExpiration > DateTime.Now);
            if (user != null)
            {
                user.IsEmailConfirmed = true;
                user.PasswordResetToken = null;
                user.TokenExpiration = null;
                _database.Update(user);
            }
        }

    }


}
