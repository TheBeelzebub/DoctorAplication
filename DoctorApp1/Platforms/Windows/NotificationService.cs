#if WINDOWS
using System;
using System.Linq;
using Windows.UI.Notifications;

namespace DoctorApp1.Platforms.Windows
{
    public static class NotificationService
    {
        // Schedule or update a toast notification with unique appointment ID
        public static void ScheduleToast(string id, string title, string message, DateTime deliveryTime)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            RemoveScheduledToast(id); // Remove old toast with same ID if exists

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(message));

            var scheduledTime = new DateTimeOffset(deliveryTime);
            var toast = new ScheduledToastNotification(toastXml, scheduledTime)
            {
                Id = id
            };

            notifier.AddToSchedule(toast);
        }

        // Remove scheduled toast by ID
        public static void RemoveScheduledToast(string id)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();

            var toastToRemove = scheduled.FirstOrDefault(t => t.Id == id);
            if (toastToRemove != null)
                notifier.RemoveFromSchedule(toastToRemove);
        }
    }
}
#endif
