using Microsoft.Extensions.Configuration;
using ProductManagement.API.Models;
using ProductManagement.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProductManagement.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;

        public JwtServiceTests()
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
                        "30"
                    }
                };

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(settings)
                    .Build();

            _jwtService =
                new JwtService(configuration);
        }


        [Fact]
        public void GenerateToken_Should_ReturnToken()
        {
            // Arrange

            var user =
                new User
                {
                    UserId = 1,
                    Name = "Admin User",
                    Email = "admin@test.com",
                    Role = "Admin",
                    IsActive = true
                };


            // Act

            string token =
                _jwtService.GenerateToken(user);


            // Assert

            Assert.False(
                string.IsNullOrWhiteSpace(token));
        }


        [Fact]
        public void GenerateToken_Should_ContainAdminRole()
        {
            // Arrange

            var user =
                new User
                {
                    UserId = 1,
                    Name = "Admin User",
                    Email = "admin@test.com",
                    Role = "Admin",
                    IsActive = true
                };


            // Act

            string token =
                _jwtService.GenerateToken(user);


            var handler =
                new JwtSecurityTokenHandler();

            var jwt =
                handler.ReadJwtToken(token);


            // Assert

            Assert.Contains(
                jwt.Claims,
                claim =>
                    claim.Type == ClaimTypes.Role &&
                    claim.Value == "Admin");
        }
    }
}