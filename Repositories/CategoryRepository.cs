using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_projekt.Data;
using wpf_projekt.Entities;

namespace wpf_projekt.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<TransactionType>> GetAllAsync()
            => _context.TransactionTypes.ToListAsync();

        public Task<TransactionType?> GetByNameAsync(string name)
            => _context.TransactionTypes.FirstOrDefaultAsync(t => t.Name == name);

        public async Task AddAsync(TransactionType category)
        {
            _context.TransactionTypes.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task EnsureExistsAsync(string name)
        {
            bool exists = await _context.TransactionTypes.AnyAsync(t => t.Name == name);
            if (!exists)
            {
                _context.TransactionTypes.Add(new TransactionType { Name = name });
                await _context.SaveChangesAsync();
            }
        }
    }
}