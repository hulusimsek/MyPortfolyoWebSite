using Microsoft.AspNetCore.Identity;

namespace MyPortfolyoWebSite.Models
{
    public class AppUser: IdentityUser
    {
        public string? FullName { get; set; }
    }
}