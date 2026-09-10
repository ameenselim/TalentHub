using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class CompanyListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string? Industry { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Size { get; set; }
        public bool IsVerified { get; set; }
        public int FollowersCount { get; set; }
        public int JobsCount { get; set; }
    }
}
