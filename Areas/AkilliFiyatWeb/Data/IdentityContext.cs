using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPortfolyoWebSite.Models;

namespace AkilliFiyatWeb.Models
{
    public class IdentityContext: IdentityDbContext<AppUser, AppRole, String>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options):base(options)
        {
            
        }
    }
}