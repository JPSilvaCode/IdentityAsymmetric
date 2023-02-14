namespace IA.Identity.API.Models
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ITIN { get; set; }
        public string Email { get; set; }
    }
}