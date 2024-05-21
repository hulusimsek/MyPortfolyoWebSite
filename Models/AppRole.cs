using Microsoft.AspNetCore.Identity;

namespace MyPortfolyoWebSite.Models
{
	public class AppRole : IdentityRole
	{
		public AppRole()
		{
		}
		public AppRole(string roleName) : base(roleName)
		{
		}
		
	}
}