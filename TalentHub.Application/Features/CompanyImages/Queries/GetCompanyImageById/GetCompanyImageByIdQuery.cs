using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.CompanyImages.Queries.GetCompanyImageById
{
    public record GetCompanyImageByIdQuery(int ImageId, int CompanyId) : IRequest<ApiResponse<CompanyImagesResponse>>;

    public class GetCompanyImageByIdQueryHandler : IRequestHandler<GetCompanyImageByIdQuery, ApiResponse<CompanyImagesResponse>>
    {
        private readonly IRepository<CompanyImage> _companyImageRepository;

        public GetCompanyImageByIdQueryHandler(IRepository<CompanyImage> companyImageRepository)
        {
            _companyImageRepository = companyImageRepository;
        }

        public async Task<ApiResponse<CompanyImagesResponse>> Handle(GetCompanyImageByIdQuery query, CancellationToken cancellationToken)
        {
            var companyImage = await _companyImageRepository.GetOneAsync(
                e => e.Id == query.ImageId && e.CompanyId == query.CompanyId,
                cancellationToken: cancellationToken);

            if (companyImage == null)
            {
                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "Company image not found."
                };
            }

            return new ApiResponse<CompanyImagesResponse>
            {
                Success = true,
                Data = MapToResponse(companyImage),
                Message = "Company image retrieved successfully."
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
