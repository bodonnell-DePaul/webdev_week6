using System;
using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=users.db");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<User>().Property(u => u.Name).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<User>().Property(u => u.Password).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).IsRequired();
    }

}