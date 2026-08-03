using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.Common.Settings;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Infrastructure.Services
{
    public class AccountServices : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly AppSettings _appSettings;

        public AccountServices(UserManager<ApplicationUser> userManager , IOptions<AppSettings> appSettings, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _appSettings = appSettings.Value;
        }
        public async Task SendMailAsync(string userId,EmailType emailType =EmailType.Register)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
            var encodeToken =Uri.EscapeDataString(token);
            var link =$"{_appSettings.ApiBaseUrl}/api/auth/confirm-email?userId={user!.Id}&token={encodeToken}";

            string subject = string.Empty;
            string message = string.Empty;
            switch (emailType)
            {
                case EmailType.Register:
                    {
                        subject = "Confirmation Your Account in TalentHub App";
                        message = $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>";
                    }
                    break;
                case EmailType.ResendConfirmation:
                    {
                        subject = "Resend Confirmation Your Account in TalentHub App";
                        message = $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>";
                    }
                    break;
            }
            await _emailSender.SendEmailAsync(user.Email!, subject, message);

        }
    }
}
