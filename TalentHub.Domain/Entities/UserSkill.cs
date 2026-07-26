using TalentHub.Domain.Enums.UserSkill;

namespace TalentHub.Domain.Entities
{
    public class UserSkill
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public int SkillId { get; set; }

        public SkillLevel Level { get; set; }

        public int YearsOfExperience { get; set; }

        // Navigation Properties

        public Skill Skill { get; set; } = null!;
    }
}
