using wpf_projekt.Entities;

namespace wpf_projekt.Services
{
    public static class AppSession
    {
        public static User? CurrentUser { get; private set; }
        public static bool IsLoggedIn => CurrentUser != null;

        public static void Login(User user) => CurrentUser = user;
        public static void Logout() => CurrentUser = null;
    }
}
