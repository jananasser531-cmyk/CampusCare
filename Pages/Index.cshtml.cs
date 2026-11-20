using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CampusCare.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public bool IsRequester { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int WaitingCount { get; set; }
        public List<RecentTicket> Recent { get; set; } = new();

        public class RecentTicket
        {
            public int Id { get; set; }
            public string TicketNumber { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime UpdatedAt { get; set; }
        }

        public async Task OnGetAsync()
        {
            IsRequester = User.IsInRole("Requester");
            if (!IsRequester)
            {
                return;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return;
            }

            FullName = user.FullName;

            // Only this student's tickets
            var mine = _context.Tickets
                .AsNoTracking()
                .Where(t => t.RequesterId == user.Id);

            var counts = await mine
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            TotalCount = counts.Sum(c => c.Count);
            OpenCount = counts.Where(c => c.Status == TicketStatuses.Open).Sum(c => c.Count);
            InProgressCount = counts.Where(c => c.Status == TicketStatuses.InProgress).Sum(c => c.Count);
            WaitingCount = counts.Where(c => c.Status == TicketStatuses.WaitingForRequester).Sum(c => c.Count);
            ResolvedCount = counts.Where(c => c.Status == TicketStatuses.Resolved || c.Status == TicketStatuses.Closed).Sum(c => c.Count);

            Recent = await mine
                .OrderByDescending(t => t.UpdatedAt)
                .Take(5)
                .Select(t => new RecentTicket
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Subject = t.Subject,
                    Status = t.Status,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();
        }
    }
}