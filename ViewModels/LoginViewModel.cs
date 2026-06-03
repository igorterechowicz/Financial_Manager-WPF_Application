using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using wpf_projekt.Services;

namespace wpf_projekt.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isLoading;

        public event Action? LoginSuccessful;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync(string password)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Podaj e-mail i hasło.";
                return;
            }

            IsLoading = true;
            try
            {
                var user = await _authService.AuthenticateAsync(Email.Trim(), password);
                if (user == null)
                {
                    ErrorMessage = "Nieprawidłowy e-mail lub hasło.";
                    return;
                }

                AppSession.Login(user);
                LoginSuccessful?.Invoke();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
