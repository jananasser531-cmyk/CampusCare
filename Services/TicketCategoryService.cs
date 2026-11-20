using CampusCare.Data;
using CampusCare.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusCare.Services
{
    public class TicketCategoryService
    {
        private readonly ApplicationDbContext _context;

        public TicketCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Get only active categories
        public async Task<List<TicketCategory>> GetActiveCategoriesAsync()
        {
            return await _context.TicketCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // 2. Get all categories
        public async Task<List<TicketCategory>> GetAllCategoriesAsync()
        {
            return await _context.TicketCategories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // 3. Add a new category
        public async Task<TicketCategory> AddCategoryAsync(
            string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required.");

            name = name.Trim();

            bool exists = await _context.TicketCategories
                .AnyAsync(c => c.Name == name);

            if (exists)
                throw new InvalidOperationException(
                    "This category already exists.");

            var category = new TicketCategory
            {
                Name = name,
                Description = description?.Trim() ?? string.Empty,
                IsActive = true
            };

            _context.TicketCategories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        // 4. Update an existing category
        public async Task<bool> UpdateCategoryAsync(
            int id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required.");

            var category = await _context.TicketCategories.FindAsync(id);

            if (category == null)
                return false;

            name = name.Trim();

            bool duplicate = await _context.TicketCategories
                .AnyAsync(c => c.Name == name && c.Id != id);

            if (duplicate)
                throw new InvalidOperationException(
                    "Another category already has this name.");

            category.Name = name;
            category.Description = description?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return true;
        }

        // 5. Deactivate a category
        public async Task<bool> DeactivateCategoryAsync(int id)
        {
            var category = await _context.TicketCategories.FindAsync(id);

            if (category == null)
                return false;

            category.IsActive = false;

            await _context.SaveChangesAsync();

            return true;
        }

        // 6. Validate that a category exists and is active
        public async Task<bool> ValidateCategoryAsync(int categoryId)
        {
            return await _context.TicketCategories
                .AnyAsync(c => c.Id == categoryId && c.IsActive);
        }

        // 7. Validate ticket priority
        public bool ValidatePriority(TicketPriority priority)
        {
            return Enum.IsDefined(typeof(TicketPriority), priority);
        }
    }
}