using Azure.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Infrastructure.Services
{
    public class EmailSender :IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("ameenselim682005@gmail.com", "amgn eurd oqwp pmwg")
            };

            return client.SendMailAsync(
            new MailMessage(from: "ameenselim628005@gmail.com",
                            to: email,
                            subject,
                            htmlMessage
                            )
            {
                IsBodyHtml = true
            });
        }
    }
}
