# CampusCare

CampusCare is a campus maintenance request system built with ASP.NET Core Razor Pages. Students report maintenance problems (for example a broken projector), follow their progress, and support agents and administrators handle the requests.

## Technology

- ASP.NET Core Razor Pages (.NET 10), C#
- SQL Server with Entity Framework Core 10
- ASP.NET Core Identity (authentication and role-based authorization)
- HTML5, CSS3, Bootstrap, JavaScript

## Prerequisites

- Windows with **Visual Studio** (2022 or later) including the **ASP.NET and web development** workload
- **.NET 10 SDK** (installed with the Visual Studio workload)
- **SQL Server Express LocalDB** (Visual Studio installer, Individual components)
- Git

## Database setup

1. Open `appsettings.json` and check the connection string. The default uses LocalDB:

   ```
   Server=(localdb)\MSSQLLocalDB;Database=CampusCareDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
   ```

2. Open **Tools > NuGet Package Manager > Package Manager Console** and run:

   ```
   Update-Database
   ```

   This creates the `CampusCareDB` database and all tables from the migrations in the `Migrations` folder.

3. Each developer has a separate local database. Test accounts and tickets are not shared between machines.

## Running the project

1. Open `CampusCare.sln` in Visual Studio.
2. Make sure the run profile is **https** and press **F5**.
3. The home page opens in your browser. Use **Register** to create a student account.

## Data created automatically at startup

- **Roles:** `Requester` (student), `SupportAgent`, `Administrator`
- **Ticket categories:** IT Support, Electrical, Plumbing, Furniture, Cleaning, Other (only added when the categories table is empty)

Registration always creates a `Requester` account. Agent and administrator accounts are not created by registration.

## Student features

- Register, log in and log out
- Dashboard with request counts and recent activity
- Create a maintenance request (subject, description, category, location, priority, optional JPG/PNG image up to 5 MB)
- My Requests list with pagination
- Ticket Details with status history and attachments

Students can only see their own tickets. The ownership check is done on the server, so changing a ticket number in the address bar returns a 404.

## Project structure

```
CampusCare
  Data/          ApplicationDbContext
  Models/        ApplicationUser, Ticket, TicketCategory, TicketComment,
                 TicketAttachment, TicketStatusHistory
  Migrations/    Entity Framework Core migrations
  Pages/
    Account/     Register, Login, Logout, AccessDenied
    Tickets/     Create, MyRequests, Details
    Shared/      Layout and partial views
  wwwroot/       Static files (uploaded images are saved in wwwroot/uploads)
  Program.cs     Services, Identity, role and category seeding
  appsettings.json
```

## Security notes

- Passwords are hashed by ASP.NET Core Identity
- Forms use anti-forgery tokens (Razor Pages default)
- All input is validated on the server
- Pages are protected with `[Authorize(Roles = "...")]`
- Login errors use one generic message so they do not reveal which emails exist

## Known notes

- Uploaded images are stored in `wwwroot/uploads/tickets` with random file names. This folder is excluded from Git.
- `Program.cs` suppresses EF Core's pending-model-changes warning because of a small difference between the Identity schema and the first migration (`PhoneNumber` column length). It does not affect the application.
