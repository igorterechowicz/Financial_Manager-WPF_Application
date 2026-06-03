using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using wpf_projekt.Entities;
using wpf_projekt.Models;

namespace wpf_projekt.ViewModels
{
    public partial class SummaryViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVm;

        // ── Filtry ───────────────────────────────────────────────────────────────
        public ObservableCollection<object> AvailableAccounts { get; } = new();
        public ObservableCollection<string> AvailableYears { get; } = new();
        public ObservableCollection<MonthItem> AvailableMonths { get; } = new();

        [ObservableProperty] private object? _selectedAccount;
        [ObservableProperty] private string _selectedYear = "Wszystkie";
        [ObservableProperty] private MonthItem? _selectedMonth;

        // ── Wartości podsumowania ────────────────────────────────────────────────
        [ObservableProperty] private string _incomeText = "0,00 zł";
        [ObservableProperty] private string _expenseText = "0,00 zł";
        [ObservableProperty] private string _balanceText = "0,00 zł";
        [ObservableProperty] private Brush _balanceBrush = Brushes.Green;

        public SummaryViewModel(MainViewModel mainVm)
        {
            _mainVm = mainVm;
            _mainVm.Transactions.CollectionChanged += (_, _) => Refresh();
        }

        public void Load()
        {
            BuildFilters();
            Calculate();
        }

        private void BuildFilters()
        {
            // KONTA
            AvailableAccounts.Clear();
            AvailableAccounts.Add("Wszystkie");
            foreach (var acc in _mainVm.Accounts)
                AvailableAccounts.Add(acc);
            SelectedAccount = AvailableAccounts[0];

            // LATA
            AvailableYears.Clear();
            AvailableYears.Add("Wszystkie");
            foreach (var year in _mainVm.Transactions
                .Select(t => t.Date.Year).Distinct().OrderByDescending(y => y))
                AvailableYears.Add(year.ToString());
            SelectedYear = "Wszystkie";

            // MIESIĄCE
            AvailableMonths.Clear();
            AvailableMonths.Add(new MonthItem(null, "Wszystkie"));
            for (int i = 1; i <= 12; i++)
            {
                var name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                AvailableMonths.Add(new MonthItem(i, char.ToUpper(name[0]) + name[1..]));
            }
            SelectedMonth = AvailableMonths[0];
        }

        partial void OnSelectedAccountChanged(object? value) => Calculate();
        partial void OnSelectedYearChanged(string value) => Calculate();
        partial void OnSelectedMonthChanged(MonthItem? value) => Calculate();

        private void Calculate()
        {
            var data = _mainVm.Transactions.AsEnumerable();

            // Filtr konta
            if (SelectedAccount is AccountListItem acc)
            {
                data = acc.Kind == AccountKind.Personal
                    ? data.Where(t => t.PersonalAccountId == acc.Id)
                    : data.Where(t => t.SharedAccountId == acc.Id);
            }

            // Filtr roku
            if (SelectedYear != "Wszystkie" && !string.IsNullOrEmpty(SelectedYear))
                data = data.Where(t => t.Date.Year.ToString() == SelectedYear);

            // Filtr miesiąca
            if (SelectedMonth?.Number != null)
                data = data.Where(t => t.Date.Month == SelectedMonth.Number);

            var list = data.ToList();
            var income = list.Where(t => t.IsPositive).Sum(t => t.Amount);
            var expense = list.Where(t => !t.IsPositive).Sum(t => t.Amount);
            var balance = income - expense;

            IncomeText = $"{income:F2} zł";
            ExpenseText = $"{expense:F2} zł";
            BalanceText = $"{balance:F2} zł";
            BalanceBrush = balance >= 0 ? Brushes.Green : Brushes.Red;
        }

        private void Refresh()
        {
            BuildFilters();
            Calculate();
        }
    }
}