using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MyPortfolyoWebSite.Models
{
    public static class IdentitySeedData
    {
        private const string adminUser = "mrholmeess";
        private const string adminPassword = "asd";

        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();


            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = await userManager.FindByNameAsync(adminUser);

            var roleManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            // Admin rolü var mı diye kontrol et
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");

            if (!adminRoleExists)
            {
                // Admin rolünü oluştur
                var adminRole = new AppRole("Admin");
                await roleManager.CreateAsync(adminRole);
            }

            if(user == null)
            {
                user = new AppUser {
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