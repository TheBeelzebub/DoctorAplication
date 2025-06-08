using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApp1.Models
{
    public class Medicine
    {
        [PrimaryKey, AutoIncrement]
        public int MedicineID { get; set; }
        public int PatientID { get; set; } // Foreign key to Patient
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string Notes { get; set; }
    }
}
