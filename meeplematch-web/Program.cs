using meeplematch_api.Model;
using meeplematch_api.Repository;
using meeplematch_api.Service;
using meeplematch_api.Utils;
using meeplematch_web.Mapping;
using Microsoft.EntityFrameworkCore;

namespace meeplematch_web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddDbContext<MeepleMatchContext>(
            options => options.UseNpgsql(Constants.PsqlConnectionString));

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