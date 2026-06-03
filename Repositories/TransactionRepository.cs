using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_projekt.Data;
using wpf_projekt.Entities;

namespace wpf_projekt.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Transaction>> GetAllWithDetailsAsync()
            => _context.Transactions
                .Include(t => t.TransactionType)
                .Include(t => t.PersonalAccount)
                .Include(t => t.SharedAccount)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

        public Task<List<Transaction>> GetAllWithDetailsByUserAsync(int userId)
            => _context.Transactions
                .Include(t => t.TransactionType)
                .Include(t => t.PersonalAccount)
                .Include(t => t.SharedAccount)
                .Where(t =>
                    (t.PersonalAccountId != null && t.PersonalAccount!.UserId == userId) ||
                    (t.SharedAccountId != null && (t.SharedAccount!.User1Id == userId || t.SharedAccount.User2Id == userId)))
                .OrderByDescending(t => t.Date)
                .ToListAsync();

        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            _context.Transactions.AddRange(transactions);
            await _context.SaveChangesAsync();
        }
    }
}