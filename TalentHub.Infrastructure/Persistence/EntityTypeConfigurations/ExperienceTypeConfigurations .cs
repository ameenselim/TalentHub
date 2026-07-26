using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExperienceTypeConfigurations : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.Property(x => x.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Position)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EmploymentType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(x => x.Resume)
            .WithMany(x => x.Experiences)
            .HasForeignKey(x => x.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}