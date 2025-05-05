using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Book> Books { get; set; }
    public DbSet<Publisher> Publishers { get; set; } // Add this line
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity properties
        modelBuilder.Entity<Book>()
            .Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        modelBuilder.Entity<Book>()
            .Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);
            
        // Configure relationship
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId);

        
        // Seed data for Publisher
        modelBuilder.Entity<Publisher>().HasData(
            new Publisher { Id = 1, Name = "Penguin Books", Location = "London" },
            new Publisher { Id = 2, Name = "HarperCollins", Location = "New York" }
        );
        
        // Seed data for Book (update with PublisherId)
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "To Kill a Mockingbird", Author = "Harper Lee", Year = 1960, Genre = "Fiction", IsAvailable = true, PublisherId = 1 },
            new Book { Id = 2, Title = "1984", Author = "George Orwell", Year = 1949, Genre = "Dystopian", IsAvailable = true, PublisherId = 1 },
            new Book { Id = 3, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Year = 1925, Genre = "Classic", IsAvailable = true, PublisherId = 2 }
        );
    }
}
