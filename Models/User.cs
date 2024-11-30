using Microsoft.AspNetCore.Identity;

namespace MvcMovie.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsBlocked { get; set; } = false;

        [PersonalData]
        public required string FirstName { get; set; }
        [PersonalData]
        public required string LastName { get; set; }
        public DateTime? LastLoginTime { get; set; } = DateTime.Now;

        // Registration time is handled by Identity (but we can define it here if needed)
        public DateTime RegistrationTime { get; set; } = DateTime.Now;
    }
}
