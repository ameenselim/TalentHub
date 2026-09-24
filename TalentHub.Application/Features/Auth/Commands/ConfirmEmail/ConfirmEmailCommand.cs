using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Features.Auth.Commands.ConfirmEmail
{
    public record ConfirmEmailCommand(string UserId, string Token) : IRequest<ApiResponse<AuthResponse>>;
}
