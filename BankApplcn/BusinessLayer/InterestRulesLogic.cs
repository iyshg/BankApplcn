using BankApplcn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplcn.BusinessLayer
{
    public class InterestRulesLogic
    {
         public List<InterestRules> interestRules = new List<InterestRules>();
        public void InterestRules()
        {
            while (true)
            {
                Console.WriteLine("\nPlease enter interest rules details in <Date> <RuleId> <Rate in %> format");
                Console.WriteLine("(or enter blank to go back to main menu):");
                Console.Write("> ");
                string input = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(input)) break;

                var result = TryParseInterestRule(input);
                if (!result.IsValid)
                {
                    Console.WriteLine(result.Error);
                    continue;
                }
                AddOrUpdateInterestRule(result.Rule);
                PrintRules();

               
            }

           

        }

        public void PrintRules()
        {
            Console.WriteLine("\nInterest rules:");
            Console.WriteLine("| Date     | RuleId | Rate (%) |");
            foreach (var rule in interestRules.OrderBy(r => r.Date))
            {
                Console.WriteLine(rule.Date.ToString("yyyyMMdd") + " " + rule.RuleID + " " + rule.Rate + "%");
            }
        }

        public void AddOrUpdateInterestRule(InterestRules rule)
        {
            interestRules.RemoveAll(r => r.Date == rule.Date);
            interestRules.Add(rule);
        }

        public InterestRulesInputs TryParseInterestRule(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new InterestRulesInputs { IsValid = false, Error = "Input is empty" };

            var split = input.Split(' ');
            if (split.Length != 3)
                return new InterestRulesInputs { IsValid = false, Error = "Invalid format" };

            if (!DateTime.TryParseExact(split[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return new InterestRulesInputs { IsValid = false, Error = "Invalid date format" };

            if (!decimal.TryParse(split[2], out decimal rate) || rate <= 0 || rate >= 100)
                return new InterestRulesInputs { IsValid = false, Error = "Invalid rate. Must be > 0 and < 100" };

            return new InterestRulesInputs
            {
                IsValid = true,
                Rule = new InterestRules
                {
                    Date = date,
                    RuleID = split[1],
                    Rate = rate
                }
            };
        }


        public InterestRules ApplyInterestRules(DateTime date)
        {
            //Console.WriteLine(interestRules.Count);
             var rules = interestRules
                .Where(r => r.Date <= date)
                .OrderByDescending(r => r.Date)
                .FirstOrDefault();

            return rules;

        }

        public decimal CalculateInterest(DateTime monthStart, DateTime monthEnd, List<Transaction> monthlyTransactions)
        {
            decimal totalInterest = 0;
            decimal balance = 0;
            DateTime currentDate = monthStart;

            while (currentDate <= monthEnd)
            {
                
                var todaysTxns = monthlyTransactions.Where(t => t.Date == currentDate).ToList();
                foreach (var txn in todaysTxns)
                {
                    balance += txn.Type == 'D' ? txn.Amount : txn.Type == 'W' ? -txn.Amount : 0;
                }

                InterestRules rule = ApplyInterestRules(currentDate);
                if (rule != null)
                {
                    decimal dailyInterest = balance * (rule.Rate / 100) / 365;
                    totalInterest += dailyInterest;
                }

                currentDate = currentDate.AddDays(1);
            }

            return Math.Round(totalInterest, 2);
        }
    }
    }
