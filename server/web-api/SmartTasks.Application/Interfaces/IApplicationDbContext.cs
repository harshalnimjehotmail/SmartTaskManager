using Microsoft.EntityFrameworkCore;
using SmartTasks.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTasks.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get;  }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}