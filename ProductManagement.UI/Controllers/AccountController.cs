using Microsoft.AspNetCore.Mvc;
using ProductManagement.UI.Models;

namespace ProductManagement.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ProductApi");

            var response = await client.PostAsJsonAsync("api/Auth/login",model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error ="Invalid email or password";
                return View(model);
            }

            var loginResponse =await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();
            if (loginResponse == null)
            {
                ViewBag.Error = "Login failed";
                return View(model);
            }

            HttpContext.Session.SetString("JwtToken",loginResponse.Token);

            HttpContext.Session.SetString("UserName",loginResponse.Name);

            HttpContext.Session.SetString("UserRole",loginResponse.Role);


            return RedirectToAction("Index","Product");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
