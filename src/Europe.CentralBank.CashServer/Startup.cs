using System;
using System.Security.Cryptography;
using System.Xml;
using Europe.CentralBank.CashServer.Models;
using Europe.CentralBank.CashServer.Utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Europe.CentralBank.CashServer {
    public class Startup {
        private readonly IConfiguration _config;
        
        public Startup(IConfiguration config) {
            _config = config;
        }

        private static RSAParameters LoadKey(string xmlString) {
            RSAParameters parameters = new RSAParameters();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            
            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue")) {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) {
                    switch (node.Name) {
                        case "Modulus": parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Exponent": parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "P": parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Q": parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DP": parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DQ": parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "InverseQ": parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "D": parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                    }
                }
            } else {
                throw new Exception("Invalid XML RSA key.");
            }

            return parameters;
        }

        public void ConfigureServices(IServiceCollection services) {
            var a = new RSACryptoServiceProvider();
            a.ImportParameters(LoadKey(_config["jwt"]));

            services.AddSingleton(a);
            
            var _key = new RsaSecurityKey(a.ExportParameters(true));
            services.AddSingleton(_key);
            services.AddSingleton(new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature));
                
            var databaseConnectionString = _config["database"];

            services
                .AddSingleton<CashValidator>()
                .AddDbContext<ApplicationDbContext>(options => options.UseMySql(databaseConnectionString,
                    mysqlOptions => {
                        mysqlOptions.ServerVersion(new Version(8, 0, 12), ServerType.MySql);
                    }
                ))
                .AddCors()
                .AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
                scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            }
            
            if (env.IsDevelopment())  {
                app.UseDeveloperExceptionPage();
            }

            app
                .Use(async (context, next) => {
                    try {
                        await next();
                    } catch (Exception e) {
                        context.Response.StatusCode = e is NotImplementedException ? 404 : e is UnauthorizedAccessException || e is SecurityTokenValidationException ? 401 : e is ArgumentException ? 400 : 500;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        string message = "";

                        Exception x = e;
                        do {
                            message += x.Message + "\r\n\r\n";
                        } while ((x = x.InnerException) != null);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                            code = -1,
                            message = message.Substring(0, message.Length - 4),
                            stacktrace = e.StackTrace
                        }));
                    }
                })
                .UseCors(policy => policy.SetPreflightMaxAge(TimeSpan.FromMinutes(10)).AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader())
                .UseMvc();
        }

        public static void Main(string[] args) {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    context.Configuration = config.Build();
                })
                .UseStartup<Startup>()
                .UseIISIntegration()
                .Build();
    }
}
