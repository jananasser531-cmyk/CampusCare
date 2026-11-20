using CampusCare.Data;
using CampusCare.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CampusCare.Services
{
    public class TicketCommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketCommentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<TicketComment> AddCommentAsync(
            int ticketId,
            string content,
            ClaimsPrincipal currentUser,
            bool isInternal = false)
        {
            if (currentUser == null ||
                currentUser.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(
                    "You must be logged in to comment.");
            }

            var user = await _userManager.GetUserAsync(currentUser);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException(
                    "Your account is invalid or inactive.");

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException(
                    "Comment content is required.");

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                throw new InvalidOperationException(
                    "Ticket not found.");

            if (ticket.Status == TicketStatuses.Closed)
                throw new InvalidOperationException(
                    "Cannot add comments to a closed ticket.");

            bool isRequester = ticket.RequesterId == user.Id;

            bool isAdmin = await _userManager
                .IsInRoleAsync(user, "Administrator");

            bool isAssignedAgent =
                ticket.AssignedAgentId == user.Id &&
                await _userManager.IsInRoleAsync(user, "SupportAgent");

            if (!isRequester && !isAdmin && !isAssignedAgent)
                throw new UnauthorizedAccessException(
                    "You are not allowed to comment on this ticket.");

            if (isInternal && !isAdmin && !isAssignedAgent)
                throw new UnauthorizedAccessException(
                    "Only support agents and administrators can add internal comments.");

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = user.Id,
                Content = content.Trim(),
                IsInternal = isInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);

            await _context.SaveChangesAsync();

            return comment;
        }
    }
}
