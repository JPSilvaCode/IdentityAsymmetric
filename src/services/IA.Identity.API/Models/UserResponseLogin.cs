using System;
using System.Collections.Generic;

namespace IA.Identity.API.Models
{
    public class UserResponseLogin
    {
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public Guid RefreshToken { get; set; }
        public UserToken UsuarioToken { get; set; }
    }

    public class UserToken
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public IEnumerable<UserClaim> Claims { get; set; }
    }

    public class UserClaim
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
