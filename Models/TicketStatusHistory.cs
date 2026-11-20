namespace CampusCare.Models
{
    public class TicketStatusHistory
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string? OldStatus { get; set; }

        public string NewStatus { get; set; } = string.Empty;

        public string ChangedById { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; }

        // Optional explanation for the status change
        public string? Note { get; set; }

        public Ticket Ticket { get; set; } = null!;

        public ApplicationUser ChangedByUser { get; set; } = null!;
    }
}