using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyFollowerTypeConfigurations : IEntityTypeConfiguration<CompanyFollower>
{
    public void Configure(EntityTypeBuilder<CompanyFollower> builder)
    {

        builder.HasIndex(x => new { x.UserId, x.CompanyId })
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany(x => x.CompanyFollowers)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Followers)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}