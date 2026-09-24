using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Features.Auth.Commands.ForgetPassword
{
    public record ForgetPasswordCommand(ForgetPasswordRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
