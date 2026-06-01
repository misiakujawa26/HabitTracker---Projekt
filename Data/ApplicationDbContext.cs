using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Models;

namespace HabitTracker.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Habit> Habits { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<HabitLog> HabitLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Zdrowie", Color = "#FF5733" },
            new Category { Id = 2, Name = "Sport", Color = "#33FF57" },
            new Category { Id = 3, Name = "Nauka", Color = "#3357FF" },
            new Category { Id = 4, Name = "Inne", Color = "#999999" }
        );
    }
}
