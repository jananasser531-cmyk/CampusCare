using System.ComponentModel.DataAnnotations;
using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CampusCare.Pages.Tickets
{
    [Authorize(Roles = "Requester")]
    public class CreateModel : PageModel
    {
        private static readonly string[] AllowedPriorities = { "Low", "Medium", "High" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList Categories { get; set; } = null!;
        public string[] Priorities => AllowedPriorities;

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter a subject.")]
            [StringLength(150)]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please describe the problem.")]
            [StringLength(2000)]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please choose a category.")]
            [Display(Name = "Category")]
            public int? CategoryId { get; set; }

            [Required(ErrorMessage = "Please enter the location.")]
            [StringLength(200)]
            public string Location { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please choose a priority.")]
            public string Priority { get; set; } = "Medium";

            [Display(Name = "Image (optional)")]
            public IFormFile? Image { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoriesAsync();

            if (!AllowedPriorities.Contains(Input.Priority))
            {
                ModelState.AddModelError("Input.Priority", "Invalid priority.");
            }

            var categoryExists = await _context.TicketCategories
                .AnyAsync(c => c.Id == Input.CategoryId && c.IsActive);
            if (!categoryExists)
            {
                ModelState.AddModelError("Input.CategoryId", "Invalid category.");
            }

            if (Input.Image != null && Input.Image.Length > 0)
            {
                var ext = Path.GetExtension(Input.Image.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("Input.Image", "Only JPG and PNG images are allowed.");
                }
                else if (Input.Image.Length > MaxImageBytes)
                {
                    ModelState.AddModelError("Input.Image", "The image must be 5 MB or smaller.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User)!;
            var now = DateTime.UtcNow;

            var ticket = new Ticket
            {
                TicketNumber = $"CC-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                Subject = Input.Subject.Trim(),
                Description = Input.Description.Trim(),
                Location = Input.Location.Trim(),
                CategoryId = Input.CategoryId!.Value,
                Priority = Input.Priority,
                Status = "Open",
                RequesterId = userId,
                CreatedAt = now,
                UpdatedAt = now
            };

            ticket.StatusHistory.Add(new TicketStatusHistory
            {
                OldStatus = null,
                NewStatus = "Open",
                ChangedById = userId,
                ChangedAt = now
            });

            if (Input.Image != null && Input.Image.Length > 0)
            {
                var ext = Path.GetExtension(Input.Image.FileName).ToLowerInvariant();
                var storedName = $"{Guid.NewGuid():N}{ext}";
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "tickets");
                Directory.CreateDirectory(folder);

                using (var stream = new FileStream(Path.Combine(folder, storedName), FileMode.Create))
                {
                    await Input.Image.CopyToAsync(stream);
                }

                ticket.Attachments.Add(new TicketAttachment
                {
                    UploadedById = userId,
                    FileName = Path.GetFileName(Input.Image.FileName),
                    FilePath = $"/uploads/tickets/{storedName}",
                    ContentType = Input.Image.ContentType,
                    UploadedAt = now
                });
            }

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Your request {ticket.TicketNumber} was submitted.";
            return RedirectToPage("/Tickets/MyRequests");
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _context.TicketCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            Categories = new SelectList(categories, "Id", "Name");
        }
    }
}