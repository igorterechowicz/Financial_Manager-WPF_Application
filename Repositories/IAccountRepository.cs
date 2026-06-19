using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_projekt.Entities;

namespace wpf_projekt.Repositories
{
    public interface IAccountRepository
    {
        Task<List<PersonalAccount>> GetAllPersonalAccountsAsync();
        Task<List<SharedAccount>> GetAllSharedAccountsAsync();
        Task<PersonalAccount?> GetPersonalAccountByIdAsync(int id);
        Task<SharedAccount?> GetSharedAccountByIdAsync(int id);
        Task AddPersonalAccountAsync(PersonalAccount account);
        Task AddSharedAccountAsync(SharedAccount account);
        Task UpdatePersonalAccountAsync(PersonalAccount account);
        Task UpdateSharedAccountAsync(SharedAccount account);
        Task<User?> GetFirstUserAsync();
        Task<List<PersonalAccount>> GetPersonalAccountsByUserAsync(int userId);
        Task<List<SharedAccount>> GetSharedAccountsByUserAsync(int userId);
        Task DeletePersonalAccountAsync(int id);
        Task ConvertSharedToPersonalAsync(int sharedAccountId, int leavingUserId);
    }
}