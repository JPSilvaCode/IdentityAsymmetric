using System;

namespace IA.Identity.API.Models
{
    public class RefreshToken
    {
        public RefreshToken()
        {
            Token = Guid.NewGuid();
        }

        public int Id { get; set; }
        public string UserEmail { get; set; }
        public Guid Token { get; private set; }
        public DateTime ExpirationDate { get; set; }
    }
}