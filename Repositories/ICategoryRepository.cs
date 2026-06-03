using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_projekt.Entities;

namespace wpf_projekt.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<TransactionType>> GetAllAsync();
        Task<TransactionType?> GetByNameAsync(string name);
        Task AddAsync(TransactionType category);
        Task EnsureExistsAsync(string name);
    }
}