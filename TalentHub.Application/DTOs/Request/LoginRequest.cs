using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class LoginRequest
    {
        [Required]
        public string UserNameOrEmail { get; set; } = null!;
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
