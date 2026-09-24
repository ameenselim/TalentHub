using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Features.Auth.Commands.RegisterJobSeeker
{
    public record RegisterJobSeekerCommand(RegisterUserRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
