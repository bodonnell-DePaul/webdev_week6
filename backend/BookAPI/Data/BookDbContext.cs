using BookAPI.Models;
using Microsoft.EntityFrameworkCore;
using BookAPI.Data;

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
            PublisherList.GetPublishers().ToArray()
        );
        
        // Seed data for Book (update with PublisherId)
        modelBuilder.Entity<Book>().HasData(BookList._books);
    }
}
