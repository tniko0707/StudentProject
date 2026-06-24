using Application.Repositories;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
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
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> FindByLoginAsync(string login, CancellationToken ct = default)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Login == login, ct);
        }

        public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
