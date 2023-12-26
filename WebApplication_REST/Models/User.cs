namespace WebApplication_REST.Models
{
    public class User
    {
        //Guid wie int aber gut für Ids
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
