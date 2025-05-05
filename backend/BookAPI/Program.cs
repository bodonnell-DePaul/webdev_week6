using BookAPI.Data;
using BookAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book API", Version = "v1" });
});

// Add EF Core with SQLite
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin() // Vite's default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure middleware
app.UseCors("AllowReactApp");

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers(); 

app.MapGet("/HelloWorld", () =>
{
    return "Hello World!";
})
.WithName("HelloWorld")
.WithOpenApi();

// GET - Get all books
app.MapGet("/api/books", async (BookDbContext db) =>
    await db.Books.ToListAsync())
   .WithName("GetAllBooks");

// GET - Get all books with publishers
app.MapGet("/api/publisherbooks", async (BookDbContext db) =>
     await db.Books.Include(b => b.Publisher).ToListAsync())
     
.WithName("GetAllPublisherBooks");

// GET - Get a specific book by ID
app.MapGet("/api/books/{id}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    return book == null ? Results.NotFound() : Results.Ok(book);
})
.WithName("GetBookById");

// POST - Add a new book
app.MapPost("/api/books", async (Book book, BookDbContext db) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/api/books/{book.Id}", book);
})
.WithName("AddBook");

// PUT - Update a book
app.MapPut("/api/books/{id}", async (int id, Book updatedBook, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Year = updatedBook.Year;
    book.Genre = updatedBook.Genre;
    book.IsAvailable = updatedBook.IsAvailable;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBook");

// PATCH - Update book availability
app.MapPatch("/api/books/{id}/availability", async (int id, bool isAvailable, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.IsAvailable = isAvailable;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBookAvailability");

// DELETE - Delete a book
app.MapDelete("/api/books/{id}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteBook");


app.Run();
