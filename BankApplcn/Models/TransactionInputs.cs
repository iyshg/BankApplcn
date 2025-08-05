using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplcn.Models
{
    public class TransactionInputs
    {
            public bool IsValid { get; set; }
            public string? Error { get; set; }
            public Transaction? Transaction { get; set; }
        
    }
}
