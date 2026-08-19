using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class UserProfileResponse
    {
        public string Id { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string? ProfileImage { get; init; }
        public string? Headline { get; init; }
        public string? Bio { get; init; }
        public string? Country { get; init; }
        public string? City { get; init; }
        public IReadOnlyCollection<CompanyMembershipResponse> Companies { get; init; }
            = [];
    }
}
