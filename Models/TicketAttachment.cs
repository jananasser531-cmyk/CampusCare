namespace CampusCare.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UploadedById { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser UploadedByUser { get; set; } = null!;
    }
}
