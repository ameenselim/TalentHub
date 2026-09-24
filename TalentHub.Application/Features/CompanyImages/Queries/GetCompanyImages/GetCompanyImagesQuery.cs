using MediatR;
using System.Collections.Generic;
using System.Linq;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.CompanyImages.Queries.GetCompanyImages
{
    public record GetCompanyImagesQuery(int CompanyId) : IRequest<ApiResponse<IEnumerable<CompanyImagesResponse>>>;

    public class GetCompanyImagesQueryHandler : IRequestHandler<GetCompanyImagesQuery, ApiResponse<IEnumerable<CompanyImagesResponse>>>
    {
        private readonly IRepository<CompanyImage> _companyImageRepository;

        public GetCompanyImagesQueryHandler(IRepository<CompanyImage> companyImageRepository)
        {
            _companyImageRepository = companyImageRepository;
        }

        public async Task<ApiResponse<IEnumerable<CompanyImagesResponse>>> Handle(GetCompanyImagesQuery query, CancellationToken cancellationToken)
        {
            var companyImages = await _companyImageRepository.GetAsync(
                e => e.CompanyId == query.CompanyId,
                tracked: false,
                cancellationToken: cancellationToken);

            return new ApiResponse<IEnumerable<CompanyImagesResponse>>
            {
                Success = true,
                Data = companyImages?.Select(MapToResponse) ?? Enumerable.Empty<CompanyImagesResponse>(),
                Message = "Company images retrieved successfully."
            };
        }

        private static CompanyImagesResponse MapToResponse(CompanyImage image)
        {
            return new CompanyImagesResponse
            {
                CompanyId = image.CompanyId,
                ImageUrl = image.ImageUrl,
                IsCover = image.IsCover,
                Caption = image.Caption
            };
        }
    }
}
