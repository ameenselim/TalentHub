using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IJwtService
    {
        Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user);
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}
