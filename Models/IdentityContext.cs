using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPortfolyoWebSite.Entity;

namespace MyPortfolyoWebSite.Models
{
    public class IdentityContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }

        public DbSet<Header> Header { get; set; }
        public DbSet<AboutMe> AboutMe { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<LinkIcon> LinkIcons { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<ErrorLogs> ErrorLogs { get; set; }
    }

}