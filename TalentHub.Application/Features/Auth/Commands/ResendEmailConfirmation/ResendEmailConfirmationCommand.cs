using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Features.Auth.Commands.ResendEmailConfirmation
{
    public record ResendEmailConfirmationCommand(ResendEmailConfirmationRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
