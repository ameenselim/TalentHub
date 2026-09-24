using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Profile.Queries.GetUserProfile
{
    public record GetUserProfileQuery(string UserId) : IRequest<ApiResponse<UserProfileResponse>>;

    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>>
    {
        private readonly IProfileServices _profileServices;

        public GetUserProfileQueryHandler(IProfileServices profileServices)
        {
            _profileServices = profileServices;
        }

        public async Task<ApiResponse<UserProfileResponse>> Handle(GetUserProfileQuery query, CancellationToken cancellationToken)
        {
            return await _profileServices.GetUserProfileAsync(query.UserId);
        }
    }
}
