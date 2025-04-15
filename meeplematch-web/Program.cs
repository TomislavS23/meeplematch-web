using meeplematch_web.Interfaces;
using meeplematch_web.Mapping;
using meeplematch_web.Service;
using meeplematch_web.Utils;
using Microsoft.EntityFrameworkCore;

namespace meeplematch_web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        //builder.Services.AddScoped<IEventRepository, EventRepository>();
        //builder.Services.AddScoped<IUserRepository, UserRepository>();

        //builder.Services.AddHttpClient<IEventApiService, EventApiService>();
        //builder.Services.AddHttpClient<IUserApiService, UserApiService>();
        //builder.Services.AddHttpClient<IAuthService, AuthService>();

        builder.Services.AddAutoMapper(typeof(MappingProfile));

        //builder.Services.AddDbContext<MeepleMatchContext>(
        //    options => options.UseNpgsql(Constants.PsqlConnectionString));

        builder.Services.AddHttpClient(Constants.ApiName, httpClient =>
        {
            string apiUrl = "http://localhost:5202/api/meeplematch/";
            //string apiUrl = "https://localhost:7230/api/meeplematch/";
            httpClient.BaseAddress = new Uri(apiUrl);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllers();
        //    endpoints.MapDefaultControllerRoute();
        //    //endpoints.MapControllerRoute(
        //    //    name: "default",
        //    //    pattern: "{controller=Home}/{action=Index}/{id?}");
        //});

        app.Run();
    }
}