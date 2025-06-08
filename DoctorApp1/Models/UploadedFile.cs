using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApp1.Models
{
    public class UploadedFile
    {
        [PrimaryKey, AutoIncrement]
        public int FileID { get; set; }
        public int PatientID { get; set; } // Foreign key to Patient
        public string FilePath { get; set; } // Path to the file on disk
        public string FileType { get; set; } // e.g., PDF, JPG, PNG
        public DateTime UploadDate { get; set; }
    }
}
