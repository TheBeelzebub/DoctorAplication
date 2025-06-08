using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DoctorApp1.Models
{
    public class DoctorUser
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Unique]
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordResetToken { get; set; }
        public DateTime? TokenExpiration { get; set; }

        // Fields for email validation
        public string EmailValidationOtp { get; set; } // New field for email validation OTP
        public DateTime? EmailValidationOtpExpiration { get; set; } // New field for OTP expiration

        public bool IsEmailConfirmed { get; set; } = false;
    }
}

