using ContactsAPI.Entities;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace ContactsAPI.Data
{
    /// <summary>
    /// Entity Framework Core Database Context.
    /// This class acts as the bridge between the domain entities and the SQL Server database.
    /// </summary>
    public class ContactsDbContext : DbContext
    {
        public ContactsDbContext(DbContextOptions<ContactsDbContext> options) : base(options) { }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactExtraField> ContactExtraFields { get; set; }
        public DbSet<ExtraFieldDefinition> ExtraFieldDefinitions { get; set; }
        public DbSet<ExtraFieldOption> ExtraFieldOptions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AdminConfig> AdminConfigs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactExtraField>()
                .HasKey(ef => ef.ExtraFieldId);
            modelBuilder.Entity<ContactExtraField>()
                .HasOne(ef => ef.Contact)
                .WithMany(c => c.ExtraFields)
                .HasForeignKey(ef => ef.ContactId);
            modelBuilder.Entity<ContactExtraField>()
                .HasOne(ef => ef.Definition)
                .WithMany(d => d.ContactExtraFields)
                .HasForeignKey(ef => ef.ExtraFieldDefinitionId);
            modelBuilder.Entity<ExtraFieldOption>()
                .HasOne(o => o.Definition)
                .WithMany(d => d.Options)
                .HasForeignKey(o => o.ExtraFieldDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            // Enum conversion for Contact.Status
            modelBuilder.Entity<Contact>()
                .Property(c => c.Status)
                .HasConversion<string>();
            // Enum conversion for User.Status
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();
            // Enum conversion for ExtraFieldDefinition.FieldType
            // Stores "Text", "Email", "Option", etc. in the DB instead of integers
            modelBuilder.Entity<ExtraFieldDefinition>()
                .Property(e => e.FieldType)
                .HasConversion<string>();
            // Auto-populate CreatedDate with SQL default
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<Contact>()
                .Property(c => c.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            // Seed Admin user
            // IMPORTANT: Use a hardcoded hash here. If you use BCrypt.HashPassword() directly in OnModelCreating,
            // EF Core will generate a new hash every time it evaluates the model, causing endless pending migrations.
            // This hash corresponds to the password: "Password@123"
            var defaultPasswordHash = "$2a$11$QVYI9saOk91TpM/cbARh6e3or7Ujlsuk.sRSHxKGHhNl4awZ1eXYm";
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = defaultPasswordHash,
                    Role = "Admin",
                    Status = UserStatus.Active,
                    CreatedDate = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 2,
                    Username = "editor",
                    Email = "editor@example.com",
                    PasswordHash = defaultPasswordHash,
                    Role = "Editor",
                    Status = UserStatus.Active,
                    CreatedDate = new DateTime(2026, 5, 28, 0, 0, 0, DateTimeKind.Utc)
                }
            );
            // Seed AdminConfigs
            modelBuilder.Entity<AdminConfig>().HasData(
                new AdminConfig
                {
                    Id = 1,
                    Key = "EnableAuditLogging",
                    Value = "true",
                    Description = "Toggle audit logging on/off"
                },
                new AdminConfig
                {
                    Id = 2,
                    Key = "MaxExtraFieldsPerContact",
                    Value = "5",
                    Description = "Maximum number of extra fields allowed per contact"
                }
            );
        }
    }
}