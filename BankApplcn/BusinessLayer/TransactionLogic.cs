using BankApplcn.BusinessLayer;
using BankApplcn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BankApplcn.Business
{
    public class TransactionLogic
    {
     public List<Account> accounts = new List<Account>();
        List<string> transactionIDs = new List<string>();
        //InterestRulesLogic IL = new InterestRulesLogic();

        private readonly InterestRulesLogic IL;

        public TransactionLogic(InterestRulesLogic interestRules)
        {
            IL = interestRules;
        }

        public void InputTransactions()
        {
            
            bool IsRun = true;

            while (IsRun)
            {
                Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format");
                Console.WriteLine("(or enter blank to go back to main menu):");
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    IsRun = false;
                    break;
                }

                var result = TryParseTransaction(input);
                if (!result.IsValid)
                {
                    Console.WriteLine($"Error: {result.Error}");
                    continue;
                }
                Console.WriteLine("Transaction created:"+result.Transaction.TransactionID);
                Account account = accounts.First(a => a.AccountNumber == result.Transaction.Account);
                ShowStatements(account);
                continue;
                
            }

        }

        /*private Account? CheckAccountNumber(string accountNumber)
        {
            foreach (var acc in accounts)
            {
                if (acc.AccountNumber == accountNumber)
                    return acc;
            }

            return null;

            
        }*/

        private string GenerateTransactionId(DateTime date)
        {
            string datePrefix = date.ToString("yyyyMMdd");
            int count = 0;
            foreach (var id in transactionIDs)
            {
                if (id.StartsWith(datePrefix))
                    count++;
            }
            count++;
            Console.WriteLine(count);
            string txnId = datePrefix + "-" + count.ToString("D2");
            transactionIDs.Add(txnId);
            return txnId;
        }

        public void ShowStatements(Account account)
        {
            Console.WriteLine("Account:" + account.AccountNumber);
            Console.WriteLine("| Date     | Txn Id      | Type | Amount |");
            foreach (var txn in account.Transactions)
            {
                string date = txn.Date.ToString("yyyyMMdd");
                string txnId = txn.TransactionID.PadRight(11); 
                string type = txn.Type.ToString();
                string amount = txn.Amount.ToString("F2").PadLeft(7); 

                Console.WriteLine("| " + date + " | " + txnId + " |  " + type + "   | " + amount + " |");
            }
        }

        public void PrintStatements()
        {
            Console.WriteLine("\nPlease enter account and month to generate the statement <Account> <Year><Month>");
            Console.WriteLine("(or enter blank to go back to main menu):");
            Console.Write("> ");
            string input = Console.ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            var parts = input.Split(' ');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid input.");
                return;
            }

            string accountNum = parts[0];
            if (!DateTime.TryParseExact(parts[1] + "01", "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthStart))
            {
                Console.WriteLine("Invalid month format.");
                return;
            }

            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNum);
            if (account == null)
            {
                Console.WriteLine("Account Number not found.");
                return;
            }

            DateTime monthEnd = new DateTime(monthStart.Year, monthStart.Month, DateTime.DaysInMonth(monthStart.Year, monthStart.Month));

            var monthlyTxns = account.Transactions
                .Where(t => t.Date >= monthStart && t.Date <= monthEnd)
                .OrderBy(t => t.Date)
                .ToList();

            decimal balance = 0;
            Console.WriteLine($"\nAccount: {account.AccountNumber}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");

            foreach (var txn in account.Transactions.OrderBy(t => t.Date))
            {
                if (txn.Date <= monthEnd)
                {
                    balance += txn.Type == 'D' ? txn.Amount :
                               txn.Type == 'W' ? -txn.Amount : txn.Amount;
                }

                if (txn.Date >= monthStart && txn.Date <= monthEnd)
                {
                    Console.WriteLine("| {0:yyyyMMdd} | {1,-11} |  {2}   | {3,7:F2} | {4,7:F2} |",
                        txn.Date, txn.TransactionID, txn.Type, txn.Amount, balance);
                }
            }
            decimal interest = IL.CalculateInterest(monthStart, monthEnd, monthlyTxns);
            balance += interest;
            Console.WriteLine("| {0:yyyyMMdd} | {1,-11} |  {2}   | {3,7:F2} | {4,7:F2} |",
                monthEnd, "", 'I', interest, balance);

        }

        public TransactionInputs TryParseTransaction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new TransactionInputs { IsValid = false, Error = "Input is empty" };

            string[] split = input.Split(' ');
            if (split.Length != 4)
                return new TransactionInputs { IsValid = false, Error = "Invalid format" };

            if (!DateTime.TryParseExact(split[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return new TransactionInputs { IsValid = false, Error = "Invalid format" };

            string accountNumber = split[1];
            if (string.IsNullOrEmpty(accountNumber))
                return new TransactionInputs { IsValid = false, Error = "Account number is empty" };

            char transtype = char.ToUpper(split[2][0]);
            if (transtype != 'D' && transtype != 'W')
                return new TransactionInputs { IsValid = false, Error = "Invalid transaction type" };

            if (!decimal.TryParse(split[3], out decimal amount))
                return new TransactionInputs { IsValid = false, Error = "Amount is not a number" };

            if (amount <= 0)
                return new TransactionInputs { IsValid = false, Error = "Amount must be greater than zero" };

            if (decimal.Round(amount, 2) != amount)
                return new TransactionInputs { IsValid = false, Error = "Amount must have max 2 decimal places" };

            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                account = new Account
                {
                    AccountNumber = accountNumber,
                    Balance = 0m,
                    Transactions = new List<Transaction>()
                };
                accounts.Add(account);
            }

            if (transtype == 'W')
            {
                if (account.Transactions.Count == 0)
                    return new TransactionInputs { IsValid = false, Error = "First transaction cannot be a withdrawal" };
                if (account.Balance < amount)
                    return new TransactionInputs { IsValid = false, Error = "Insufficient balance" };
            }

            decimal newBalance = transtype == 'D' ? account.Balance + amount : account.Balance - amount;

            string txnId = GenerateTransactionId(date);

            var transaction = new Transaction
            {
                TransactionID = txnId,
                Date = date,
                Account = accountNumber,
                Type = transtype,
                Amount = amount
            };

            account.Balance = newBalance;
            account.Transactions.Add(transaction);

            return new TransactionInputs
            {
                IsValid = true,
                Transaction = transaction
            };
        }

    }
    }
