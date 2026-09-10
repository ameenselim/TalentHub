using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class AddCompanyImageRequest
    {
        [Required]
        public IFormFile Image { get; set; } = null!;
        public string? Caption { get; set; }
        public bool IsCover { get; set; }
    }
}
