
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartTasks.Application.Interfaces;
using SmartTasks.Domain.Exceptions;
using SmartTasks.Domain.Entities;

namespace SmartTasks.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ICacheService _cacheService;

        public RegisterUserCommandHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            ICacheService cacheService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _cacheService = cacheService;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(request.Email);
            }

            // Create new user
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateJwtToken(user.UserId, user.Email, user.Role);

            // Cache user info for quick lookups
            var cacheKey = $"user:{user.UserId}";
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1), cancellationToken);

            return new RegisterUserResponse(
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                token
            );
        }
    }
}