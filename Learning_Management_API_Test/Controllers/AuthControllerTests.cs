using LearningManagement_API.Controllers;
using LearningManagement_API.Data;
using LearningManagement_API.DTO;
using LearningManagement_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LearningManagement_API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private LearningManagement_APIContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LearningManagement_APIContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new LearningManagement_APIContext(options);

            context.Users.Add(new User
            {
                Id = 1,
                Email = "admin@test.com",
                FullName = "Admin User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin"
            });

            context.SaveChanges();
            return context;
        }

        private IConfiguration CreateConfiguration()
        {
            var settings = new Dictionary<string, string>
            {
                { "Jwt:Key", "THIS_IS_A_SUPER_SECRET_JWT_KEY_32_CHARS" },
                { "Jwt:Issuer", "LearningManagementAPI" },
                { "Jwt:Audience", "LearningManagementAPIUsers" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();
        }

        [Fact]
        public void Login_ReturnsToken_WhenCredentialsAreCorrect()
        {
            var context = CreateDbContext();
            var config = CreateConfiguration();
            var controller = new AuthController(context, config);

            var request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };

            var result = controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void Login_ReturnsUnauthorized_WhenPasswordIsWrong()
        {
            var context = CreateDbContext();
            var config = CreateConfiguration();
            var controller = new AuthController(context, config);

            var request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "WrongPassword"
            };

            var result = controller.Login(request);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Login_ReturnsUnauthorized_WhenUserDoesNotExist()
        {
            var context = CreateDbContext();
            var config = CreateConfiguration();
            var controller = new AuthController(context, config);

            var request = new LoginRequest
            {
                Email = "notfound@test.com",
                Password = "Admin123!"
            };

            var result = controller.Login(request);

            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
