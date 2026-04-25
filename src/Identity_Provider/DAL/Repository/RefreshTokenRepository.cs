using DAL.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context):base(context) { }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _dbSet.Include(x => x.User).FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeAllUserTokenAsync(int userId)
        {
            await _dbSet.Where(rt => rt.UserId == userId).ForEachAsync(rt => rt.IsRevoked = true);
        }
    }
}
