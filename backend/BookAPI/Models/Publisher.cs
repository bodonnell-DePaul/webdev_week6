namespace BookAPI.Models;

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    // Navigation property
    [System.Text.Json.Serialization.JsonIgnore] 
    public ICollection<Book> Books { get; set; } = new List<Book>();
}