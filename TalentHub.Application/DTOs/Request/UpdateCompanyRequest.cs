using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TalentHub.Application.DTOs.Request
{
    public class UpdateCompanyRequest
    {
        [Required]
        [MaxLength(150)]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public IFormFile? Logo { get; set; }

        public IFormFile? CoverImage { get; set; }

        public IEnumerable<IFormFile>? AdditionalImages { get; set; }

        [Url]
        public string? Website { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public DateOnly? FoundedDate { get; set; }

        public string? Size { get; set; }

        public string? Industry { get; set; }
    }
}