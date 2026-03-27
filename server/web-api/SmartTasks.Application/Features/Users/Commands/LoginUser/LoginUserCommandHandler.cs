using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartTasks.Application.Interfaces;

namespace SmartTasks.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICacheService _cacheService;
    public LoginUserCommandHandler(IApplicationDbContext applicationDbContext,
                                IPasswordHasher passwordHasher,
                                IJwtTokenGenerator jwtTokenGenerator,
                                ICacheService cacheService)
    {
        _applicationDbContext = applicationDbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _cacheService = cacheService;
    }

    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _applicationDbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Generate JWT token 
        var token = _jwtTokenGenerator.GenerateJwtToken(user.UserId, user.Email, user.Role);

        // upodate cache 
        var cacheKey = $"user:{user.UserId}";
        await _cacheService.SetAsync(cacheKey,
                                     new { user.UserId, user.Email, user.FirstName, user.LastName, user.Role },
                                      TimeSpan.FromHours(1), cancellationToken);

        return new LoginUserResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            token
        );
    }
}
