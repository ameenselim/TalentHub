using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.Common.Enums;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task SendMailAsync(string userId, EmailType emailType = EmailType.Register);
    }
}
