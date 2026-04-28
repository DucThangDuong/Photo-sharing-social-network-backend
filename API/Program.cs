using API.Entities;
using Microsoft.EntityFrameworkCore;
namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("CORS", options =>
                {
                    options
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            var app = builder.Build();
            builder.Services.AddDbContext<InstagramContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Instagram"));
            });
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseCors("CORS");
            app.Run();

        }
    }
}
