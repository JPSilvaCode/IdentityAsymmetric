using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IA.Identity.API.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        [PersonalData, Required, StringLength(30)]
        public string FirstName { get; set; }

        [PersonalData, Required, StringLength(60)]
        public string LastName { get; set; }

        [PersonalData, Required, StringLength(11)]
        public string ITIN { get; set; }
    }
}