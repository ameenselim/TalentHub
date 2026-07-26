using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Skill :BaseEntity
    {
        public string Name { get; set; } = null!;

        //Navigation properties
        public ICollection<JobSkill> JobSkills { get; set; } = new HashSet<JobSkill>();

        public ICollection<UserSkill> UserSkills { get; set; } = new HashSet<UserSkill>();
    }
}
