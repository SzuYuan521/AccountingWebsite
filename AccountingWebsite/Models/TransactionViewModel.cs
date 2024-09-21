namespace AccountingWebsite.Models
{
    public class TransactionViewModel
    {
        public decimal Balance { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
