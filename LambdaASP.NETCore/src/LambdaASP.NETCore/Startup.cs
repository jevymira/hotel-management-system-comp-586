using Abstractions;
using DynamoDB;
using Amazon.DynamoDBv2;

namespace LambdaASP.NETCore
{
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
            services.AddControllers();
            services.AddScoped<IDBConnectionConfigFactory<AmazonDynamoDBConfig>,
                              DynamoDBConnectionConfigFactory>();
            services.AddScoped<IDBClientFactory<AmazonDynamoDBClient>,
                               DynamoDBClientFactory>();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder => builder
                .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );
            //.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader()

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}