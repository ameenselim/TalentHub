using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ResumeTypeConfigurations : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.Resumes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Property(x => x.ExpectedSalary)
            .HasPrecision(18, 2);

        builder.Property(x => x.PortfolioUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.GithubUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.LinkedinUrl)
            .HasMaxLength(2048);
    }
}