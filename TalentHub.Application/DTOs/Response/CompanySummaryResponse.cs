using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class CompanySummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
    }
}
