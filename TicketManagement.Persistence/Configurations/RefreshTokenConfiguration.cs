using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder) {
            builder.ToTable("RefreshTokens");
            builder.HasKey(rt => rt.Id);
            
            builder.Property(rt => rt.TokenHash).HasMaxLength(512).IsRequired();
            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.HasIndex(rt => rt.SessionId);

            builder.HasOne<User>().WithMany().HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
        }

    }
}
