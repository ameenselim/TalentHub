using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }

        public string UserId { get; set; } = null!;
        public string? CompanyId { get; set; }
        public string UserName { get; set; } =null!;
        public string Email { get; set; } = null!;
    }
}
