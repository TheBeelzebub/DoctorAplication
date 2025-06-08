using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DoctorApp1.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com"; // Replace with your SMTP server
        private readonly int _smtpPort = 587; // Replace with your SMTP port
        private readonly string _senderEmail = "koumides.a@gmail.com"; // Replace with your email
        private readonly string _senderPassword; // Replace with your email password

        public EmailService()
        {
            // Initialize _senderPassword using the DecryptPassword method
            _senderPassword = DecryptPassword("cHNybCB1bHR1IHN5a3IgenJ6cw==");
        }
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email (HTML supported).</param>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_smtpServer)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string DecryptPassword(string encryptedPassword)
        {
            var base64EncodedBytes = Convert.FromBase64String(encryptedPassword);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
