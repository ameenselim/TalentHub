using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TalentHub.API.Data.EntityTypeConfigurations
{
    public class CompanyTypeConfigurations : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasIndex(x => x.Name);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(4000);

            builder.Property(x => x.Logo)
                .HasMaxLength(2048);

            builder.Property(x => x.CoverImage)
                .HasMaxLength(2048);

            builder.Property(x => x.Website)
                .HasMaxLength(2048);

            builder.Property(x => x.Email)
                .HasMaxLength(256);

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            builder.Property(x => x.Country)
                .HasMaxLength(100);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.Address)
                .HasMaxLength(300);

            builder.Property(x => x.Industry)
                .HasMaxLength(150);
        }
    }
}
