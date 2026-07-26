using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EducationTypeConfigurations : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.Property(x => x.University)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Faculty)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Degree)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Grade)
            .HasMaxLength(50);

        builder.HasOne(x => x.Resume)
            .WithMany(x => x.Educations)
            .HasForeignKey(x => x.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}