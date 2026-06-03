using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_projekt.Entities;

namespace wpf_projekt.Repositories
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetAllWithDetailsAsync();
        Task<List<Transaction>> GetAllWithDetailsByUserAsync(int userId);
        Task AddAsync(Transaction transaction);
        Task AddRangeAsync(IEnumerable<Transaction> transactions);
    }
}
