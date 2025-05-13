using System;

namespace BookAPI.Models;

public class AuthModels
{
    // backend/BookAPI/Models/AuthModels.cs
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

}
