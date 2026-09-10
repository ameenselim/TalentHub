
using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Category :BaseEntity
    {
        public string Name { get; set; } = null!;

        public string? Icon { get; set; }
        public string? IconPublicId { get; set; }

        //Navigation properties
        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();
    }
}
