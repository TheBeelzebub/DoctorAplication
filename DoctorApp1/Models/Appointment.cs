using System;
using System.ComponentModel;
using SQLite;

namespace DoctorApp1.Models
{
    public class Appointment : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int AppointmentID { get; set; }

        private int _patientID;
        public int PatientID
        {
            get => _patientID;
            set
            {
                if (_patientID != value)
                {
                    _patientID = value;
                    OnPropertyChanged(nameof(PatientID));
                }
            }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                if (_endTime != value)
                {
                    _endTime = value;
                    OnPropertyChanged(nameof(EndTime));
                }
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        // Add more fields as needed (e.g., Location, Type), following the same pattern.

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}