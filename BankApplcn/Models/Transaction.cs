using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplcn.Models
{
    public class Transaction
    {
        public string TransactionID { get; set; }
        public DateTime Date { get; set; }
        public string Account { get; set; }
        public char Type { get; set; }
        public decimal Amount { get; set; }

    }
}
