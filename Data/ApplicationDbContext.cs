using Microsoft.EntityFrameworkCore;
using CountriesAPI.Models;

namespace CountriesAPI.Data;

/// <summary>
/// This class is our bridge to the database
/// DbContext manages the connection and tracks changes to our data
/// </summary>
public class ApplicationDbContext : DbContext
{
    // Constructor receives options (like connection string) from Program.cs
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)  // Pass options to the parent class (DbContext)
    {
    }

    // DbSet = a table in your database
    // This line creates a "Countries" table
    public DbSet<Country> Countries { get; set; }

    /// <summary>
    /// Configure how our models map to database tables
    /// This method is called when the database is being created
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the Country entity
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.id);
            // Make country names unique (no duplicates allowed)
            entity.HasIndex(e => e.name).IsUnique();

            // Set Name as required with max length
            entity.Property(e => e.name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.region);
            entity.HasIndex(e => e.currency_code);

            // Configure decimal precision
            // HasPrecision(18, 4) means: 18 total digits, 4 after decimal point
            entity.Property(e => e.exchange_rate)
                .HasPrecision(18, 4);

            entity.Property(e => e.estimated_gdp)
                .HasPrecision(20, 2);  // GDP can be very large

        });
    }
}