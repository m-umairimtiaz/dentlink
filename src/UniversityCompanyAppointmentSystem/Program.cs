using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Data;
using UniversityCompanyAppointmentSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Add services to the container (dependency injection).
// ---------------------------------------------------------------------------

builder.Services.AddControllersWithViews();

// Render.com injects a DATABASE_URL environment variable (postgres://user:pass@host/db).
// Convert it to an Npgsql connection string when present; otherwise fall back to appsettings.json.
static string GetConnectionString(IConfiguration config)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
    return config.GetConnectionString("DefaultConnection")!;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(GetConnectionString(builder.Configuration)));

// Session needs a backing cache to store its data in; in-memory cache is enough for this simple app.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);           // how long a login stays active without activity
    options.Cookie.HttpOnly = true;                        // JavaScript on the page cannot read the session cookie
    options.Cookie.IsEssential = true;                     // session cookie is required for the app to work (login)
});

// Register our own application services for dependency injection.
// AddScoped = one instance per web request, which matches how DbContext should be used.
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
    app.UseExceptionHandler("/Home/Error");                // show a friendly error page instead of a stack trace
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();                                       // serves wwwroot (css/js/images)

app.UseRouting();

app.UseSession();                                            // must run before UseAuthorization / controllers, so Session is ready

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
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();                              // creates the DB and applies migrations automatically
    await DbSeeder.SeedAsync(context, scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
}

app.Run();
