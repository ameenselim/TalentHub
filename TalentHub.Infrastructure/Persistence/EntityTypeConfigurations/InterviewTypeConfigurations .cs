using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InterviewTypeConfigurations : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.MeetingLink)
            .HasMaxLength(2048);

        builder.Property(x => x.Location)
            .HasMaxLength(300);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);
    }
}