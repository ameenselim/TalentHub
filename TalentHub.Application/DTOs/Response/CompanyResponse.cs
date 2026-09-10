namespace TalentHub.Application.DTOs.Response
{
    public class CompanyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string? CoverImage { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public DateOnly? FoundedDate { get; set; }
        public string? Size { get; set; }
        public string? Industry { get; set; }
        public bool IsVerified { get; set; }
    }
}