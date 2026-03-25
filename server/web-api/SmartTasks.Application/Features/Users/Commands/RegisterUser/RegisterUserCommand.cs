using MediatR;

namespace SmartTasks.Application.Features.Users.Commands.RegisterUser
{
    public record RegisterUserCommand(
        string Email,
        string FirstName,
        string LastName,
        string Password,
        string ConfirmPassword
    ) : IRequest<RegisterUserResponse>;

    public record RegisterUserResponse(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName,
        string Token
    );
}