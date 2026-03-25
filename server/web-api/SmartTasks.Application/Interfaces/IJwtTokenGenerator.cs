using System;

namespace SmartTasks.Application.Interfaces;

public interface IJwtTokenGenerator
{
    String GenerateJwtToken(Guid userId, string email, string role);
}
