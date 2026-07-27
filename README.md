# University – Company Employee Appointment System

A simple ASP.NET Core MVC web application that lets **Companies** book group
appointments with **Universities** for a set of selected **Employees**.

This project is intentionally kept small and beginner-friendly: no ASP.NET
Identity, no roles/claims, no email verification, no external services. Just
a plain session-based login, EF Core + SQL Server, Bootstrap, and straightforward
MVC controllers/views with comments explaining the non-obvious parts.

---

## 1. Project Purpose

- **Universities** register an account, publish available appointment time
  slots (date, time, maximum employees), and confirm/reject/complete/cancel
  appointment requests from companies.
- **Companies** register an account, manage their own list of employees,
  select multiple employees at once, and book **one group appointment**
  with a university for all of them.
- Both sides get simple in-website **notifications** whenever something
  relevant happens (submitted / confirmed / rejected / cancelled / completed).

## 2. Technologies Used

- ASP.NET Core MVC (.NET 8, C#)
- Entity Framework Core 8 (Code First + Migrations)
- Microsoft SQL Server / SQL Server Express
- Bootstrap 5 (already included via LibMan under `wwwroot/lib`)
- Plain HTML, CSS and vanilla JavaScript (no React/Angular/Vue)
- ASP.NET Core Session for the simple login system

## 3. Required Software

- **Visual Studio 2022** (17.8+) with the **ASP.NET and web development** workload
- **.NET 8 SDK**
- **SQL Server** (Express, Developer, or full edition) or **SQL Server LocalDB**
- **SQL Server Management Studio (SSMS)** to view/manage the database
- The `dotnet-ef` global tool if you want to run migrations from the command
  line: `dotnet tool install --global dotnet-ef`

## 4. How to Open the Project in Visual Studio

1. Open `UniversityCompanyAppointmentSystem.sln` (in the repository root) with Visual Studio.
2. Visual Studio will restore the NuGet packages automatically. If not, right‑click
   the solution → **Restore NuGet Packages**.
3. Set `UniversityCompanyAppointmentSystem` (under `src/`) as the startup project
   (it should already be the only project in the solution).

## 5. Configuring SQL Server / Changing the Connection String

The connection string lives in
`src/UniversityCompanyAppointmentSystem/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=UniversityCompanyAppointmentDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

- `Server=localhost` assumes a **default** SQL Server instance on your machine.
- If you installed **SQL Server Express**, the instance is usually named
  `SQLEXPRESS`, so use: `Server=localhost\SQLEXPRESS;...`
- If you use **LocalDB**, use: `Server=(localdb)\mssqllocaldb;...`
- If SQL Server is on another machine, replace `localhost` with its name or IP.

> This repository also ships an `appsettings.Development.json` pre-configured
> with `Server=localhost\SQLEXPRESS;...`, because that is what was available
> in the environment this project was built and tested in. When you run the
> app in **Development** (the default when pressing F5 in Visual Studio), that
> file's connection string wins. Edit it (or `appsettings.json`) to match your
> own SQL Server instance name.

## 6. Creating the Database

The app uses **EF Core Code First Migrations**. The `InitialCreate` migration
is already included in `src/UniversityCompanyAppointmentSystem/Migrations`.

**Option A — it happens automatically.** `Program.cs` calls
`context.Database.Migrate()` on startup, so simply running the app (F5, or
`dotnet run`) creates the database and applies all migrations for you.

**Option B — run migrations manually first**, using either the Visual Studio
Package Manager Console or the .NET CLI:

Package Manager Console (Visual Studio):
```powershell
Add-Migration InitialCreate
Update-Database
```

.NET CLI (from the `src/UniversityCompanyAppointmentSystem` folder):
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Once created, open **SQL Server Management Studio (SSMS)**, connect to your
server, and you will see the `UniversityCompanyAppointmentDb` database with
all 7 tables plus the `__EFMigrationsHistory` table EF Core uses internally.

## 7. Sample Seed Data

The first time the app runs, `Data/DbSeeder.cs` inserts:

- 1 University, 1 Company, 5 Employees (for that company), and 4 appointment
  slots (3 Available, 1 Unavailable) so there is something to explore right away.

### Sample Login Details

| Account Type | Email                | Password        |
|--------------|-----------------------|------------------|
| University   | `admin@ku.edu.kw`     | `University@123` |
| Company      | `hr@gulftech.com`     | `Company@123`     |

## 8. Running the Website

- **Visual Studio:** press **F5** (or Ctrl+F5 for no debugging). The browser
  opens automatically at the URL configured in `Properties/launchSettings.json`
  (e.g. `http://localhost:5187`).
- **CLI:** from `src/UniversityCompanyAppointmentSystem`, run `dotnet run`.

## 9. Main Database Tables

| Table                 | Purpose                                                        |
|-----------------------|-----------------------------------------------------------------|
| `Universities`        | University accounts                                              |
| `Companies`           | Company accounts                                                  |
| `Employees`           | Employees, each belonging to one Company                          |
| `AppointmentSlots`    | Time slots a University publishes for booking                     |
| `Appointments`        | One row per group booking (Company + University + Slot)           |
| `AppointmentEmployees`| Many‑to‑many join table linking Appointments ↔ Employees          |
| `Notifications`       | In‑website notifications for Companies and Universities           |

## 10. Main Project Folders

```
src/UniversityCompanyAppointmentSystem/
├── Controllers/     Account, dashboards, employees, appointments, slots, notifications
├── Models/          EF Core entities (University, Company, Employee, Appointment, ...)
├── ViewModels/      Form/view-specific models used instead of binding entities directly
├── Data/            ApplicationDbContext + DbSeeder
├── Services/        AppointmentService, EmployeeService, NotificationService, PasswordHasher
├── Views/           Razor views, organised per controller, sharing _Layout.cshtml
├── Migrations/      EF Core Code First migrations
└── wwwroot/         css/site.css (colour palette), js/site.js, Bootstrap/jQuery libs
```

## 11. How Employee Selection & Group Booking Works

1. A Company logs in and goes to **Employees**, where all of its own employees
   are listed with checkboxes (`Views/Employees/Index.cshtml`).
2. The company ticks the employees it wants (e.g. 10 employees) and clicks
   **"Book Appointment for Selected Employees"**. This sends the selected
   employee IDs to `GET /Appointments/Book?employeeIds=1&employeeIds=2...`.
3. On the **Book Appointment** page, the company picks a **University**; this
   triggers a small AJAX call to `AppointmentSlotsController.GetAvailableSlots`
   which returns that university's open slots (with remaining seats) as JSON.
   Unavailable / full slots are shown disabled and greyed out.
4. The company can remove any employee from the list before confirming
   (this only removes them from *this* booking, not from the Employees list).
5. On **Confirm Appointment**, `AppointmentService.BookAppointmentAsync`
   re-validates everything server-side (slot still available, capacity not
   exceeded, employees really belong to this company), then creates **one**
   `Appointment` row plus one `AppointmentEmployee` row per selected employee
   — so 10 selected employees produce 1 appointment linked to 10 employees,
   never 10 separate appointments.
6. Notifications are created for both the company and the university, and the
   university can then Confirm / Reject the appointment (and later mark it
   Completed or Cancel it) from the Appointment Details page.

---

Built as a learning-friendly example project — see the inline code comments
throughout the Controllers, Services and Models for explanations of each step.
