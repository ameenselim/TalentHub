using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JobTypeConfigurations : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Country, x.City });

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Responsibilities)
            .IsRequired();

        builder.Property(x => x.SalaryFrom)
            .HasPrecision(18, 2);

        builder.Property(x => x.SalaryTo)
            .HasPrecision(18, 2);

        builder.Property(x => x.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.JobType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ExperienceLevel)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.WorkMode)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}