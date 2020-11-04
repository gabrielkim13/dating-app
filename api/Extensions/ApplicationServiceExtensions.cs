using System;
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
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        string connStr;

        // Depending on if in development or production, use either Heroku-provided
        // connection string, or development connection string from env var.
        if (env == "Development")
        {
          // Use connection string from file.
          connStr = config.GetConnectionString("DefaultConnection");
        }
        else
        {
          // Use connection string provided at runtime by Heroku.
          var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

          // Parse connection URL to connection string for Npgsql
          // postgres://mbnuyvccmlixkr:3d16b886ec9a028b0513446352000e7a3b84e615577133d96f04c1b9cc059239@ec2-52-73-199-211.compute-1.amazonaws.com:5432/d5f08c3paac23
          connUrl = connUrl.Replace("postgres://", string.Empty);
          var pgUserPass = connUrl.Split("@")[0];
          var pgHostPortDb = connUrl.Split("@")[1];
          var pgHostPort = pgHostPortDb.Split("/")[0];
          var pgDb = pgHostPortDb.Split("/")[1];
          var pgUser = pgUserPass.Split(":")[0];
          var pgPass = pgUserPass.Split(":")[1];
          var pgHost = pgHostPort.Split(":")[0];
          var pgPort = pgHostPort.Split(":")[1];

          connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb}";
        }

        // Whether the connection string came from the local development configuration file
        // or from the environment variable from Heroku, use it to set up your DbContext.
        options.UseNpgsql(connStr);
      });

      return services;
    }
  }
}
