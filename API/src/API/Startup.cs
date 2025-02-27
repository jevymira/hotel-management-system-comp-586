using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Domain.Abstractions.Services;
using Domain.Abstractions.Repositories;
using Infrastructure.Abstractions.Database;
using Infrastructure.DynamoDB;
using Application.Abstractions.Services;
using Infrastructure.Repositories;
using Application.Services;
using Infrastructure.Services;
using Application.Abstractions.Factories;
using Application.Factories;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Application.Models;
using Microsoft.Extensions.Options;

namespace API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();
        services.AddScoped<IDBConnectionConfigFactory<AmazonDynamoDBConfig>,
                          DynamoDBConnectionConfigFactory>();
        services.AddScoped<IDBClientFactory<AmazonDynamoDBClient>,
                           DynamoDBClientFactory>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IJWTService, JWTService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IAdminAccountService, AdminAccountService>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IRoomReservationServiceFactory, RoomReservationServiceFactory>();
        services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();
        services.AddTransient<IImageService, ImageService>();
        services.Configure<JWTCredentials>(Configuration);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration.GetValue<string>("Issuer"),
                    ValidAudience = Configuration.GetValue<string>("Issuer"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        Configuration.GetValue<string>("Key")))
                };
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(builder => builder
            .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        );
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}