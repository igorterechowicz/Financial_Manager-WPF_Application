using System.ComponentModel;

namespace wpf_projekt.Models
{
    public enum AccountKind
    {
        Personal = 0,
        Shared = 1
    }

    public class AccountListItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public AccountKind Kind { get; set; }
        public string KindLabel => Kind == AccountKind.Personal ? "Osobiste" : "Wspólne";
        public string DisplayName => $"{Name} ({KindLabel}) - saldo: {Balance} zł";

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
