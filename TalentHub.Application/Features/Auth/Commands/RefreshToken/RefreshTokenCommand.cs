using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
