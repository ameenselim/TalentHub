using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Features.Auth.Commands.Logout
{
    public record LogoutCommand(RefreshTokenRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
