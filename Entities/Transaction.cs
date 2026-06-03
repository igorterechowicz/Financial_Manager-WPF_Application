using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using wpf_projekt.Models;

namespace wpf_projekt.Entities
{
    public class Transaction : ObservableModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public bool IsPositive { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Description { get; set; }

        public int? PersonalAccountId { get; set; }

        [ForeignKey("PersonalAccountId")]
        public virtual PersonalAccount PersonalAccount { get; set; }

        public int? SharedAccountId { get; set; }

        [ForeignKey("SharedAccountId")]
        public virtual SharedAccount SharedAccount { get; set; }

        [Required]
        public int TransactionTypeId { get; set; }

        [ForeignKey("TransactionTypeId")]
        public virtual TransactionType TransactionType { get; set; }

        public Guid? TransferGroupId { get; set; }

        [NotMapped]
        public string TypeName => IsPositive ? "Przychód" : "Wydatek";
    }
}
