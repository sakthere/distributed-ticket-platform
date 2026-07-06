using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Persistence.Configurations
{
    internal class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title).HasMaxLength(100).IsRequired();
            builder.Property(t => t.Description).HasMaxLength(1000).IsRequired();
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.Priority);
            builder.HasIndex(t => t.CreatedByUserId);
            builder.HasIndex(t => t.AssignedToUserId);

            builder.HasOne(x => x.CreatedByUser).WithMany(x => x.CreatedTickets).
                HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByUser).WithMany(x => x.AssignedTickets).
                HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
