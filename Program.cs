using CampusCare.Data;
using CampusCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
    options.ConfigureWarnings(w =>
        w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// Identity with roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version2;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Seed roles and categories
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Requester", "SupportAgent", "Administrator" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!await db.TicketCategories.AnyAsync())
    {
        db.TicketCategories.AddRange(
            new TicketCategory { Name = "IT Support", Description = "Computers, projectors, network", IsActive = true },
            new TicketCategory { Name = "Electrical", Description = "Lights, sockets, power", IsActive = true },
            new TicketCategory { Name = "Plumbing", Description = "Water, toilets, leaks", IsActive = true },
            new TicketCategory { Name = "Furniture", Description = "Desks, chairs, doors", IsActive = true },
            new TicketCategory { Name = "Cleaning", Description = "Cleanliness and waste", IsActive = true },
            new TicketCategory { Name = "Other", Description = "Anything else", IsActive = true });
        await db.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Error");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();