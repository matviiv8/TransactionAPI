using TransactionAPI.Domain.Enums;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Infrastructure.ViewModels.Transactions
{
    public class TransactionFilterViewModel
    {
        public List<Type>? Types { get; set; }

        public Status? Status { get; set; }

        public string? ClientName { get; set; }
    }
}
