using Microsoft.AspNetCore.Identity;

namespace MvcMovie.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsBlocked { get; set; } = false;

        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }
        // You can add custom properties here, like FirstName, LastName, etc.
    }
}
