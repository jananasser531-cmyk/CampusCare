using Microsoft.AspNetCore.Identity;
namespace CampusCare.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<Ticket> RequestedTickets { get; set; } = new List<Ticket>();
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAttachment> UploadedAttachments { get; set; } = new List<TicketAttachment>();
        public ICollection<TicketStatusHistory> StatusChanges { get; set; } = new List<TicketStatusHistory>();
    }
}
