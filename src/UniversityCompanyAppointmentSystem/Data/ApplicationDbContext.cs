using Microsoft.EntityFrameworkCore;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.Data
{
    // The EF Core database context: represents a session with the database and
    // exposes one DbSet<T> per table. EF Core uses this class (plus the
    // OnModelCreating configuration below) to generate the database schema.
    public class ApplicationDbContext : DbContext
    {
        // The constructor just forwards the options (connection string, provider, etc.)
        // that Program.cs configures, to the base DbContext class.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // One DbSet per table in the database.
        public DbSet<University> Universities => Set<University>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppointmentEmployee> AppointmentEmployees => Set<AppointmentEmployee>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- University ----------
            modelBuilder.Entity<University>()
                .HasIndex(u => u.Email)          // Email must be unique so it can be used to log in
                .IsUnique();

            // ---------- Company ----------
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.Email)          // Email must be unique so it can be used to log in
                .IsUnique();

            // ---------- Employee ----------
            // An employee number must be unique, but only within the SAME company
            // (two different companies are allowed to use the same employee number).
            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.CompanyId, e.EmployeeNumber })
                .IsUnique();

            // Same rule for Civil ID: unique within the same company only.
            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.CompanyId, e.CivilId })
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)                       // each Employee belongs to one Company
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);             // deleting a company removes its employees

            // ---------- AppointmentSlot ----------
            modelBuilder.Entity<AppointmentSlot>()
                .HasOne(s => s.University)                    // each slot belongs to one University
                .WithMany(u => u.AppointmentSlots)
                .HasForeignKey(s => s.UniversityId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Appointment ----------
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentReference)
                .IsUnique();                                   // reference number must be unique across the system

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.University)                    // each appointment is with one University
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);            // do not cascade-delete appointments if university row removed

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Company)                       // each appointment is made by one Company
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.AppointmentSlot)               // each appointment is booked into one slot
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.AppointmentSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- AppointmentEmployee (many-to-many join table) ----------
            modelBuilder.Entity<AppointmentEmployee>()
                .HasOne(ae => ae.Appointment)
                .WithMany(a => a.AppointmentEmployees)
                .HasForeignKey(ae => ae.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);             // deleting an appointment removes its employee links

            modelBuilder.Entity<AppointmentEmployee>()
                .HasOne(ae => ae.Employee)
                .WithMany(e => e.AppointmentEmployees)
                .HasForeignKey(ae => ae.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);             // deleting an employee removes their appointment links

            // Prevent the same employee from being added twice to the same appointment.
            modelBuilder.Entity<AppointmentEmployee>()
                .HasIndex(ae => new { ae.AppointmentId, ae.EmployeeId })
                .IsUnique();

            // ---------- Notification ----------
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.University)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UniversityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Company)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Appointment)
                .WithMany(a => a.Notifications)
                .HasForeignKey(n => n.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
