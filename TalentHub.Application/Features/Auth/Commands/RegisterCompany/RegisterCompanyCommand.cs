using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Features.Auth.Commands.RegisterCompany
{
    public record RegisterCompanyCommand(RegisterCompanyRequest Request) : IRequest<ApiResponse<AuthResponse>>;
}
