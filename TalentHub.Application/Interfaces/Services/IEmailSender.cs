using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
