namespace AccountingWebsite.Models
{
    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Total { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
