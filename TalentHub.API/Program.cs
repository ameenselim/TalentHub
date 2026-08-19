
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TalentHub.API.Repositories;
using TalentHub.Application.Common.Settings;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Infrastructure.Identity;
using TalentHub.Infrastructure.Persistence;
using TalentHub.Infrastructure.Services;

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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {

                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 6;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IAuthServices, AuthServices>();
            builder.Services.AddScoped<IAccountService, AccountServices>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                //app.UseSwagger();
                //app.UseSwaggerUI();
                app.MapScalarApiReference();
                //app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
