namespace WebApplication_REST.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public bool IsIncome { get; set; }
    }

}
