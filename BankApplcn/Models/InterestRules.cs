using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplcn.Models
{
    public class InterestRules
    {
        public string RuleID { get; set; }
        public DateTime Date { get; set; }
        public decimal Rate { get; set; }
    }
}
