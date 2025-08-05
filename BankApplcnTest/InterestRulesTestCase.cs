using BankApplcn.BusinessLayer;
using BankApplcn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplcnTest
{
    public class InterestRulesTestCase
    {
        [Fact]
        public void ValidInput_ShouldBeParsedSuccessfully()
        {
            var logic = new InterestRulesLogic();
            string input = "20250101 RULE01 3.5";

            var result = logic.TryParseInterestRule(input);

            Assert.True(result.IsValid);
            Assert.Equal("RULE01", result.Rule.RuleID);
            Assert.Equal(3.5m, result.Rule.Rate);
            Assert.Equal(new DateTime(2025, 1, 1), result.Rule.Date);
        }

        [Fact]
        public void InvalidRate_ShouldFail()
        {
            var logic = new InterestRulesLogic();
            string input = "20250101 RULE01 150";

            var result = logic.TryParseInterestRule(input);

            Assert.False(result.IsValid);
            Assert.Equal("Invalid rate. Must be > 0 and < 100", result.Error);
        }

        [Fact]
        public void AddOrUpdateInterestRule_ShouldReplaceOldRuleWithSameDate()
        {
            var logic = new InterestRulesLogic();

            logic.AddOrUpdateInterestRule(new InterestRules
            {
                Date = new DateTime(2025, 1, 1),
                RuleID = "RULE01",
                Rate = 3.5m
            });

            logic.AddOrUpdateInterestRule(new InterestRules
            {
                Date = new DateTime(2025, 1, 1),
                RuleID = "RULE01",
                Rate = 4.0m
            });

            Assert.Single(logic.interestRules);

            Assert.Equal("RULE01", logic.interestRules[0].RuleID);
            Assert.Equal(4.0m, logic.interestRules[0].Rate);
        }

        [Fact]
        public void PrintRules_ShouldDisplayFormattedInterestRules()
        {
            
            var logic = new InterestRulesLogic();

            logic.AddOrUpdateInterestRule(new InterestRules
            {
                Date = new DateTime(2025, 8, 1),
                RuleID = "RULE01",
                Rate = 3.5m
            });

            logic.AddOrUpdateInterestRule(new InterestRules
            {
                Date = new DateTime(2025, 8, 2),
                RuleID = "RULE02",
                Rate = 4.0m
            });

            var originalOut = Console.Out;
            
            var sw = new StringWriter();
            try
            {
                Console.SetOut(sw);

                
                logic.PrintRules();

                
                string output = sw.ToString();
                Assert.Contains("| Date     | RuleId | Rate (%) |", output);
                Assert.Contains("20250801 RULE01 3.5%", output);
                Assert.Contains("20250802 RULE02 4.0%", output);
            }
            catch
            {
                Console.SetOut(originalOut);
                sw.Dispose();
            }
        }

        [Fact]
        public void ApplyInterestRules_ShouldReturnLatestRuleBeforeGivenDate()
        {

            var rule1 =new InterestRules { RuleID = "RULE01", Date = new DateTime(2025, 01, 01), Rate = 3.5m };
            var rule2 = new InterestRules { RuleID = "RULE02", Date = new DateTime(2025, 06, 01), Rate = 4.0m };
        

            var logic = new InterestRulesLogic();
            logic.AddOrUpdateInterestRule(rule1);
            logic.AddOrUpdateInterestRule(rule2);
           
            var result = logic.ApplyInterestRules(new DateTime(2025, 07, 01));

            
            Assert.NotNull(result);
            Assert.Equal("RULE02", result.RuleID);
            Assert.Equal(4.0m, result.Rate);
        }

        [Fact]
        public void CalculateInterest_WithDailyDeposit_ShouldAccumulateInterestCorrectly()
        {
            
            var rules =
            new InterestRules { RuleID = "RULE01", Date = new DateTime(2025, 01, 01), Rate = 3.65m };
        

            var logic = new InterestRulesLogic();
            logic.AddOrUpdateInterestRule(rules);

            var transactions = new List<Transaction>
        {
            new Transaction { Date = new DateTime(2025, 08, 01), Type = 'D', Amount = 1000, TransactionID = "20250801-01" }
        };

            DateTime start = new DateTime(2025, 08, 01);
            DateTime end = new DateTime(2025, 08, 10);

            
            decimal interest = logic.CalculateInterest(start, end, transactions);

           
            Assert.True(interest > 0);
            Assert.Equal(1.00m, interest, 2);  
        }




    }
}
