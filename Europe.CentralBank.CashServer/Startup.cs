using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Europe.CentralBank.CashServer.Models;
using Europe.CentralBank.CashServer.Utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Europe.CentralBank.CashServer {
    public class Startup {
        private readonly IConfigurationRoot _config;
        
        public Startup(IHostingEnvironment env) {
            _config = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services) {
            RsaSecurityKey _key;

            using (RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider()) {
                rsaKey.FromXmlString(_config["jwt"]);

                _key = new RsaSecurityKey(rsaKey.ExportParameters(true));
                services.AddSingleton(_key);
                services.AddSingleton(new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature));
            }
            
            var databaseConnectionString = _config["database"];

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>(true));
            
            services
                .AddSingleton(_key)
                .AddSingleton<CashValidator>()
                .AddScoped(a => new ApplicationDbContext(databaseConnectionString))
                .AddCors()
                .AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment())  {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseCors(policy => policy.SetPreflightMaxAge(TimeSpan.FromMinutes(10)).AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader())
                .UseMvc();
        }

        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
