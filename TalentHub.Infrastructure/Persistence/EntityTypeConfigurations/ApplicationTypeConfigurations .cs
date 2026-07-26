using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApplicationTypeConfigurations : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.JobId })
            .IsUnique();

        builder.HasIndex(x => new { x.JobId, x.Status });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne<ApplicationUser>()
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Job)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Resume)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}