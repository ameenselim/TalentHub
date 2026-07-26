using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JobRequirementTypeConfigurations : IEntityTypeConfiguration<JobRequirement>
{
    public void Configure(EntityTypeBuilder<JobRequirement> builder)
    {
        builder.Property(x => x.Requirement)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(x => x.Job)
            .WithMany(x => x.JobRequirements)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}