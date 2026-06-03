using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using wpf_projekt.Data;
using wpf_projekt.Entities;
using wpf_projekt.Repositories;
using wpf_projekt.Services;
using wpf_projekt.ViewModels;


namespace wpf_projekt
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {

            InitializeComponent();

            var context = App.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventLogService = new EventLogService(context);
            var accountRepo = new AccountRepository(context);
            var transactionRepo = new TransactionRepository(context);
            var categoryRepo = new CategoryRepository(context);

            _viewModel = new MainViewModel(accountRepo, transactionRepo, categoryRepo, context, eventLogService, AppSession.CurrentUser!);
            DataContext = _viewModel;

            var user = AppSession.CurrentUser!;
            UserNameBlock.Text = $"Zalogowany: {user.FirstName} {user.LastName}";

            Loaded += async (_, _) => await _viewModel.InitializeAsync();
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AppSession.Logout();
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
            Close();
        }
    }

}
