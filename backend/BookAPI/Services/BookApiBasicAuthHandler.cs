using System;

namespace BookAPI.Services;

using BookAPI.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

public class BookApiBasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserService _userService;

    public BookApiBasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserService userService) : base(options, logger, encoder, clock)
    {
        _userService = userService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (authHeader.Scheme != "Basic")
                return AuthenticateResult.Fail("Invalid authentication scheme");

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            var securePassword = authHeader.Parameter;
            var username = credentials[0];
            var password = credentials[1];

            // For demo purposes - replace with your actual user store
            using(var db = new UserDbContext())
            {
                try{
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == username && u.Password == securePassword);
                }
                catch (Exception ex)
                {
                    return AuthenticateResult.Fail("Database error: " + ex.Message);
                }
                
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, username),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }
}

// Simple user service interface
public interface IUserService
{
    Task<bool> ValidateUser(string username, string password);
}

public class UserService : IUserService
{
    // In a real app, this would validate against a database
    public Task<bool> ValidateUser(string username, string password)
    {
        // Demo implementation
        return Task.FromResult(username == "admin" && password == "password");
    }
}