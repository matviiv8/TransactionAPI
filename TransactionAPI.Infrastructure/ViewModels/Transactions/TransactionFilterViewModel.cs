using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Enums;

namespace TransactionAPI.Infrastructure.ViewModels.Transactions
{
    public class TransactionFilterViewModel
    {
        public List<Domain.Enums.Type>? Types { get; set; }

        public Status? Status { get; set; }

        public string? ClientName { get; set; }
    }
}
