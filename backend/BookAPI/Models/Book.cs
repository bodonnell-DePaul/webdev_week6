using System;

namespace BookAPI.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool AudioBookAvailable { get; set; }  = false;// New property
    public int? PublisherId { get; set; } // New property Publisher FK

    // Navigation property
    public Publisher? Publisher { get; set; }
}
