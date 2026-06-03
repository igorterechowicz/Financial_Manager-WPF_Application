using System.Windows;
using wpf_projekt.Data;
using wpf_projekt.Repositories;
using wpf_projekt.Services;
using wpf_projekt.ViewModels;

namespace wpf_projekt.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _vm;

        public RegisterWindow()
        {
            InitializeComponent();

            var context = new AppDbContext();
            var userRepo = new UserRepository(context);
            var authService = new AuthService(userRepo);
            _vm = new RegisterViewModel(userRepo, authService);

            _vm.RegisterSuccessful += OnRegisterSuccessful;

            DataContext = _vm;
        }

        private void OnRegisterSuccessful()
        {
            var mainWindow = new wpf_projekt.MainWindow();
            mainWindow.Show();
            Close();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await _vm.TryRegisterAsync(PasswordInput.Password, ConfirmPasswordInput.Password);
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
