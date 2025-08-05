using BankApplcn.Business;
using BankApplcn.BusinessLayer;
using BankApplcn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BankApplcnTest
{
    public class TransactionTestCase
    {
        private readonly TransactionLogic _logic;
        public TransactionTestCase()
        {
            _logic = new TransactionLogic(new InterestRulesLogic());
        }

        [Fact]
        public void ValidDepositTransaction_ShouldBeValid()
        {

            string input = "20250801 AC001 D 100.00";

            var result = _logic.TryParseTransaction(input);

            Assert.True(result.IsValid);
            Assert.NotNull(result.Transaction);
            Assert.Equal("AC001", result.Transaction.Account);
            Assert.Equal('D', result.Transaction.Type);
            Assert.Equal(100.00m, result.Transaction.Amount);
            Assert.StartsWith("20250801-", result.Transaction.TransactionID);
        }

        [Fact]
        public void FirstTransactionCannotBeWithdrawal_ShouldFail()
        {
            string input = "20250801 AC001 W 50.00";
            var result = _logic.TryParseTransaction(input);

            Assert.False(result.IsValid);
            Assert.Equal("First transaction cannot be a withdrawal", result.Error);
        }

        [Fact]
        public void WithdrawalExceedingBalance_ShouldFail()
        {

            _logic.TryParseTransaction("20250801 AC001 D 100.00");

            var result = _logic.TryParseTransaction("20250801 AC001 W 150.00");

            Assert.False(result.IsValid);
            Assert.Equal("Insufficient balance", result.Error);
        }

        [Fact]
        public void AmountMoreThanTwoDecimalPlaces_ShouldFail()
        {
            string input = "20250801 AC001 D 100.123";
            var result = _logic.TryParseTransaction(input);

            Assert.False(result.IsValid);
            Assert.Equal("Amount must have max 2 decimal places", result.Error);
        }

        [Fact]
        public void InvalidDateFormat_ShouldFail()
        {
            
            var result = _logic.TryParseTransaction("2025-08-01 AC001 D 100.00");

            Assert.False(result.IsValid);
            Assert.Equal("Invalid format", result.Error);
        }

        [Fact]
        public void InvalidTransactionType_ShouldFail()
        {
            
            var result = _logic.TryParseTransaction("20250801 AC001 X 100.00");

            Assert.False(result.IsValid);
            Assert.Equal("Invalid transaction type", result.Error);
        }

        [Fact]
        public void ZeroAmount_ShouldFail()
        {
            
            var result = _logic.TryParseTransaction("20250801 AC001 D 0");

            Assert.False(result.IsValid);
            Assert.Equal("Amount must be greater than zero", result.Error);
        }

        [Fact]
        public void EmptyAccount_ShouldFail()
        {
            
            var result = _logic.TryParseTransaction("20250801  D 100.00");

            Assert.False(result.IsValid);
            Assert.Equal("Account number is empty", result.Error); 
        }

        [Fact]
        public void InvalidFormat_ShouldFail()
        {
            
            var result = _logic.TryParseTransaction("This is not valid");

            Assert.False(result.IsValid);
            Assert.Equal("Invalid format", result.Error);
        }

        [Fact]
        public void ValidDepositFollowedByValidWithdrawal_ShouldSucceed()
        {
           

            var deposit = _logic.TryParseTransaction("20250801 AC001 D 200.00");
            Assert.True(deposit.IsValid);

            var withdrawal = _logic.TryParseTransaction("20250801 AC001 W 100.00");
            Assert.True(withdrawal.IsValid);
            var account = _logic.accounts.Find(a => a.AccountNumber == "AC001");
            Assert.Equal(100.00m, account?.Balance);
        }
        [Fact]
        public void TransactionIDs_ShouldBeUniqueAndCorrectlyFormatted()
        {
            

            string date = "20250805"; 

         
            var result1 = _logic.TryParseTransaction($"{date} A001 D 100");
            var result2 = _logic.TryParseTransaction($"{date} A001 D 200");
            var result3 = _logic.TryParseTransaction($"{date} A001 W 50");

            var account = _logic.accounts.Find(a => a.AccountNumber == "A001");
            Assert.Equal(3, account?.Transactions.Count);

            var ids = account?.Transactions.Select(t => t.TransactionID).ToList();

            
            Assert.Equal($"{date}-01", ids?[0]);
            Assert.Equal($"{date}-02", ids?[1]);
            Assert.Equal($"{date}-03", ids?[2]);

            Assert.Equal(3, ids?.Distinct().Count()); 
        }

        [Fact]
        public void ShowStatements_ShouldPrintFormattedTransactionDetails()
        {
            
            var account = new Account
            {
                AccountNumber = "A001",
                Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Date = new DateTime(2025, 8, 1),
                    TransactionID = "20250801-01",
                    Type = 'D',
                    Amount = 1000m
                },
                new Transaction
                {
                    Date = new DateTime(2025, 8, 1),
                    TransactionID = "20250801-02",
                    Type = 'W',
                    Amount = 500m
                }
            }
            };


            var originalOut = Console.Out;
            var sw = new StringWriter();
            try
            {
                Console.SetOut(sw);

                
                _logic.ShowStatements(account);

             
                string output = sw.ToString();
                Assert.Contains("Account:A001", output);
                Assert.Contains("| Date     | Txn Id      | Type | Amount |", output);
                Assert.Contains("| 20250801 | 20250801-01 |  D   | 1000.00 |", output);
                Assert.Contains("| 20250801 | 20250801-02 |  W   |  500.00 |", output);
            }
            catch
            {
                sw.Dispose();
                Console.SetOut(originalOut);
            }
        }


    }
}
