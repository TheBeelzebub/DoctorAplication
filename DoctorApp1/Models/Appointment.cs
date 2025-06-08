using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DoctorApp1.Models
{
    public class Appointment
    {
        [PrimaryKey, AutoIncrement]
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Notes { get; set; }
        // Add more fields as needed (e.g., Location, Type)
    }
}
