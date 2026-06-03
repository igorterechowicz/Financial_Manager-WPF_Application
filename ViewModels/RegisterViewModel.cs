using CommunityToolkit.Mvvm.ComponentModel;
using wpf_projekt.Entities;
using wpf_projekt.Repositories;
using wpf_projekt.Services;

namespace wpf_projekt.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthService _authService;

        [ObservableProperty] private string _firstName = string.Empty;
        [ObservableProperty] private string _lastName = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isLoading;

        public event Action? RegisterSuccessful;

        public RegisterViewModel(IUserRepository userRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task TryRegisterAsync(string password, string confirmPassword)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Wypełnij wszystkie pola.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessage = "Hasła nie są zgodne.";
                return;
            }

            if (password.Length < 6)
            {
                ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.";
                return;
            }

            IsLoading = true;
            try
            {
                var existing = await _userRepository.GetByEmailAsync(Email.Trim());
                if (existing != null)
                {
                    ErrorMessage = "Konto z tym e-mailem już istnieje.";
                    return;
                }

                var user = new User
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    PasswordHash = _authService.HashPassword(password),
                    Earnings = 0
                };

                await _userRepository.AddAsync(user);
                AppSession.Login(user);
                RegisterSuccessful?.Invoke();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
