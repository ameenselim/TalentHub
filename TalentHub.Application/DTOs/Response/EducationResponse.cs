using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class EducationResponse
    {
        public int Id { get; set; }
        public string University { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
