using BankApplcn.Business;
using BankApplcn.BusinessLayer;
using BankApplcn.Models;
using System.Globalization;



var IL = new InterestRulesLogic(); 
var TL = new TransactionLogic(IL); 

while (true)
{
    Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
    Console.WriteLine("[T] Input transactions");
    Console.WriteLine("[I] Define interest rules");
    Console.WriteLine("[P] Print statement");
    Console.WriteLine("[Q] Quit");
    Console.Write("Enter your choice: ");

    string userinput = Console.ReadLine().Trim().ToUpper();

    switch (userinput)
    {
        case "T":
            TL.InputTransactions();
            break;
        case "I":
            IL.InterestRules();
            break;
        case "P":
             TL.PrintStatements();
            break;
        case "Q":
            Console.WriteLine("Thank you for using AwesomeGic Bank!");
            return;
        default:
            Console.WriteLine("Invalid choice! Please try again.");
            break;
    }

    Console.WriteLine("\nPress any key to return to main menu...");
    Console.ReadKey();
}




