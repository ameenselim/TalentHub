using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TalentHub.Application.Common.Extensions;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Companies.Queries.GetAllCompanies
{
    public record GetAllCompaniesQuery(CompanyFilterRequest CompanyRequest) : IRequest<ApiResponse<PagedResponse<CompanyListResponse>>>;

    public class GetAllCompaniesQueryHandler : IRequestHandler<GetAllCompaniesQuery, ApiResponse<PagedResponse<CompanyListResponse>>>
    {
        private readonly IRepository<Company> _companyRepository;

        public GetAllCompaniesQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<ApiResponse<PagedResponse<CompanyListResponse>>> Handle(GetAllCompaniesQuery query, CancellationToken cancellationToken)
        {
            var companyRequest = query.CompanyRequest;
            Expression<Func<Company, bool>> filter = e => !e.IsDeleted;
            if (!string.IsNullOrWhiteSpace(companyRequest.Name))
            {
                filter = filter.And(e => EF.Functions.Like(e.Name, $"%{companyRequest.Name.Trim()}%"));
            }
            if (!string.IsNullOrEmpty(companyRequest.Country))
            {
                filter = filter.And(e => e.Country == companyRequest.Country);
            }
            if (!string.IsNullOrEmpty(companyRequest.City))
            {
                filter = filter.And(e => e.City == companyRequest.City);
            }
            if (!string.IsNullOrEmpty(companyRequest.Industry))
            {
                filter = filter.And(e => e.Industry == companyRequest.Industry);
            }
            if (!string.IsNullOrEmpty(companyRequest.Size))
            {
                filter = filter.And(e => e.Size == companyRequest.Size);
            }
            if (companyRequest.IsVerified.HasValue)
            {
                filter = filter.And(e => e.IsVerified == companyRequest.IsVerified.Value);
            }

            int totalCount = await _companyRepository.CountAsync(filter, cancellationToken);
            int totalPages = (int)Math.Ceiling((double)totalCount / companyRequest.PageSize);

            var companies = await _companyRepository.GetAsync(
                expression: filter,
                pageSize: companyRequest.PageSize,
                page: companyRequest.PageNumber,
                tracked: false,
                cancellationToken: cancellationToken,
                include: q => q.Include(c => c.Followers).Include(c => c.Jobs)
            );

            var pagedData = new PagedResponse<CompanyListResponse>
            {
                Data = companies.Adapt<IEnumerable<CompanyListResponse>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = companyRequest.PageSize,
                PageNumber = companyRequest.PageNumber
            };

            return new ApiResponse<PagedResponse<CompanyListResponse>>
            {
                Success = true,
                Message = "Companies retrieved successfully",
                Data = pagedData
            };
        }
    }
}
