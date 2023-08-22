using TransactionAPI.Domain.Enums;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Infrastructure.ViewModels.Transactions
{
    public class TransactionTypeStatusViewModel
    {
        public Type? Type { get; set; }

        public Status? Status { get; set; }

    }
}
