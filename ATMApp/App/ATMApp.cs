// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using ATMApp.Domain.Entities;
using ATMApp.Domain.Enums;
using ATMApp.Domain.Interfaces;
using ATMApp.UI;
using ConsoleTables;
using Transaction = ATMApp.Domain.Entities.Transaction;

public class AtMApp : IUserLogin, IUserAccountActions, ITransaction
{
    private List<UserAccount> UserAccountList;
    private UserAccount selectedAccount;
    private List<Transaction> _listofTransaction;
    private const decimal minimumKeptAmount = 500;
    private readonly AppScreen screen;

    public AtMApp()
    {
        screen = new AppScreen();
    }
    

    public void Run()
    {
        AppScreen.Welcome();
        CheckUserCardNumberAndPassword();
        AppScreen.WelcomeCustomer(selectedAccount.FullName);
        while (true)
        {

            AppScreen.DisplayAppMenu();
            ProcessMenuoptions();
        }
    }
    public void InitializeData()
    {
        UserAccountList = new List<UserAccount>
        {
            new UserAccount{ Id=1, FullName ="Adapa Mohith Surya",AccountNumber=123456, CardNumber=321321 , CardPin=123123 , AccountBalance=500000.00m,IsLocked=false},
            new UserAccount{ Id=2, FullName ="Adapa Ramakrishna",AccountNumber=654321, CardNumber=987987 , CardPin=555555 , AccountBalance=800000.00m,IsLocked=false},
            new UserAccount{ Id=3, FullName ="Adapa Aruna Kumari",AccountNumber=258258, CardNumber=121212 , CardPin=198412 , AccountBalance=600000.00m,IsLocked=true},
        };
        _listofTransaction = new List<Transaction>();
    }


    public void CheckUserCardNumberAndPassword()
    {
        bool isCorrectLogin = false;
        while (isCorrectLogin == false)
        {
            UserAccount inputAccount = AppScreen.UserLoginForm();
            AppScreen.LoginProgress();
            foreach (UserAccount account in UserAccountList)
            {
                selectedAccount = account;
                if (inputAccount.CardNumber.Equals(selectedAccount.CardNumber))
                {
                    selectedAccount.TotalLogin++;

                    if (inputAccount.CardPin.Equals(selectedAccount.CardPin))
                    {
                        selectedAccount = account;

                        if (selectedAccount.IsLocked || selectedAccount.TotalLogin > 3)
                        {
                            AppScreen.PrintLockScreen();
                        }
                        else
                        {
                            selectedAccount.TotalLogin = 0;
                            isCorrectLogin = true;
                            break;
                        }
                    }
                }
                if (isCorrectLogin == false)
                {
                    Utility.PrintMessage("\n Invalid card number or PIN.", false);
                    selectedAccount.IsLocked = selectedAccount.TotalLogin == 3;
                    if (selectedAccount.IsLocked)
                    {
                        AppScreen.PrintLockScreen();
                    }
                }
                Console.Clear();

            }

        }
    }
    private void ProcessMenuoptions()
    {
        switch (validator.Convert<int>("an option:"))
        {
            case (int)AppMenu.CheckBalance:
                CheckBalance();
                break;
            case (int)AppMenu.PlaceDeposit:
                PlaceDeposit();
                break;
            case (int)AppMenu.MakeWithdrawal:
                MakeWithDrawal();
                break;
            case (int)AppMenu.InternalTransfer:
                var internalTransfer = screen.InternalTransferForm();
                ProcessInternalTransfer(internalTransfer);
                break;
            case (int)AppMenu.ViewTransaction:
               ViewTranction();
                break;
            case (int)AppMenu.Logout:
                AppScreen.LogoutProgress();
                Utility.PrintMessage("You have successfully logged out.Please " +
                    "collect your ATM card.");
                Run();
                break;
            default:
                Utility.PrintMessage("Invalid Option.", false);
                break;
        }
    }

    public void CheckBalance()
    {
        Utility.PrintMessage($"Your account balance is:{Utility.FormatAmount(selectedAccount.AccountBalance)}");
    }

    public void PlaceDeposit()
    {
        Console.WriteLine("\nOnly multiples of 500 and 2000 are allowed.\n ");
        var transaction_amt = validator.Convert<int>($"amount{AppScreen.cur}");
        Console.WriteLine("\nChecking and Counting bank notes.");
        Utility.PrintDotAnimation();
        Console.WriteLine("");

        //some gaurd clause
        if (transaction_amt <= 0)
        {
            Utility.PrintMessage("Amount needs to be greater than zero.Try aging", false);
            return;
        }
        if (transaction_amt % 500 != 0)
        {
            Utility.PrintMessage($"Enter deposit amount in multiples of 500 or 1000.Try again.", false);
            return;
        }
        if (PreviewBankNotesCount(transaction_amt) == false)
        {
            Utility.PrintMessage($"You have cancelled your action.", false);
            return;
        }

        InsertTransaction(selectedAccount.Id, TransactionType.Deposit, transaction_amt, "");

        selectedAccount.AccountBalance += transaction_amt;

        Utility.PrintMessage($"Your deposit of {Utility.FormatAmount(transaction_amt)} was" +
             $"succesful.", true);
    }

