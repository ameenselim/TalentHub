using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CertificateTypeConfigurations : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Organization)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.CertificateUrl)
            .HasMaxLength(2048);

        builder.HasOne(x => x.Resume)
            .WithMany(x => x.Certificates)
            .HasForeignKey(x => x.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}