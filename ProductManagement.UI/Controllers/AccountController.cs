using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.UI.Models;

namespace ProductManagement.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =====================================================
        // LOGIN GET
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // =====================================================
        // LOGIN POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ================================================
            // API CLIENT
            // ================================================

            var client =
                _httpClientFactory
                    .CreateClient("ProductApi");


            // ================================================
            // LOGIN REQUEST
            // ================================================

            var response =
                await client.PostAsJsonAsync(
                    "api/Auth/login",
                    model);


            // ================================================
            // INVALID LOGIN
            // ================================================

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    "Invalid email or password.";

                return View(model);
            }


            // ================================================
            // READ RESPONSE
            // ================================================

            var result =
                await response.Content
                    .ReadFromJsonAsync<LoginResponse>();


            if (result == null ||
                string.IsNullOrWhiteSpace(result.Token))
            {
                ViewBag.Error =
                    "Invalid response from API.";

                return View(model);
            }


            // ================================================
            // STORE JWT
            // ================================================

            HttpContext.Session.SetString(
                "JwtToken",
                result.Token);


            // ================================================
            // STORE USER INFORMATION
            // ================================================

            if (!string.IsNullOrWhiteSpace(
                    result.UserName))
            {
                HttpContext.Session.SetString(
                    "UserName",
                    result.UserName);
            }

            if (!string.IsNullOrWhiteSpace(
                    result.Role))
            {
                HttpContext.Session.SetString(
                    "UserRole",
                    result.Role);
            }


            // ================================================
            // REDIRECT
            // ================================================

            return RedirectToAction(
                "Index",
                "Product");
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Account");
        }
    }


    // =========================================================
    // LOGIN RESPONSE
    // =========================================================

    public class LoginResponse
    {
        public string Token { get; set; } =
            string.Empty;

        public string UserName { get; set; } =
            string.Empty;

        public string Role { get; set; } =
            string.Empty;
    }
}