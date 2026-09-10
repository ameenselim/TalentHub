namespace TalentHub.Application.DTOs.Request
{
    public class CompanyFilterRequest : PaginationRequest
    {
        public string? Name { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? Industry { get; set; }

        public string? Size { get; set; }

        public bool? IsVerified { get; set; }
    }
}