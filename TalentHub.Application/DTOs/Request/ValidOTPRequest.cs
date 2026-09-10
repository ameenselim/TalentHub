using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class ValidOTPRequest
    {
        [Required]
        public string OTP { get; set; } = string.Empty;
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}