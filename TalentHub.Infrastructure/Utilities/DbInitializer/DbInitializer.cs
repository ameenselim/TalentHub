using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TalentHub.Infrastructure.Utilities.DbInitializer
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DbInitializer(ApplicationDbContext context, IConfiguration configuration, RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task Initialize()
        {
            try
            {
                // Apply pending migrations
                if ((await _context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _context.Database.MigrateAsync();
                }
                // Seed Roles
                var roles = new[]
                {
                    SystemRoles.SUPER_ADMIN,
                    SystemRoles.ADMIN,
                    SystemRoles.EMPLOYEE,
                    SystemRoles.CUSTOMER
                };

                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role.ToString()))
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            throw new Exception($"Failed to create role {role}: {errors}");
                        }
                    }
                }
                // Seed Super Admin
                var email = _configuration["SuperAdminAccount:Email"];
                if (await _userManager.FindByEmailAsync(email!) is null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = _configuration["SuperAdminAccount:UserName"],
                        Email = email,
                        FirstName = "Super",
                        LastName = "Admin",
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, _configuration["SuperAdminAccount:Password"]!);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create Super Admin: {errors}");
                    }
                    await _userManager.AddToRoleAsync(user, SystemRoles.SUPER_ADMIN);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during database initialization: {ex}");
            }
        }
    }
}