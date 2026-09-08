using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductManagement.API.Controllers;
using ProductManagement.API.Data;
using ProductManagement.API.DTOs;
using ProductManagement.API.Models;
using ProductManagement.API.Services;

namespace ProductManagement.Tests.API
{
    public class AuthControllerTests
    {
        private ApplicationDbContext CreateDatabase()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(
                        databaseName:
                        Guid.NewGuid().ToString())
                    .Options;

            return new ApplicationDbContext(options);
        }


        private JwtService CreateJwtService()
        {
            var settings =
                new Dictionary<string, string?>
                {
                    {
                        "Jwt:Key",
                        "THIS_IS_MY_SUPER_SECRET_JWT_KEY_123456789"
                    },

                    {
                        "Jwt:Issuer",
                       "ProductManagementAPI"
                    },

                    {
                        "Jwt:Audience",
                         "ProductManagementUI"
                    },

                    {
                        "Jwt:ExpiryMinutes",
                        "60"
                    }
                };

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(settings)
                    .Build();

            return new JwtService(configuration);
        }


        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOk()
        {
            // Arrange

            using var context =
                CreateDatabase();


            context.Users.Add(
                new User
                {
                    UserId = 1,
                    Name = "Admin User",
                    Email = "admin@test.com",
                    Password = "123456",
                    Role = "Admin",
                    IsActive = true
                });


            await context.SaveChangesAsync();


            var controller =
                new AuthController(
                    context,
                    CreateJwtService());


            var loginRequest =
                new LoginRequest
                {
                    Email = "admin@test.com",
                    Password = "123456"
                };


            // Act

            var result =
                await controller.Login(loginRequest);


            // Assert

            Assert.IsType<OkObjectResult>(result);
        }


        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange

            using var context =
                CreateDatabase();


            context.Users.Add(
                new User
                {
                    UserId = 1,
                    Name = "Admin User",
                    Email = "admin@test.com",
                    Password = "123456",
                    Role = "Admin",
                    IsActive = true
                });


            await context.SaveChangesAsync();


            var controller =
                new AuthController(
                    context,
                    CreateJwtService());


            var loginRequest =
                new LoginRequest
                {
                    Email = "admin@test.com",
                    Password = "WRONG"
                };


            // Act

            var result =
                await controller.Login(loginRequest);


            // Assert

            Assert.IsType<UnauthorizedObjectResult>(
                result);
        }
    }
}