using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DoctorApp1.Models
{
    public class MedicalHistory
    {
        [PrimaryKey, AutoIncrement]
        public int HistoryID { get; set; }
        public int PatientID { get; set; } // Foreign key to Patient
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public DateTime DateRecorded { get; set; }
    }
}
