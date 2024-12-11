using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Abstractions.Services;
using Application.Services;
using Application.Abstractions.Factories;
using Application.Factories;
using Application.Abstractions.Repositories;
using Application.Services.Repository;
using Application.DynamoDB;
using Application.Repositories;
using Application.Abstractions.Database;

namespace API;

public class Startup
{
    public Startup()
    {
        
    }

    public DynamoDBClientFactory database
    {
        get => default;
        set
        {
        }
    }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices()
    {
        //try
        //{
        //    SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
        //                Configuration.GetSection("Jwt:Key").Get<string>()));
        //    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(options =>
        //    {
        //        options.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            ValidIssuer = Configuration.GetSection("Jwt:Issuer").Get<string>(),
        //            ValidAudience = Configuration.GetSection("Jwt:Issuer").Get<string>(),
        //            IssuerSigningKey = key
        //        };
        //    });
        //}
        //catch (ArgumentNullException)
        //{

        //}

        //services.AddCors();
        //services.AddControllers();
        //services.AddScoped<IDBConnectionConfigFactory<AmazonDynamoDBConfig>,
        //                  DynamoDBConnectionConfigFactory>();
        //services.AddScoped<IDBClientFactory<AmazonDynamoDBClient>,
        //                   DynamoDBClientFactory>();
        //services.AddScoped<IAuthenticationService, AuthenticationService>();
        //services.AddScoped<IJWTService, JWTService>();
        //services.AddScoped<IRoomService, RoomService>();
        //services.AddScoped<IReservationService, ReservationService>();
        //services.AddScoped<IAdminAccountService, AdminAccountService>();
        //services.AddScoped<IRoomRepository, RoomRepository>();
        //services.AddScoped<IReservationRepository, ReservationRepository>();
        //services.AddScoped<IRoomReservationServiceFactory, RoomReservationServiceFactory>();
        //services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();
        //services.AddTransient<IImageService, ImageService>();
        ////services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        ////    .AddJwtBearer(options =>
        ////    {
        ////        options.TokenValidationParameters = new TokenValidationParameters
        ////        {
        ////            ValidateIssuer = true,
        ////            ValidateAudience = true,
        ////            ValidateLifetime = true,
        ////            ValidateIssuerSigningKey = true,
        ////            ValidIssuer = Configuration.GetSection("Jwt:Issuer").Get<string>(),
        ////            ValidAudience = Configuration.GetSection("Jwt:Issuer").Get<string>(),
        ////            IssuerSigningKey = key
        ////        };
        ////    });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure()
    {
        //if (env.IsDevelopment())
        //{
        //    app.UseDeveloperExceptionPage();
        //}

        //app.UseCors(builder => builder
        //    .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        //);
        //app.UseHttpsRedirection();
        //app.UseRouting();
        //app.UseAuthentication();
        //app.UseAuthorization();
        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllers();
        //});
    }
}