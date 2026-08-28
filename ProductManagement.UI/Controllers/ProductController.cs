using Microsoft.AspNetCore.Mvc;
using ProductManagement.UI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProductManagement.UI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================================================
        // GET API CLIENT WITH JWT
        // =========================================================
        private HttpClient GetApiClient()
        {
            var client = _httpClientFactory.CreateClient("ProductApi");

            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("JWT token not found.");
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // =========================================================
        // CHECK LOGIN
        // =========================================================
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("JwtToken"));
        }

        // =========================================================
        // CHECK ADMIN
        // =========================================================
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }


        // =========================================================
        // PRODUCT LIST
        // GET: /Product/Index
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = GetApiClient();

                var response = await client.GetAsync("api/Products");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Unable to load products.";

                    return View(new List<ProductViewModel>());
                }

                var products =
                    await response.Content
                        .ReadFromJsonAsync<List<ProductViewModel>>();

                return View(products ?? new List<ProductViewModel>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View(new List<ProductViewModel>());
            }
        }


        // =========================================================
        // ADD PRODUCT - GET
        // GET: /Product/Create
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            var model = new ProductViewModel
            {
                IsActive = true
            };

            return View(model);
        }


        // =========================================================
        // ADD PRODUCT - POST
        // POST: /Product/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            // DEBUG:
            // Put breakpoint here and check model.Name
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = GetApiClient();

                var response =
                    await client.PostAsJsonAsync(
                        "api/Products",
                        model);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ModelState.AddModelError(
                        "",
                        $"Unable to create product. {error}");

                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(model);
            }
        }


        // =========================================================
        // EDIT PRODUCT - GET
        // GET: /Product/Edit/1
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            try
            {
                var client = GetApiClient();

                var response =
                    await client.GetAsync(
                        $"api/Products/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Unable to load product.";

                    return View();
                }

                var product =
                    await response.Content
                        .ReadFromJsonAsync<ProductViewModel>();

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View();
            }
        }


        // =========================================================
        // EDIT PRODUCT - POST
        // POST: /Product/Edit/1
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            if (id != model.ProductId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = GetApiClient();

                var response =
                    await client.PutAsJsonAsync(
                        $"api/Products/{id}",
                        model);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(
                        "",
                        "Product not found.");

                    return View(model);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ModelState.AddModelError(
                        "",
                        $"Unable to update product. {error}");

                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(model);
            }
        }


        // =========================================================
        // DELETE PRODUCT
        // POST: /Product/Delete/1
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            try
            {
                var client = GetApiClient();

                var response =
                    await client.DeleteAsync(
                        $"api/Products/{id}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Unable to delete product.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] =
                    "Product deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }
    }
}