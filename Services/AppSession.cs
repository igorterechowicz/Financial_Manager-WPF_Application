using wpf_projekt.Entities;
using wpf_projekt.Models;

namespace wpf_projekt.Services
{
    public static class AppSession
    {
        public static User? CurrentUser { get; private set; }
        public static bool IsLoggedIn => CurrentUser != null;

        public static AccountListItem? CurrentAccount { get; private set; }

        public static void Login(User user) => CurrentUser = user;
        public static void Logout()
        {
            CurrentUser = null;
            CurrentAccount = null;
        }

        public static void SetCurrentAccount(AccountListItem? account) => CurrentAccount = account;
    }
}
