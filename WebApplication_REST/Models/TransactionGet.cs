namespace WebApplication_REST.Models
{
    public class TransactionGet
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid UserId { get; set; } 
        public bool IsIncome { get; set; }
    }
}
