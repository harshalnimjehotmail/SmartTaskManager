using System;
using MediatR;

namespace SmartTasks.Application.Features.Users.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;

public record LoginUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Token
);
