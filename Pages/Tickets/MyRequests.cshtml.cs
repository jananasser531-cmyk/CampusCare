using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CampusCare.Pages.Tickets
{
    [Authorize(Roles = "Requester")]
    public class MyRequestsModel : PageModel
    {
        private const int PageSize = 10;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyRequestsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }
        public List<TicketRow> Tickets { get; set; } = new();

        public class TicketRow
        {
            public int Id { get; set; }
            public string TicketNumber { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            // Only this student's tickets, decided on the server
            var query = _context.Tickets
                .AsNoTracking()
                .Where(t => t.RequesterId == userId);

            var total = await query.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

            Tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(t => new TicketRow
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Subject = t.Subject,
                    CategoryName = t.Category.Name,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();
        }
    }
}