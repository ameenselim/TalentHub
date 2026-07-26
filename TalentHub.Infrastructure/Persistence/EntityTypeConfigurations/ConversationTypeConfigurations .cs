using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConversationTypeConfigurations : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasIndex(x => x.ApplicationId)
            .IsUnique();

        builder.HasOne(x => x.Application)
            .WithOne(x => x.Conversation)
            .HasForeignKey<Conversation>(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}