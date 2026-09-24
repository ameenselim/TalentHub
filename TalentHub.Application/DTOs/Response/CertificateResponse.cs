using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class CertificateResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public DateOnly IssueDate { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public string? CertificateUrl { get; set; }
    }
}
