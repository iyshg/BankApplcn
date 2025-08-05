using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BankApplcn.Models
{
    public class Account
    {
        public string AccountNumber { get; set; }

        public decimal Balance { get; set; }
        public List<Transaction> transactions = new List<Transaction>();

        public List<Transaction> Transactions
        {
            get { return transactions; }
            set { transactions = value; }
        }

    }
}
