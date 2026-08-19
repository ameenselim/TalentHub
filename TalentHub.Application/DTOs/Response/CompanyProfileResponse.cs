using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.DTOs.Response
{
    public class CompanyProfileResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public string? Logo { get; init; }
        public string? Website { get; init; }
        public string? Email { get; init; }
        public string? Size { get; init; }
        public string? Country { get; init; }
        public string? City { get; init; }
        public string? Industry { get; init; }
        public bool IsVerified { get; init; }
        public IReadOnlyCollection<CompanyImage> Images { get; init; }
            = [];
    }
}