    public void MakeWithDrawal()
    {
        var transaction_amt = 0;
        int selectedAmount = AppScreen.selectAmount();
        if (selectedAmount == -1)
        {
            MakeWithDrawal();
            return;
        }
        else if (selectedAmount != 0)
        {
            transaction_amt = selectedAmount;
        }
        else
        {
            transaction_amt = validator.Convert<int>($"amount{AppScreen.cur}");
        }

        //input validation
        if (transaction_amt <= 0)
        {
            Utility.PrintMessage("Amount needs to be greater than zero. Try again", false);
            return;
        }
        if (transaction_amt % 500 != 0)
        {
            Utility.PrintMessage("You can only withdraw amount in multiples of 500 or 1000. Try again.", false);
            return;
        }
        //Business logic validation 

        if (transaction_amt > selectedAccount.AccountBalance)
        {
            Utility.PrintMessage($"Withdrawal failed. Your balance is too low to withdraw " +
                $"{Utility.FormatAmount(transaction_amt)}", false);
            return;
        }

        if ((selectedAccount.AccountBalance - transaction_amt) < minimumKeptAmount)
        {
            Utility.PrintMessage($"Withdrawal failed.Your account needs to have" +
                $"minimum{Utility.FormatAmount(minimumKeptAmount)}", false);
            return;
        }

        InsertTransaction(selectedAccount.Id, TransactionType.Withdrawal, -transaction_amt, "");
        //update account balance

        selectedAccount.AccountBalance -= transaction_amt;
        //success message
        Utility.PrintMessage($"You have successfully withdrawn" +
            $"{Utility.FormatAmount(transaction_amt)}.", true);
    }

    private bool PreviewBankNotesCount(int amount)
    {
        int thousandNotesCount = amount / 1000;
        int fiveHundredNotesCount = (amount % 1000) / 500;

        Console.WriteLine("\nSummary");
        Console.WriteLine("---------");
        Console.WriteLine($"{AppScreen.cur}1000 X {thousandNotesCount} = {1000 * thousandNotesCount}");
        Console.WriteLine($"{AppScreen.cur}500 X {fiveHundredNotesCount} = {500 * fiveHundredNotesCount}");
        Console.WriteLine($"Total amount:{Utility.FormatAmount(amount)}\n\n");
        int opt = validator.Convert<int>("1 to confirm");
        return opt.Equals(1);
    }

    public void InsertTransaction(long _UserBankAccountId, TransactionType _tranType, decimal _tranAmount, string _desc)
    {
        //create a new transaction object
        var transaction = new Transaction()
        {
            TransactionId = Utility.GetTransactionId(),
            UserBankAccountId = _UserBankAccountId,
            TransactionDate = DateTime.Now,
            TransactionType = _tranType,
            TransactionAmount = _tranAmount,
            Description = _desc
        };

        //add transaction object to the list
        _listofTransaction.Add(transaction);
    }

    public void ViewTranction()
    {
        var filteredTransactionList= _listofTransaction.Where(t => t.UserBankAccountId==selectedAccount.Id).ToList();
        if(filteredTransactionList.Count <= 0) 
        {
            Utility.PrintMessage("You have no Transactiob Yet.", true);
        }
        else
        {
            var table = new ConsoleTable("Id", "Transaction Date", "Type", "Descriptions", "Amount" + AppScreen.cur);
            foreach(var tran in filteredTransactionList) 
            {
                table.AddRow(tran.TransactionId, tran.TransactionDate, tran.TransactionType, tran.Description, tran.TransactionAmount);

            }
            table.Options.EnableCount = false;
            table.Write();
            Utility.PrintMessage($"You have{filteredTransactionList.Count}transaction(s)", true);
        }
    }

    private void ProcessInternalTransfer(InternalTransfer internalTransfer)
    {
        if(internalTransfer.TransferAmount<=0) 
        {
            Utility.PrintMessage("Amount needs to be more than zero.Try again.", false);
            return;
        }
        //check sender.s accouunt balance 
        if (internalTransfer.TransferAmount> selectedAccount.AccountBalance) 
        {
            Utility.PrintMessage($"Transfer failed.You do not have enough balance"+
                $" to transfer {Utility.FormatAmount(internalTransfer.TransferAmount)}", false);
            return;
        }
        //check the minmum kept amount
        if((selectedAccount.AccountBalance - internalTransfer.TransferAmount) < minimumKeptAmount)
        {
            Utility.PrintMessage($"Transfer failed. Your account needs to have minimum"+
                
            $"{Utility.FormatAmount(minimumKeptAmount)}",false); 
            return;
        }

        //check reciever's account number is valid

        var selectedBankAccountReciever =(from userAcc in UserAccountList
                                          where userAcc.AccountNumber == internalTransfer.RecientBankAccountNumber
                                          select userAcc).FirstOrDefault();
        if(selectedBankAccountReciever==null) 
        {
            Utility.PrintMessage("Transfer failed. Reciver bank account number is invalid.",false);
            return;
        }
        //check receiver's name
        if(selectedBankAccountReciever.FullName != internalTransfer.RecipientBankAccountName)
        {
            Utility.PrintMessage("Transfer Failed. Recipient's bank account name does not match ", false);
            return;
        }
        //add transaction to transaction record sender
        InsertTransaction(selectedAccount.Id, TransactionType.Transfer, -internalTransfer.TransferAmount, "Transfered" +
            $"to{selectedBankAccountReciever.AccountNumber}({selectedBankAccountReciever.FullName})");
        //update sender's account balance

        selectedAccount.AccountBalance -= internalTransfer.TransferAmount;

        InsertTransaction(selectedBankAccountReciever.Id, TransactionType.Transfer, internalTransfer.TransferAmount, "Transfered from" +
            $"{selectedAccount.AccountNumber}({selectedAccount.FullName})");
        //update reciever's acount

        selectedBankAccountReciever.AccountBalance += internalTransfer.TransferAmount;
        //print success msgs

        Utility.PrintMessage($"You have successfully transfered" +
            $" {Utility.FormatAmount(internalTransfer.TransferAmount)} to" +
            $"{internalTransfer.RecipientBankAccountName}",true);
        


            


    }
    
}