using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CampusCare.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketStatusHistory> TicketStatusHistories { get; set; }
        // Database relationships
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ticket -> Category
            builder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Requester
            builder.Entity<Ticket>()
                .HasOne(t => t.Requester)
                .WithMany(u => u.RequestedTickets)
                .HasForeignKey(t => t.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Assigned agent
            builder.Entity<Ticket>()
                .HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Comments
            builder.Entity<TicketComment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment -> User
            builder.Entity<TicketComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Attachments
            builder.Entity<TicketAttachment>()
                .HasOne(a => a.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Attachment -> Uploader
            builder.Entity<TicketAttachment>()
                .HasOne(a => a.UploadedByUser)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(a => a.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Status history
            builder.Entity<TicketStatusHistory>()
                .HasOne(h => h.Ticket)
                .WithMany(t => t.StatusHistory)
                .HasForeignKey(h => h.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Status history -> User
            builder.Entity<TicketStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany(u => u.StatusChanges)
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket number must be unique
            builder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();
        }
    }
}
