using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using VincallIntegration.Application;
using VincallIntegration.Infrastructure;
using VincallIntegration.Service.Models;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using VincallIntegration.Service.WebApiServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using VincallIntegration.Application.AutoMapper;

namespace VincallIntegration.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {                     
            services.AddControllersWithViews();
            services.AddAutoMapper(typeof(EntityProfile));
            services.AddMemoryCache();
            var connectionString = Configuration.GetConnectionString("Vincall");
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddEntityFrameworkSqlServer();
            services.AddDbContextPool<VincallDBContext>((sp, builder) =>
            {
                builder.UseSqlServer(connectionString, b => {
                    b.MigrationsAssembly(migrationAssembly);
                });
                builder.UseInternalServiceProvider(sp);
            });
          
            services.AddHttpApi<IComm100OauthClient>();           
            services.AddHttpApi<IVincallOAuthService>();           
            services.AddScoped(typeof(ICrudServices<>), typeof(CrudServices<>));
            services.AddScoped<ICrudServices, CrudServices>();
            services.AddScoped<GlobalSettingService>();
            services.AddScoped(s => (DbContext)s.GetRequiredService<VincallDBContext>());
            services.AddScoped<HostProvider>();

            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Vincall Service Api",
                    Version = "v1"
                });

            });
            var oauthUri = new Uri(Configuration["OauthUri"]);            

            services.AddCors(options =>
               options.AddPolicy("cors", p =>
               {
                   p.SetIsOriginAllowed((host) => true);
                   p.AllowAnyHeader();
                   p.AllowAnyMethod();
                   p.AllowCredentials();
                   p.SetPreflightMaxAge(TimeSpan.FromSeconds(24 * 60 * 60));
               }));


            services.AddAuthorization(options => {
                options.AddPolicy("Api", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "api");
                });
             });


            var x509Cert =new System.Security.Cryptography.X509Certificates.X509Certificate2("vincall.pfx");
            services.AddDataProtection()
                .PersistKeysToDbContext<VincallDBContext>()
                .ProtectKeysWithCertificate(x509Cert)
                .SetApplicationName("vincall");
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["OauthUri"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });
        }

        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors("cors");
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.josn", "v1");
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        
        
    }
}
