namespace ProductManagement.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddRazorPages();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSession();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient("ProductApi", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:BaseUrl"]!
                );
            });

           

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                                name: "default",
                                pattern: "{controller=Account}/{action=Login}/{id?}");

           // app.MapRazorPages();

            app.Run();
        }
    }
}
