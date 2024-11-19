using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Domain.Abstractions.Services;
using Domain.Abstractions.Repositories;
using Infrastructure.Abstractions.Database;
using Infrastructure.DynamoDB;
using Infrastructure.Abstractions.Services;
using Application.Abstractions.Services;
using Infrastructure.Repositories;
using Application.Services;
using Domain.Services;
using Infrastructure.Services;

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
        var jwtIssuer = Configuration.GetSection("Jwt:Issuer").Get<string>();
        var jwtKey =  Configuration.GetSection("Jwt:Key").Get<string>();

        services.AddCors();
        services.AddControllers();
        services.AddScoped<IDBConnectionConfigFactory<AmazonDynamoDBConfig>,
                          DynamoDBConnectionConfigFactory>();
        services.AddScoped<IDBClientFactory<AmazonDynamoDBClient>,
                           DynamoDBClientFactory>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IAdminAccountService, AdminAccountService>();
        services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();
        services.AddTransient<IImageService, ImageService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
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