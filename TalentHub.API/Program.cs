
using Microsoft.EntityFrameworkCore;
using TalentHub.API.Repositories;
using TalentHub.Application.Common.Settings;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Infrastructure.Persistence;

namespace TalentHub.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var connectionString =
                    builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string"
                        + "'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
             {
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
             });
            builder.Services.Configure<AppSettings>(
                builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
