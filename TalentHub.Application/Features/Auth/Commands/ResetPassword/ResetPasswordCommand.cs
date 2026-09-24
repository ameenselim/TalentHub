using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Features.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
