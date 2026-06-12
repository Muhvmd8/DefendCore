using DefendCore.API.Middlewares.Extensions;
using DefendCore.Application.Interfaces;
using DefendCore.Domain.Interfaces;
using DefendCore.Domain.Settings;
using DefendCore.Infrastructure.Presistence.DbContexts;
using DefendCore.Infrastructure.Presistence.Repositoreis;
using DefendCore.Infrastructure.Services.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace DefendCore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IIpSecurityService, IpSecurityService>();

            #region Settings Configuration
            builder.Services.Configure<IpSecuritySettings>(builder.Configuration.GetSection(nameof(IpSecuritySettings)));
            #endregion

            #region Forwarded Headers Configuration
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                         | ForwardedHeaders.XForwardedProto;
            });
            #endregion

            var app = builder.Build();

            // Use forwarded headers to correctly handle client IP addresses when behind a reverse proxy
            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();


            app.UseHttpsRedirection();

            #region IP Security Middleware
            app.UseIpSecurity();
            #endregion

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
