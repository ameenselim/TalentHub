using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Profile.Queries.GetCompanyProfile
{
    public record GetCompanyProfileQuery(int CompanyId) : IRequest<ApiResponse<CompanyProfileResponse>>;

    public class GetCompanyProfileQueryHandler : IRequestHandler<GetCompanyProfileQuery, ApiResponse<CompanyProfileResponse>>
    {
        private readonly IProfileServices _profileServices;

        public GetCompanyProfileQueryHandler(IProfileServices profileServices)
        {
            _profileServices = profileServices;
        }

        public async Task<ApiResponse<CompanyProfileResponse>> Handle(GetCompanyProfileQuery query, CancellationToken cancellationToken)
        {
            return await _profileServices.GetCompanyProfileAsync(query.CompanyId);
        }
    }
}
