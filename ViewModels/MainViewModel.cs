using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using wpf_projekt.Data;
using wpf_projekt.Entities;
using wpf_projekt.Models;
using wpf_projekt.Repositories;
using wpf_projekt.Services;

namespace wpf_projekt.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDataErrorInfo
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly AppDbContext _context;
        private readonly IEventLogService _eventLogService;

        //  Sub-ViewModels
        public TransactionsViewModel TransactionsVm { get; }
        public SummaryViewModel SummaryVm { get; }

        //  Kolekcje
        public ObservableCollection<Transaction> Transactions { get; } = new();
        public ObservableCollection<AccountListItem> Accounts { get; } = new();
        public ObservableCollection<TransactionType> Categories { get; } = new();
        public ObservableCollection<EventLog> Logs { get; } = new();

        //  Właściwości bindowane — formularz transakcji (WALIDOWANE)

        private string _amountText = string.Empty;
        public string AmountText
        {
            get => _amountText;
            set => SetProperty(ref _amountText, value);
        }

        [ObservableProperty] private string _descriptionText = string.Empty;
        [ObservableProperty] private DateTime _transactionDate = DateTime.Now;
        [ObservableProperty] private TransactionType? _selectedCategory;
        [ObservableProperty] private AccountListItem? _selectedAccount;
        [ObservableProperty] private bool _isIncome = false;
        [ObservableProperty] private bool _isExpense = true;

        //  Właściwości bindowane — formularz konta
        [ObservableProperty] private string _newAccountName = string.Empty;
        [ObservableProperty] private string _newAccountType = "Osobiste";

        //  Właściwości bindowane — formularz transferu
        [ObservableProperty] private AccountListItem? _transferFrom;
        [ObservableProperty] private AccountListItem? _transferTo;
        [ObservableProperty] private string _transferAmountText = string.Empty;
        [ObservableProperty] private string _transferDescription = string.Empty;

        private readonly User _currentUser;

        //  Konstruktor
        public MainViewModel(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            AppDbContext context,
            IEventLogService eventLogService,
            User currentUser)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _eventLogService = eventLogService;
            _currentUser = currentUser;

            TransactionsVm = new TransactionsViewModel(this, transactionRepository, categoryRepository, accountRepository);
            SummaryVm = new SummaryViewModel(this);
        }

        //  Inicjalizacja
        public async Task InitializeAsync()
        {
            await _categoryRepository.EnsureExistsAsync("Transfer");
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            Categories.Clear();
            Accounts.Clear();
            Transactions.Clear();
            Logs.Clear();

            var dbCategories = await _categoryRepository.GetAllAsync();
            var dbPersonal = await _accountRepository.GetPersonalAccountsByUserAsync(_currentUser.Id);
            var dbShared = await _accountRepository.GetSharedAccountsByUserAsync(_currentUser.Id);
            var dbTransactions = await _transactionRepository.GetAllWithDetailsByUserAsync(_currentUser.Id);

            var dbLogs = await _context.EventLogs.OrderByDescending(x => x.Timestamp).ToListAsync();

            foreach (var c in dbCategories) Categories.Add(c);

            foreach (var a in dbPersonal)
                Accounts.Add(new AccountListItem
                {
                    Id = a.Id,
                    Name = string.IsNullOrWhiteSpace(a.Name) ? $"Konto osobiste #{a.Id}" : a.Name,
                    Balance = a.Balance,
                    Kind = AccountKind.Personal
                });

            foreach (var a in dbShared)
                Accounts.Add(new AccountListItem
                {
                    Id = a.Id,
                    Name = string.IsNullOrWhiteSpace(a.Name) ? $"Konto wspólne #{a.Id}" : a.Name,
                    Balance = a.Balance,
                    Kind = AccountKind.Shared
                });

            foreach (var t in dbTransactions) Transactions.Add(t);
            foreach (var log in dbLogs) Logs.Add(log);
        }

        //  Komendy

        [RelayCommand]
        private async Task SaveTransactionAsync()
        {
            if (!decimal.TryParse(AmountText.Replace('.', ','), out decimal amount))
            {
                MessageBox.Show("Wprowadź poprawną kwotę.");
                return;
            }

            if (SelectedCategory == null || SelectedAccount == null)
            {
                MessageBox.Show("Wybierz kategorię i konto!");
                return;
            }

            var newTransaction = new Transaction
            {
                Amount = amount,
                IsPositive = IsIncome,
                Date = TransactionDate,
                Description = DescriptionText,
                TransactionTypeId = SelectedCategory.Id
            };

            try
            {
                decimal updatedBalance;

                if (SelectedAccount.Kind == AccountKind.Personal)
                {
                    var acc = await _accountRepository.GetPersonalAccountByIdAsync(SelectedAccount.Id);
                    if (acc == null) { MessageBox.Show("Nie znaleziono konta osobistego."); return; }
                    acc.Balance += IsIncome ? amount : -amount;
                    newTransaction.PersonalAccountId = acc.Id;
                    await _accountRepository.UpdatePersonalAccountAsync(acc);
                    updatedBalance = acc.Balance;
                }
                else
                {
                    var acc = await _accountRepository.GetSharedAccountByIdAsync(SelectedAccount.Id);
                    if (acc == null) { MessageBox.Show("Nie znaleziono konta wspólnego."); return; }
                    acc.Balance += IsIncome ? amount : -amount;
                    newTransaction.SharedAccountId = acc.Id;
                    await _accountRepository.UpdateSharedAccountAsync(acc);
                    updatedBalance = acc.Balance;
                }

                await _transactionRepository.AddAsync(newTransaction);

                await _eventLogService.LogAsync(
                EventType.TransactionAdded,
                $"Dodano transakcję: {amount} zł",
                newTransaction.Id);

                await LoadDataAsync();
                MessageBox.Show($"Zapisano! Aktualne saldo: {updatedBalance:F2} zł");
                ClearTransactionForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddAccountAsync()
        {
            var name = NewAccountName.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Podaj nazwę konta.");
                return;
            }

            await _eventLogService.LogAsync(
            EventType.AccountCreated,
            $"Utworzono konto: {name}");

            if (NewAccountType == "Wspólne")
                await _accountRepository.AddSharedAccountAsync(new SharedAccount
                {
                    Name = name,
                    Balance = 0m,
                    User1Id = _currentUser.Id,
                    User2Id = _currentUser.Id
                });
            else
                await _accountRepository.AddPersonalAccountAsync(new PersonalAccount
                { Name = name, Balance = 0m, UserId = _currentUser.Id });

            await LoadDataAsync();
            NewAccountName = string.Empty;
        }

        [RelayCommand]
        private async Task ExecuteTransferAsync()
        {
            if (!decimal.TryParse(TransferAmountText.Replace('.', ','), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Podaj poprawną kwotę transferu.");
                return;
            }
            if (TransferFrom == null || TransferTo == null)
            {
                MessageBox.Show("Wybierz konto źródłowe i docelowe.");
                return;
            }
            if (TransferFrom.Kind == TransferTo.Kind && TransferFrom.Id == TransferTo.Id)
            {
                MessageBox.Show("Wybierz dwa różne konta.");
                return;
            }
            if (TransferFrom.Balance < amount)
            {
                MessageBox.Show(
                    $"Niewystarczające środki.\n" +
                    $"Dostępne: {TransferFrom.Balance:F2} zł\n" +
                    $"Brakuje: {(amount - TransferFrom.Balance):F2} zł");
                return;
            }

            var transferType = await _categoryRepository.GetByNameAsync("Transfer");
            if (transferType == null) { MessageBox.Show("Nie można wykonać transferu."); return; }

            var groupId = Guid.NewGuid();
            var desc = string.IsNullOrWhiteSpace(TransferDescription)
                ? "Transfer między kontami" : TransferDescription.Trim();

            var outgoing = new Transaction
            {
                Amount = amount,
                IsPositive = false,
                Date = DateTime.Now,
                TransactionTypeId = transferType.Id,
                Description = $"{desc} (wyjście)",
                TransferGroupId = groupId
            };
            var incoming = new Transaction
            {
                Amount = amount,
                IsPositive = true,
                Date = DateTime.Now,
                TransactionTypeId = transferType.Id,
                Description = $"{desc} (wejście)",
                TransferGroupId = groupId
            };

            if (TransferFrom.Kind == AccountKind.Personal)
            {
                var acc = await _accountRepository.GetPersonalAccountByIdAsync(TransferFrom.Id);
                if (acc == null) { MessageBox.Show("Błąd transferu."); return; }
                acc.Balance -= amount;
                outgoing.PersonalAccountId = acc.Id;
                await _accountRepository.UpdatePersonalAccountAsync(acc);
            }
            else
            {
                var acc = await _accountRepository.GetSharedAccountByIdAsync(TransferFrom.Id);
                if (acc == null) { MessageBox.Show("Błąd transferu."); return; }
                acc.Balance -= amount;
                outgoing.SharedAccountId = acc.Id;
                await _accountRepository.UpdateSharedAccountAsync(acc);
            }

            if (TransferTo.Kind == AccountKind.Personal)
            {
                var acc = await _accountRepository.GetPersonalAccountByIdAsync(TransferTo.Id);
                if (acc == null) { MessageBox.Show("Błąd transferu."); return; }
                acc.Balance += amount;
                incoming.PersonalAccountId = acc.Id;
                await _accountRepository.UpdatePersonalAccountAsync(acc);
            }
            else
            {
                var acc = await _accountRepository.GetSharedAccountByIdAsync(TransferTo.Id);
                if (acc == null) { MessageBox.Show("Błąd transferu."); return; }
                acc.Balance += amount;
                incoming.SharedAccountId = acc.Id;
                await _accountRepository.UpdateSharedAccountAsync(acc);
            }

            await _transactionRepository.AddRangeAsync(new[] { outgoing, incoming });
            await _eventLogService.LogAsync(
            EventType.TransferCompleted,
            $"Transfer {amount} zł z {TransferFrom.Name} do {TransferTo.Name}");

            await LoadDataAsync();
            TransferAmountText = string.Empty;
            TransferDescription = string.Empty;
            MessageBox.Show("Transfer wykonany.");
        }

        [RelayCommand]
        private async Task OpenAddCategoryAsync()
        {
            var window = new wpf_projekt.Views.AddCategoryWindow();
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true)
                await LoadDataAsync();
        }

        //  Metody pomocnicze
        private void ClearTransactionForm()
        {
            AmountText = string.Empty;
            DescriptionText = string.Empty;
            TransactionDate = DateTime.Now;
        }


        // --- LOGIKA WALIDACJI (IDataErrorInfo) ---
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(AmountText))
                {
                    if (string.IsNullOrWhiteSpace(AmountText)) return null;

                    string normalized = AmountText.Replace('.', ',');
                    if (!decimal.TryParse(normalized, out decimal result))
                    {
                        return "Nieprawidłowy format kwoty (np. wpisz 100,50)";
                    }

                    if (normalized.Contains(",") && normalized.Substring(normalized.IndexOf(",") + 1).Length > 2)
                    {
                        return "Maksymalnie 2 miejsca po przecinku";
                    }

                    if (result <= 0) return "Kwota musi być większa od 0";
                }
                return null;
            }
        }
    }
}
