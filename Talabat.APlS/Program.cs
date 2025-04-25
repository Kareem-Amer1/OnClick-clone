using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Talabat.APlS.Errors;
using Talabat.APlS.Extensions;
using Talabat.APlS.Helpers;
using Talabat.APlS.Middlewares;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Identity;
using Talabat.Core.Repositories;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.Identity;
namespace Talabat.APlS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            #region Configure Services Add services to the container.
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<StoreContext>(Options =>
            {
                    Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            //builder.Services.AddScoped<IGenericRepository<Product>, GenericRepository<Product>>();

            builder.Services.AddDbContext<AppIdentityDbContext>(Options=>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });
            

            builder.Services.AddSingleton<IConnectionMultiplexer>(Options =>
            {
                var Connection = builder.Configuration.GetConnectionString("RedisConnection");
                return ConnectionMultiplexer.Connect(Connection);
            });
            builder.Services.AddApplicationServices();

            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddCors(Options =>
            {
                Options.AddPolicy("MyPolicy", options =>
                {
                    options.AllowAnyHeader();
                    options.AllowAnyMethod();
                    options.WithOrigins(builder.Configuration["FrontBaseUrl"]);
                });
            });
            #endregion
            var app = builder.Build();
            #region Update-Database
            using var Scope = app.Services.CreateScope();
            var Services = Scope.ServiceProvider;
            var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();
            try {
                
                var dbContext = Services.GetRequiredService<StoreContext>();
                await dbContext.Database.MigrateAsync();

                var IdentityDbContext = Services.GetRequiredService<AppIdentityDbContext>();
                await IdentityDbContext.Database.MigrateAsync();
                //Seed the database
                var UserManager = Services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUserAsync(UserManager);
                await StoreContextSeed.SeedAsync(dbContext);
            }
            catch (Exception ex)
            {
                var logger = LoggerFactory.CreateLogger<Program>();
                //Log the error
                logger.LogError(ex, "An error occurred during migration");
            }
            //using var Scope = app.Services.CreateScope();
            //var Services = Scope.ServiceProvider;
            //var DbContext = Services.GetRequiredService<StoreContext>();
            //await DbContext.Database.MigrateAsync();

            #endregion


            

            #region Configure the HTTP request pipeline.
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMiddleware<ExceptionMiddleWare>();
                app.UseSwaggerMiddlewares();
            }
            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseHttpsRedirection();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            #endregion
            app.Run();
        }
    }
}
