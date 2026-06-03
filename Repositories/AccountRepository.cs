using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_projekt.Data;
using wpf_projekt.Entities;

namespace wpf_projekt.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<PersonalAccount>> GetAllPersonalAccountsAsync()
            => _context.PersonalAccounts.ToListAsync();

        public Task<List<SharedAccount>> GetAllSharedAccountsAsync()
            => _context.SharedAccounts.ToListAsync();

        public Task<PersonalAccount?> GetPersonalAccountByIdAsync(int id)
            => _context.PersonalAccounts.FirstOrDefaultAsync(a => a.Id == id);

        public Task<SharedAccount?> GetSharedAccountByIdAsync(int id)
            => _context.SharedAccounts.FirstOrDefaultAsync(a => a.Id == id);

        public async Task AddPersonalAccountAsync(PersonalAccount account)
        {
            _context.PersonalAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task AddSharedAccountAsync(SharedAccount account)
        {
            _context.SharedAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePersonalAccountAsync(PersonalAccount account)
        {
            _context.PersonalAccounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSharedAccountAsync(SharedAccount account)
        {
            _context.SharedAccounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public Task<User?> GetFirstUserAsync()
            => _context.Users.FirstOrDefaultAsync();

        public Task<List<PersonalAccount>> GetPersonalAccountsByUserAsync(int userId)
            => _context.PersonalAccounts.Where(a => a.UserId == userId).ToListAsync();

        public Task<List<SharedAccount>> GetSharedAccountsByUserAsync(int userId)
            => _context.SharedAccounts
                .Where(a => a.User1Id == userId || a.User2Id == userId)
                .ToListAsync();
    }
}