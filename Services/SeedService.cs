using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePro.Data;
using MoviePro.Models.Database;
using MoviePro.Models.Settings;

namespace MoviePro.Services
{
    public class SeedService
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public SeedService(IOptions<AppSettings> appSettings, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _appSettings = appSettings.Value;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task ManageDataAsync()
        {
            await UpdateDatabaseAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCollections();
        }

        private async Task UpdateDatabaseAsync()
        {
            await _dbContext.Database.MigrateAsync();
        }

        private async Task SeedRolesAsync()
        {
            if (_dbContext.Roles.Any())
                return;
            //var adminRole = _appSettings.MovieProSettings.DefaultCredentials.DCRole;
            var adminRole = _configuration["DCRole"];
            await _roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        private async Task SeedUsersAsync()
        {
            if (_userManager.Users.Any())
                return;
            //var credentials = _appSettings.MovieProSettings.DefaultCredentials;
            //var newUser = new IdentityUser()
            //{
            //    Email = credentials.DCEmail,
            //    UserName = credentials.DCEmail,
            //    EmailConfirmed = true
            //};
            //await _userManager.CreateAsync(newUser, credentials.DCPassword);
            //await _userManager.AddToRoleAsync(newUser, credentials.DCRole);

            var userCredentials = _configuration["DCEmail"];
            var seedPassword = _configuration["DCPassword"];
            var seedRole = _configuration["DCRole"];

            var newUser = new IdentityUser()
            {
                Email = userCredentials,
                UserName = userCredentials,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(newUser, seedPassword);
            await _userManager.AddToRoleAsync(newUser, seedRole);

        }

        private async Task SeedCollections()
        {
            if (_dbContext.Collection.Any())
                return;

            _dbContext.Add(new Collection()
            {
                Name = _appSettings.MovieProSettings.DefaultCollection.Name,
                Description = _appSettings.MovieProSettings.DefaultCollection.Description
            });
            await _dbContext.SaveChangesAsync();
        }
    }
}
