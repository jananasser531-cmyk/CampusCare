using System.ComponentModel.DataAnnotations;
namespace CampusCare.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string RequesterId { get; set; } = string.Empty;
        public string? AssignedAgentId { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public TicketCategory Category { get; set; } = null!;
        public ApplicationUser Requester { get; set; } = null!;
        public ApplicationUser? AssignedAgent { get; set; }
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
        public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
    }
}
