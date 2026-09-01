
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagement.API.Authentication;
using ProductManagement.API.Data;
using ProductManagement.API.Services;

namespace ProductManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckles
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });            

            var jwtKey = builder.Configuration["Jwt:Key"]!;
            var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
            var jwtAudience = builder.Configuration["Jwt:Audience"]!;


            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience=true,
            //        ValidateLifetime=true,
            //        ValidateIssuerSigningKey=true,
            //        ValidIssuer= jwtIssuer,
            //        ValidAudience= jwtAudience,
            //        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            //        ClockSkew=TimeSpan.Zero
            //    };
            //});

            //builder.Services.AddAuthorization();

            // ==========================================
            // HMAC NONCE STORE
            // ==========================================

            builder.Services.AddSingleton<
                IHmacNonceStore,
                HmacNonceStore>();

            // ==========================================
            // AUTHENTICATION
            // ==========================================

            builder.Services
                .AddAuthentication(options =>
                {
                    // JWT is default authentication
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })


                // ======================================
                // JWT
                // ======================================

                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer = true,

                                ValidateAudience = true,

                                ValidateLifetime = true,

                                ValidateIssuerSigningKey = true,

                                ValidIssuer =
                                    jwtIssuer,

                                ValidAudience =
                                    jwtAudience,

                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(
                                            jwtKey)),

                                ClockSkew =
                                    TimeSpan.Zero
                            };
                    })


                // ======================================
                // HMAC
                // ======================================

                .AddScheme<
                    HmacAuthenticationOptions,
                    HmacAuthenticationHandler>(
                    HmacAuthenticationOptions.DefaultScheme,
                    options =>
                    {
                        options.ExpirySeconds = 300;
                    });


            // ==========================================
            // AUTHORIZATION
            // ==========================================

            builder.Services.AddAuthorization(options =>
            {
                // --------------------------------------
                // JWT ONLY
                // --------------------------------------

                options.AddPolicy(
                    "JwtOnly",
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(
                            JwtBearerDefaults
                                .AuthenticationScheme);

                        policy.RequireAuthenticatedUser();
                    });


                // --------------------------------------
                // HMAC ONLY
                // --------------------------------------

                options.AddPolicy(
                    "HmacOnly",
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(
                            HmacAuthenticationOptions
                                .DefaultScheme);

                        policy.RequireAuthenticatedUser();
                    });


                // --------------------------------------
                // JWT + HMAC
                // --------------------------------------

                options.AddPolicy(
                    "JwtAndHmac",
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.AddAuthenticationSchemes(
                            JwtBearerDefaults
                                .AuthenticationScheme,
                            HmacAuthenticationOptions
                                .DefaultScheme);
                    });


                // --------------------------------------
                // JWT + HMAC + ADMIN
                // --------------------------------------

                options.AddPolicy(
                    "JwtAndHmacAdmin",
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireRole("Admin");

                        policy.AddAuthenticationSchemes(
                            JwtBearerDefaults
                                .AuthenticationScheme,
                            HmacAuthenticationOptions
                                .DefaultScheme);
                    });
            });


            builder.Services.AddScoped<JwtService>();//Add JWT Token

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers(); 

            app.Run();
        }
    }
}
