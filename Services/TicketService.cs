using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampusCare.Services
{
    public class TicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // TASK 5: Get active support agents
        public async Task<List<ApplicationUser>> GetAvailableAgentsAsync()
        {
            var agents = await _userManager.GetUsersInRoleAsync("SupportAgent");

            return agents
                .Where(agent => agent.IsActive)
                .ToList();
        }

        // TASK 5: Create a new ticket
        public async Task<Ticket> CreateTicketAsync(
            string subject,
            string description,
            string location,
            int categoryId,
            string requesterId,
            TicketPriority priority)
        {
            bool categoryIsValid = await _context.TicketCategories
                .AnyAsync(c => c.Id == categoryId && c.IsActive);

            if (!categoryIsValid)
                throw new InvalidOperationException(
                    "The selected category is invalid or inactive.");

            if (!Enum.IsDefined(typeof(TicketPriority), priority))
                throw new ArgumentException("Invalid ticket priority.");

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.");

            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location is required.");

            if (string.IsNullOrWhiteSpace(requesterId))
                throw new ArgumentException("Requester is required.");

            var requester = await _userManager.FindByIdAsync(requesterId);

            if (requester == null || !requester.IsActive)
                throw new InvalidOperationException(
                    "The requester is invalid or inactive.");

            var ticket = new Ticket
            {
                TicketNumber = "TKT-" + Guid.NewGuid().ToString("N"),
                Subject = subject.Trim(),
                Description = description.Trim(),
                Location = location.Trim(),
                CategoryId = categoryId,
                RequesterId = requesterId,
                Priority = priority,
                Status = TicketStatuses.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);

            // Record the initial status
            var history = new TicketStatusHistory
            {
                Ticket = ticket,
                OldStatus = null,
                NewStatus = TicketStatuses.Open,
                ChangedById = requesterId,
                ChangedAt = DateTime.UtcNow,
                Note = "Ticket created."
            };

            _context.TicketStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            return ticket;
        }

        // TASK 5: Assign a ticket to a support agent
        public async Task<bool> AssignTicketAsync(
            int ticketId,
            string agentId,
            ClaimsPrincipal currentUser)
        {
            await GetAuthorizedAdministratorAsync(currentUser);

            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID is required.");

            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return false;

            if (ticket.AssignedAgentId != null)
                throw new InvalidOperationException(
                    "This ticket is already assigned. Use reassignment instead.");

            var agent = await _userManager.FindByIdAsync(agentId);

            if (agent == null ||
                !agent.IsActive ||
                !await _userManager.IsInRoleAsync(agent, "SupportAgent"))
            {
                throw new InvalidOperationException(
                    "The selected agent is invalid or inactive.");
            }

            ticket.AssignedAgentId = agent.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // TASK 5: Reassign a ticket
        public async Task<bool> ReassignTicketAsync(
            int ticketId,
            string newAgentId,
            ClaimsPrincipal currentUser)
        {
            await GetAuthorizedAdministratorAsync(currentUser);

            if (string.IsNullOrWhiteSpace(newAgentId))
                throw new ArgumentException("Agent ID is required.");

            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return false;

            if (ticket.AssignedAgentId == null)
                throw new InvalidOperationException(
                    "This ticket has not been assigned yet.");

            var agent = await _userManager.FindByIdAsync(newAgentId);

            if (agent == null ||
                !agent.IsActive ||
                !await _userManager.IsInRoleAsync(agent, "SupportAgent"))
            {
                throw new InvalidOperationException(
                    "The selected agent is invalid or inactive.");
            }

            ticket.AssignedAgentId = agent.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // TASK 6A: Staff update ticket status
        public async Task<bool> UpdateTicketStatusAsync(
            int ticketId,
            string newStatus,
            ClaimsPrincipal currentUser,
            string? note = null,
            string? resolutionSummary = null)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return false;

            var user = await GetAuthorizedStaffAsync(ticket, currentUser);

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("New status is required.");

            if (!IsValidStaffTransition(ticket.Status, newStatus))
                throw new InvalidOperationException(
                    "This status transition is not allowed.");

            if (newStatus == TicketStatuses.Resolved &&
                string.IsNullOrWhiteSpace(resolutionSummary))
            {
                throw new ArgumentException(
                    "A resolution summary is required.");
            }

            string oldStatus = ticket.Status;
            DateTime now = DateTime.UtcNow;

            // Update the current ticket status
            ticket.Status = newStatus;
            ticket.UpdatedAt = now;

            if (newStatus == TicketStatuses.Resolved)
            {
                ticket.ResolvedAt = now;
                ticket.ResolutionSummary = resolutionSummary!.Trim();
            }

            // Record the status change
            var history = new TicketStatusHistory
            {
                TicketId = ticket.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedById = user.Id,
                ChangedAt = now,
                Note = note
            };

            _context.TicketStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            return true;
        }

        // TASK 6B: Requester confirms that the ticket is resolved
        public async Task<bool> ConfirmResolutionAsync(
            int ticketId,
            ClaimsPrincipal currentUser,
            string? note = null)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return false;

            var requester = await GetAuthorizedRequesterAsync(
                ticket, currentUser);

            if (ticket.Status != TicketStatuses.Resolved)
                throw new InvalidOperationException(
                    "Only resolved tickets can be closed.");

            await ChangeStatusAsync(
                ticket,
                TicketStatuses.Closed,
                requester.Id,
                note);

            return true;
        }

        // TASK 6C: Requester reopens a ticket
        public async Task<bool> ReopenTicketAsync(
            int ticketId,
            ClaimsPrincipal currentUser,
            string? note = null)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return false;

            var requester = await GetAuthorizedRequesterAsync(
                ticket, currentUser);

            if (ticket.Status != TicketStatuses.Resolved &&
                ticket.Status != TicketStatuses.Closed)
            {
                throw new InvalidOperationException(
                    "Only resolved or closed tickets can be reopened.");
            }

            await ChangeStatusAsync(
                ticket,
                TicketStatuses.InProgress,
                requester.Id,
                note ?? "Ticket reopened by requester.");

            ticket.ResolvedAt = null;
            ticket.ResolutionSummary = null;

            await _context.SaveChangesAsync();

            return true;
        }

        // TASK 6D: Retrieve complete status history
        public async Task<List<TicketStatusHistory>>
            GetTicketStatusHistoryAsync(int ticketId)
        {
            return await _context.TicketStatusHistories
                .Where(h => h.TicketId == ticketId)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();
        }

        // Save a status change and its history together
        private async Task ChangeStatusAsync(
            Ticket ticket,
            string newStatus,
            string changedById,
            string? note)
        {
            string oldStatus = ticket.Status;
            DateTime now = DateTime.UtcNow;

            ticket.Status = newStatus;
            ticket.UpdatedAt = now;

            var history = new TicketStatusHistory
            {
                TicketId = ticket.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedById = changedById,
                ChangedAt = now,
                Note = note
            };

            _context.TicketStatusHistories.Add(history);

            await _context.SaveChangesAsync();
        }

        // Check allowed staff status transitions
        private bool IsValidStaffTransition(
            string currentStatus,
            string newStatus)
        {
            if (newStatus == TicketStatuses.InProgress &&
                (currentStatus == TicketStatuses.Open ||
                 currentStatus == TicketStatuses.WaitingForRequester))
                return true;

            if (newStatus == TicketStatuses.WaitingForRequester &&
                currentStatus == TicketStatuses.InProgress)
                return true;

            if (newStatus == TicketStatuses.Resolved &&
                currentStatus == TicketStatuses.InProgress)
                return true;

            return false;
        }

        // Verify assigned agent or active administrator
        private async Task<ApplicationUser> GetAuthorizedStaffAsync(
            Ticket ticket,
            ClaimsPrincipal currentUser)
        {
            if (currentUser == null ||
                currentUser.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(
                    "You must be logged in.");
            }

            var user = await _userManager.GetUserAsync(currentUser);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException(
                    "Your account is invalid or inactive.");

            bool isAdmin =
                await _userManager.IsInRoleAsync(user, "Administrator");

            if (isAdmin)
                return user;

            bool isAgent =
                await _userManager.IsInRoleAsync(user, "SupportAgent");

            if (!isAgent || ticket.AssignedAgentId != user.Id)
                throw new UnauthorizedAccessException(
                    "Only the assigned agent or an administrator can update this ticket.");

            return user;
        }

        // Verify ticket requester
        private async Task<ApplicationUser> GetAuthorizedRequesterAsync(
            Ticket ticket,
            ClaimsPrincipal currentUser)
        {
            if (currentUser == null ||
                currentUser.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(
                    "You must be logged in.");
            }

            var user = await _userManager.GetUserAsync(currentUser);

            if (user == null ||
                !user.IsActive ||
                ticket.RequesterId != user.Id)
            {
                throw new UnauthorizedAccessException(
                    "Only the requester can perform this action.");
            }

            return user;
        }

        // Verify administrator
        private async Task<ApplicationUser> GetAuthorizedAdministratorAsync(
            ClaimsPrincipal currentUser)
        {
            if (currentUser == null ||
                currentUser.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(
                    "You must be logged in.");
            }

            var administrator = await _userManager.GetUserAsync(currentUser);

            if (administrator == null ||
                !administrator.IsActive ||
                !await _userManager.IsInRoleAsync(
                    administrator, "Administrator"))
            {
                throw new UnauthorizedAccessException(
                    "Only active administrators can perform this action.");
            }

            return administrator;
        }
    }
}