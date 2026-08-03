using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class RegisterUserRequest
    {
        [Required]
        [MaxLength(25)]
        [MinLength(3)]
        public string FirstName { get; set; } = null!;
        [Required]
        [MaxLength(25)]
        [MinLength(3)]
        public string LastName { get; set; } = null!;
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        [MinLength(10)]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
