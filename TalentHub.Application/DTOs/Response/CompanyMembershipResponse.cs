using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class CompanyMembershipResponse
    {
        public int CompanyId { get; init; }
        public string CompanyName { get; init; } = null!;
        public string? Logo { get; init; }
        public string Role { get; init; } = null!;
    }
}
