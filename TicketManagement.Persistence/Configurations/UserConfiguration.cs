using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Persistence.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
