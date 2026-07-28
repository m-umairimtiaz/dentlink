using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Add services to the container (dependency injection).
// ---------------------------------------------------------------------------

builder.Services.AddControllersWithViews();

// Prefer discrete PG* env vars from Render (avoids URI password encoding issues).
// Fall back to DATABASE_URL, then appsettings.json.
static string GetConnectionString(IConfiguration config)
{
    var host = Environment.GetEnvironmentVariable("PGHOST");
    if (!string.IsNullOrEmpty(host))
    {
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgUser = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "";
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE") ?? "postgres";
        return $"Host={host};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Prefer;Trust Server Certificate=true";
    }

    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrEmpty(databaseUrl))
        return config.GetConnectionString("DefaultConnection")!;

    if (!databaseUrl.Contains("://", StringComparison.Ordinal))
        return databaseUrl;

    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var passwordFromUrl = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var databaseFromUrl = uri.AbsolutePath.TrimStart('/');
    var portFromUrl = uri.Port > 0 ? uri.Port : 5432;
    return $"Host={uri.Host};Port={portFromUrl};Database={databaseFromUrl};Username={username};Password={passwordFromUrl};SSL Mode=Prefer;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(GetConnectionString(builder.Configuration)));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Configure the HTTP request pipeline (middleware order matters here).
// ---------------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Do NOT use HTTPS redirection or HSTS on Render: TLS is terminated at the
    // reverse proxy and forcing redirects breaks the open-port health check.
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------------------------------------------------------
// Create the database (if missing) and apply any pending EF Core migrations,
// then seed sample data. This runs once each time the application starts.
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await DbSeeder.SeedAsync(context, scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed failed.");
        throw;
    }
}

app.Run();
