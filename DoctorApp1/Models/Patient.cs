using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SQLite;

namespace DoctorApp1.Models
{
    public class Patient : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int PatientID { get; set; }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged(nameof(LastName));
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private DateTime _dateOfBirth;
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (_dateOfBirth != value)
                {
                    _dateOfBirth = value;
                    OnPropertyChanged(nameof(DateOfBirth));
                    OnPropertyChanged(nameof(Age));
                }
            }
        }

        private string _contactInfo = string.Empty;
        public string ContactInfo
        {
            get => _contactInfo;
            set
            {
                if (_contactInfo != value)
                {
                    _contactInfo = value;
                    OnPropertyChanged(nameof(ContactInfo));
                }
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged(nameof(Address));
                }
            }
        }

        private string _occupation = string.Empty;
        public string Occupation
        {
            get => _occupation;
            set
            {
                if (_occupation != value)
                {
                    _occupation = value;
                    OnPropertyChanged(nameof(Occupation));
                }
            }
        }

        private string _generalNotes = string.Empty;
        public string GeneralNotes
        {
            get => _generalNotes;
            set
            {
                if (_generalNotes != value)
                {
                    _generalNotes = value;
                    OnPropertyChanged(nameof(GeneralNotes));
                }
            }
        }

        public string FullName => $"{FirstName} {LastName}";
        public int Age => DateTime.Now.Year - DateOfBirth.Year;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
