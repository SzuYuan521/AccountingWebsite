namespace AccountingWebsite.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string TransactionTitle { get; set; } = string.Empty;
        public string TransactionDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionType TransactionType { get; set; }

        public int UserId { get; set; } // Foreign key
        public User User { get; set; }
    }

    public enum TransactionType
    {
        Income, // 收入
        Expense, // 支出
    }
}
