using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MyPortfolyoWebSite.Models
{
    public static class IdentitySeedData
    {
        private const string adminUser = "";
        private const string adminPassword = "";

        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();


            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(adminUser);

            var roleManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Admin rolü var mı diye kontrol et
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");

            if (!adminRoleExists)
            {
                // Admin rolünü oluştur
                var adminRole = new IdentityRole("Admin");
                await roleManager.CreateAsync(adminRole);
            }

            if(user == null)
            {
                user = new IdentityUser {
                    UserName = adminUser,
                    Email = "hulusimsek58@gmail.com",
                    PhoneNumber = "1234"                    
                };
                

                await userManager.CreateAsync(user, adminPassword);
            }

            await userManager.AddToRoleAsync(user, "Admin");

        }
    } 
}