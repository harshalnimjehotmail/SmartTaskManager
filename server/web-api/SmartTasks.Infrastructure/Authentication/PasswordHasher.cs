using System;
using SmartTasks.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace SmartTasks.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<object> _passwordHasher;
    public PasswordHasher()
    {
        _passwordHasher = new PasswordHasher<object>();
    }
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), hashedPassword, password);
        return result == PasswordVerificationResult.Success || 
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
