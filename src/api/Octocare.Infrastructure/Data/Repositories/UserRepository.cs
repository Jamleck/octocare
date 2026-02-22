using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly OctocareDbContext _db;

    public UserRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, CancellationToken ct = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
