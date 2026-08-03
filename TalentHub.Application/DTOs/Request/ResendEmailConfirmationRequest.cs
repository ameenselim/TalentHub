using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class ResendEmailConfirmationRequest
    {
        [Required(ErrorMessage = "Email or username is required.")]
        public string EmailOrUserName { get; set; } = string.Empty;
    }
}
