using Microsoft.EntityFrameworkCore;
using Users.Application.Repositories;
using Users.Domain.Models;
using Users.Infrastructure.DataAccess;

namespace Users.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<User> AddAsync(User user, CancellationToken ct = default)
        {
            _db.Add(user);
            await _db.SaveChangesAsync(ct);
            return user;
        }

        public async Task<User?> GetByLoginAsync(string login, CancellationToken ct = default)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Login == login, ct);
        }
        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }
        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        }

 
    }
}
