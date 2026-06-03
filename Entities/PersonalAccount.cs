using System.Collections.Generic;
using wpf_projekt.Models;

namespace wpf_projekt.Entities
{
    public class PersonalAccount : ObservableModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged();
                }
            }
        }
        public int UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
