using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Enums;

namespace TransactionAPI.Infrastructure.ViewModels.Transactions
{
    public class TransactionTypeStatusViewModel
    {
        public Domain.Enums.Type? Type { get; set; }

        public Status? Status { get; set; }

    }
}
