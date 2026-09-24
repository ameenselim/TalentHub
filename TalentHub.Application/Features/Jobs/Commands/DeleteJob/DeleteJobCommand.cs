using MediatR;
using System;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.Jobs.Commands.DeleteJob
{
    public record DeleteJobCommand(string UserId, int JobId) : IRequest<ApiResponse<bool>>;

    public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        public DeleteJobCommandHandler(IRepository<Job> jobRepository, IRepository<CompanyMember> companyMemberRepository)
        {
            _jobRepository = jobRepository;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteJobCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var jobId = command.JobId;

            var job = await _jobRepository.GetOneAsync(e => e.Id == jobId && !e.IsDeleted, cancellationToken: cancellationToken);
            if (job is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Job not found"
                };
            }
            if (!await IsAuthorizedAsync(userId, job.CompanyId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You are not authorized to delete this job."
                };
            }
            try
            {
                job.IsDeleted = true;
                job.DeletedAt = DateTime.UtcNow;
                _jobRepository.Update(job);
                await _jobRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the job."
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Job deleted successfully",
            };
        }

        private async Task<bool> IsAuthorizedAsync(string userId, int companyId, CancellationToken cancellationToken)
        {
            var member = await _companyMemberRepository.GetOneAsync(m => m.UserId == userId &&
                     m.CompanyId == companyId &&
                     m.IsActive &&
                     (m.Role == CompanyRole.Owner || m.Role == CompanyRole.Admin || m.Role == CompanyRole.Recruiter), tracked: false,
                     cancellationToken: cancellationToken);

            return member != null;
        }
    }
}
