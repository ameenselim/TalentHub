using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApplicationUserOTPTypeConfigurations : IEntityTypeConfiguration<ApplicationUserOTP>
{
    public void Configure(EntityTypeBuilder<ApplicationUserOTP> builder)
    {
        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.ApplicationUserOTPs)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.OTP).HasMaxLength(6);
    }
}