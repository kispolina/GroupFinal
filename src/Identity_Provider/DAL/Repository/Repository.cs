using DAL.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class Repository<T>: IRepository<T> where T : class
    {
        public readonly AppDbContext _context;
        public readonly DbSet<T> _dbSet;
        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();  
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

        public async Task DeleteAsync(T entity) => _dbSet.Remove(entity);

        public async Task<T?> FirstOfDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task SaveChangeAsync() => await _context.SaveChangesAsync();

        public async Task UpdateAsync(T entity) => _dbSet.Update(entity);
    }
}
