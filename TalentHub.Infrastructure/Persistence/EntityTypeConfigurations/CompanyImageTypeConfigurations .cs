using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyImageTypeConfigurations : IEntityTypeConfiguration<CompanyImage>
{
    public void Configure(EntityTypeBuilder<CompanyImage> builder)
    {
        builder.Property(x => x.Caption)
            .HasMaxLength(500);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}