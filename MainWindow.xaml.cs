using System.Windows;
using System.Windows.Input;
using wpf_projekt.models;
using wpf_projekt.Repositories;
using wpf_projekt.ViewModels;
using wpf_projekt.Services;


namespace wpf_projekt
{  
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var context = new AppDbContext();

            // NOWE
            var eventLogService = new Services.EventLogService(context);

            var accountRepo = new AccountRepository(context);
            var transactionRepo = new TransactionRepository(context);
            var categoryRepo = new CategoryRepository(context);

            // NOWE — przekazujemy eventLogService
            _viewModel = new MainViewModel(
                context,
                accountRepo,
                transactionRepo,
                categoryRepo,
                eventLogService);

            DataContext = _viewModel;

            Loaded += async (_, _) => await _viewModel.InitializeAsync();
        }

        // Jedyna pozostała metoda w code-behind – walidacja inputu klawiszowego
        // Nie zawiera logiki biznesowej, więc może zostać tutaj.
        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".";
        }
    }

}