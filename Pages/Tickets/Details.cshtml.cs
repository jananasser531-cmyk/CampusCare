using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CampusCare.Pages.Tickets
{
    [Authorize(Roles = "Requester")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Ticket Ticket { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            // The ownership check is part of the query itself:
            // a ticket that belongs to someone else is simply not found.
            var ticket = await _context.Tickets
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.Category)
                .Include(t => t.AssignedAgent)
                .Include(t => t.Attachments)
                .Include(t => t.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(t => t.Id == id && t.RequesterId == userId);

            if (ticket == null)
            {
                return NotFound();
            }

            Ticket = ticket;
            return Page();
        }
    }
}