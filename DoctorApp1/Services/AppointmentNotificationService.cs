using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using DoctorApp1.Models;

namespace DoctorApp1.Services
{
    public class AppointmentNotificationService
    {
        private Dictionary<int, CancellationTokenSource> scheduledNotifications = new();
        private HashSet<int> alertedAppointmentIds = new();
        private HashSet<int> missedNotificationIds = new();
        private HashSet<int> seenMissedNotificationIds = new();

        private const string AlertedKey = "AlertedAppointmentIDs";
        private const string MissedKey = "MissedAppointmentIDs";
        private const string SeenMissedKey = "SeenMissedAppointmentIDs";

        public bool EnableGlobalPopups { get; set; } = false; //flag for enabling global popups

        public AppointmentNotificationService()
        {
            LoadAlertedAppointments();
            LoadMissedNotifications();
            LoadSeenMissedNotifications();
        }

        private void LoadAlertedAppointments()
        {
            var idsString = Preferences.Get(AlertedKey, "");
            alertedAppointmentIds = ParseIds(idsString);
        }

        private void SaveAlertedAppointments()
        {
            Preferences.Set(AlertedKey, string.Join(",", alertedAppointmentIds));
        }

        private void LoadMissedNotifications()
        {
            var missedString = Preferences.Get(MissedKey, "");
            missedNotificationIds = ParseIds(missedString);
        }

        private void SaveMissedNotifications()
        {
            Preferences.Set(MissedKey, string.Join(",", missedNotificationIds));
        }

        private void LoadSeenMissedNotifications()
        {
            var seenString = Preferences.Get(SeenMissedKey, "");
            seenMissedNotificationIds = ParseIds(seenString);
        }

        private void SaveSeenMissedNotifications()
        {
            Preferences.Set(SeenMissedKey, string.Join(",", seenMissedNotificationIds));
        }

        private HashSet<int> ParseIds(string idsString)
        {
            return string.IsNullOrWhiteSpace(idsString)
                ? new HashSet<int>()
                : new HashSet<int>(idsString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));
        }

        public void MarkMissedNotifications()
        {
            var now = DateTime.Now;
            var appointments = App.Database.GetAppointments();

            foreach (var appt in appointments)
            {
                var notifyTime = appt.StartTime.AddMinutes(-10);
                if (notifyTime < now && !alertedAppointmentIds.Contains(appt.AppointmentID))
                {
                    missedNotificationIds.Add(appt.AppointmentID);
                }
            }

            SaveMissedNotifications();
        }

        public List<Appointment> GetMissedAppointments()
        {
            var appointments = App.Database.GetAppointments();
            return appointments
                .Where(a => missedNotificationIds.Contains(a.AppointmentID)
                         && !seenMissedNotificationIds.Contains(a.AppointmentID))
                .ToList();
        }

        public void ClearMissedNotification(int appointmentId)
        {
            if (missedNotificationIds.Remove(appointmentId))
                SaveMissedNotifications();

            seenMissedNotificationIds.Add(appointmentId);
            SaveSeenMissedNotifications();
        }

        public void ScheduleNotification(Appointment appt, Patient patient)
        {
            if (appt == null || patient == null)
                return;

            CancelNotification(appt.AppointmentID);

            if (alertedAppointmentIds.Contains(appt.AppointmentID) || appt.StartTime <= DateTime.Now)
                return;

            var notifyTime = appt.StartTime.AddMinutes(-10);
            var delay = notifyTime - DateTime.Now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            var cts = new CancellationTokenSource();
            scheduledNotifications[appt.AppointmentID] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);
                    if (cts.Token.IsCancellationRequested) return;

                    string message = $"Appointment is scheduled with {patient.FullName} at {appt.StartTime:HH:mm}";

                    if (EnableGlobalPopups)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Application.Current.MainPage.DisplayAlert("Upcoming Appointment", message, "OK");
                        });
                    }

                    alertedAppointmentIds.Add(appt.AppointmentID);
                    SaveAlertedAppointments();

                    missedNotificationIds.Remove(appt.AppointmentID);
                    seenMissedNotificationIds.Add(appt.AppointmentID);
                    SaveMissedNotifications();
                    SaveSeenMissedNotifications();

                    scheduledNotifications.Remove(appt.AppointmentID);
                }
                catch (TaskCanceledException)
                {
                    // Canceled
                }
            });
        }

        public void CancelNotification(int appointmentId)
        {
            if (scheduledNotifications.TryGetValue(appointmentId, out var cts))
            {
                cts.Cancel();
                scheduledNotifications.Remove(appointmentId);
            }

            alertedAppointmentIds.Remove(appointmentId);
            SaveAlertedAppointments();
        }

        public void RescheduleNotification(Appointment appt, Patient patient)
        {
            CancelNotification(appt.AppointmentID);
            ScheduleNotification(appt, patient);
        }
    }
}
