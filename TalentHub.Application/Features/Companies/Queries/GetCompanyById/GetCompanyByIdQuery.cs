using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Companies.Queries.GetCompanyById
{
    public record GetCompanyByIdQuery(int CompanyId) : IRequest<ApiResponse<CompanyResponse>>;

    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, ApiResponse<CompanyResponse>>
    {
        private readonly IRepository<Company> _companyRepository;

        public GetCompanyByIdQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<ApiResponse<CompanyResponse>> Handle(GetCompanyByIdQuery query, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetOneAsync(e => e.Id == query.CompanyId && !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
            if (company is null)
            {
                return new ApiResponse<CompanyResponse>
                {
                    Success = false,
                    Message = "Company not found"
                };
            }

            return new ApiResponse<CompanyResponse>
            {
                Success = true,
                Data = MapToResponse(company),
                Message = "Company retrieved successfully"
            };
        }

        private static CompanyResponse MapToResponse(Company company)
        {
            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Logo = company.Logo,
                CoverImage = company.CoverImage,
                Website = company.Website,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber,
                Country = company.Country,
                City = company.City,
                Address = company.Address,
                FoundedDate = company.FoundedDate,
                Size = company.Size,
                Industry = company.Industry,
                IsVerified = company.IsVerified
            };
        }
    }
}
