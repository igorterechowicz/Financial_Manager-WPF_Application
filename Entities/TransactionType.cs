using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_projekt.Entities
{
    public class TransactionType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } // np. "Zakupy", "Wypłata"

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
