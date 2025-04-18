using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDBContext _dBContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DbInitializer(ApplicationDBContext dBContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dBContext = dBContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (_dBContext.Database.GetPendingMigrations().Count() > 0)
                {
                    _dBContext.Database.Migrate();
                }
                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();

                    //Add User only if the user does not exist
                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "saurav@gmail.com",
                        Email = "saurav@gmail.com",
                        Name = "Saurav Lamichhane",
                        NormalizedEmail = "SAURAV@GMAIL.COM",
                        NormalizedUserName = "SAURAV@GMAIL.COM",
                        PhoneNumber = "9865013003"
                    },"Saurav@1234").GetAwaiter().GetResult();
                    ApplicationUser user = _dBContext.ApplicationUsers.FirstOrDefault(u => u.Email == "saurav@gmail.com");
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                }
            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}
