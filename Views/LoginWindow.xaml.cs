using System.Windows;
using wpf_projekt.Data;
using wpf_projekt.Repositories;
using wpf_projekt.Services;
using wpf_projekt.ViewModels;

namespace wpf_projekt.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

        public LoginWindow()
        {
            InitializeComponent();

            var context = new AppDbContext();
            var userRepo = new UserRepository(context);
            var authService = new AuthService(userRepo);
            _vm = new LoginViewModel(authService);

            _vm.LoginSuccessful += OnLoginSuccessful;

            DataContext = _vm;
        }

        private void OnLoginSuccessful()
        {
            var mainWindow = new wpf_projekt.MainWindow();
            mainWindow.Show();
            Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.LoginCommand.Execute(PasswordInput.Password);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            Close();
        }
    }
}
