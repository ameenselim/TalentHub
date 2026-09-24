using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Features.Auth.Commands.ValidOTP
{
    public record ValidOTPCommand(ValidOTPRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
