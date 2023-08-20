using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransactionAPI.Domain.Enums;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Domain.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int TransactionId { get; set; }

        public string ClientName { get; set; }

        public Status Status { get; set; }

        public Type Type { get; set; }

        public decimal Amount { get; set; }
    }
}
