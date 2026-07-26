using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TalentHub.API.Data.EntityTypeConfigurations
{
    public class ApplicationUserTypeConfigurations : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.FirstName)
           .HasMaxLength(100)
           .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Country)
                .HasMaxLength(100);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.Headline)
                .HasMaxLength(200);

            builder.Property(x => x.Bio)
                .HasMaxLength(4000);
        }
    }
}
