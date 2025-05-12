using BookAPI.Models;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Claims;
using BookAPI.Services;
using BookAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book API", Version = "v1" });
});

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

// // Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Add EF Core with SQLite
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
// Configure basic authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BookApiBasicAuthHandler>("BasicAuthentication", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(ClaimTypes.NameIdentifier);
    });
});
// Add services
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure middleware
app.UseCors("AllowReactApp");

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var bookDbContext = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    bookDbContext.Database.EnsureCreated();

    var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    userDbContext.Database.EnsureCreated();
    User user = new User
    {
        Name = "Admin",
        Email = "admin@bodonnell.com", 
        Password = "password",
        CreatedAt = DateTime.UtcNow
    };
    if (!userDbContext.Users.Any(u => u.Email == user.Email))
    {
        userDbContext.Users.Add(user);
        await userDbContext.SaveChangesAsync();
    }
}
// Add usage before your existing endpoints
app.UseAuthentication();
app.UseAuthorization();

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
     
.WithName("GetAllPublisherBooks").RequireAuthorization();

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
// Add a protected route
// app.MapGet("/api/protected-books", (ClaimsPrincipal user) =>
// {
//     return books;
// })
// .WithName("GetProtectedBooks")
// .RequireAuthorization();

app.Run();
