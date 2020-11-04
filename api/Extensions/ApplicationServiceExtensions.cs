using api.Data;
using api.Helpers;
using api.Interfaces;
using api.Services;
using api.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace api.Extensions
{
  public static class ApplicationServiceExtensions
  {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddSingleton<PresenceTracker>();

      services.AddScoped<ITokenService, TokenService>();
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      services.AddScoped<IPhotoService, PhotoService>();
      services.AddScoped<LogUserActivity>();

      services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

      services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

      services.AddDbContext<DataContext>(options =>
      {
        options.UseSqlite(config.GetConnectionString("DefaultConnection"));
      });

      return services;
    }
  }
}
