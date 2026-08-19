using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Infrastructure.Services
{
    
    public class ProfileServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly ILogger<ProfileServices> _logger;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        private readonly IRepository<Company> _companyRepository;

        public ProfileServices(UserManager<ApplicationUser> userManager,IRepository<ApplicationUser> userRepository, ILogger<ProfileServices> logger, IRepository<CompanyMember> companyMemberRepository, IRepository<Company> companyRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
            _companyMemberRepository = companyMemberRepository;
            _companyRepository = companyRepository;
        }

        public async Task<ApiResponse<UserProfileResponse>> GetUserProfileAsync(string userId)
        {
            var user = await _userRepository.GetOneAsync(e => e.Id == userId, tracked: false,
                include: q => q.Include(e => e.CompanyMemberships)
                        .ThenInclude(q => q.Company));
            if (user is null)
            {
                return new ApiResponse<UserProfileResponse>("User not found.", new()
                {
                    "The specified user does not exist."
                });
            }
            var profileResponse = new UserProfileResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                ProfileImage = user.ProfileImage,
                Headline = user.Headline,
                Bio = user.Bio,
                Country = user.Country,
                City = user.City,
                Companies = user.CompanyMemberships
                .Where(x => x.IsActive)
                .Select(x => new CompanyMembershipResponse
                {
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.Name,
                    Logo = x.Company.Logo,
                    Role = x.Role.ToString()
                }).ToList()
            };
            return new ApiResponse<UserProfileResponse>(profileResponse, "Profile retrieved successfully.");
        }
        public async Task<ApiResponse<CompanyProfileResponse>> GetCompanyProfileAsync(int companyId)
        {
            var company = await _companyRepository.GetOneAsync(e => e.Id == companyId && !e.IsDeleted, tracked: false,
                include:q=>q.Include(e=>e.Images));
            if (company is null)
            {
                return new ApiResponse<CompanyProfileResponse>("Company not found.", new()
                {
                    "The specified company does not exist."
                });
            }
            var companyResponse = new CompanyProfileResponse()
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Logo = company.Logo,
                Website = company.Website,
                Email = company.Email,
                Country = company.Country,
                Size =company.Size,
                City = company.City,
                Industry = company.Industry,
                
                IsVerified = company.IsVerified,
                Images = (IReadOnlyCollection<CompanyImage>)company.Images.Select(x => new CompanyImagesResponse
                {
                    CompanyId =x.CompanyId,
                    ImageUrl = x.ImageUrl,
                    Caption = x.Caption,
                    IsCover = x.IsCover
                }).ToList()
            };
            return new ApiResponse<CompanyProfileResponse>(companyResponse,"Company retrieved successfully.");
        }
        
    }
}
