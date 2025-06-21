using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // for MainThread
using Microsoft.Maui.Storage;          // for Preferences
using DoctorApp1.Models;               // your models namespace

namespace DoctorApp1.Services
{
    public class AppointmentNotificationService
    {
        private Dictionary<int, CancellationTokenSource> scheduledNotifications = new();
        private HashSet<int> alertedAppointmentIds = new();

        public AppointmentNotificationService()
        {
            LoadAlertedAppointments();
        }

        private void LoadAlertedAppointments()
        {
            var idsString = Preferences.Get("AlertedAppointmentIDs", "");
            if (!string.IsNullOrEmpty(idsString))
            {
                alertedAppointmentIds = new HashSet<int>(
                    idsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => int.Parse(s))
                );
            }
            else
            {
                alertedAppointmentIds = new HashSet<int>();
            }
        }

        private void SaveAlertedAppointments()
        {
            var str = string.Join(",", alertedAppointmentIds);
            Preferences.Set("AlertedAppointmentIDs", str);
        }

        public void ScheduleNotification(Appointment appt, Patient patient)
        {
            if (appt == null || patient == null)
                return;

            // Cancel existing if any
            CancelNotification(appt.AppointmentID);

            // Skip if already alerted or appointment time passed
            if (alertedAppointmentIds.Contains(appt.AppointmentID) || appt.StartTime <= DateTime.Now)
                return;

            var notifyTime = appt.StartTime.AddMinutes(-10);
            var delay = notifyTime - DateTime.Now;

            if (delay <= TimeSpan.Zero)
                delay = TimeSpan.Zero;

            var cts = new CancellationTokenSource();
            scheduledNotifications[appt.AppointmentID] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);

                    if (cts.Token.IsCancellationRequested)
                        return;

                    string message = $"Appointment is scheduled with {patient.FullName} at {appt.StartTime:HH:mm}";

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // You can replace this with your preferred in-app popup/notification method
                        Application.Current.MainPage.DisplayAlert("Upcoming Appointment", message, "OK");
                    });

                    alertedAppointmentIds.Add(appt.AppointmentID);
                    SaveAlertedAppointments();

                    scheduledNotifications.Remove(appt.AppointmentID);
                }
                catch (TaskCanceledException)
                {
                    // Notification cancelled - do nothing
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
