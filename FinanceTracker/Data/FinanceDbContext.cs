using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data
{
    public class FinanceDbContext : DbContext
    {
        // DbSet for storing expenses
        public DbSet<Expense> Expenses { get; set; }

        // DbSet for storing categories
        public DbSet<Category> Categories { get; set; }

        // DbSet for storing monthly budget
        public DbSet<MonthlyBudget> MonthlyBudgets { get; set; }

        // Configure the SQLite database
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "finance.db");
            options.UseSqlite($"Data Source={path}");
        }

        // Seed initial categories
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food" },
                new Category { Id = 2, Name = "Transport" },
                new Category { Id = 3, Name = "Entertainment" },
                new Category { Id = 4, Name = "Bills" },
                new Category { Id = 5, Name = "Other" }
            );
        }
    }
}