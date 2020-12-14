using AspNet.Security.OpenId.Steam;
using EU4AchievementChecklist.Helpers.Middlewares;
using EU4AchievementChecklist.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace EU4AchievementChecklist
{
    public class Startup
    {
        public Startup() { }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = SteamAuthenticationDefaults.AuthenticationScheme;
                    })
                .AddCookie()
                .AddSteam(SteamAuthenticationDefaults.AuthenticationScheme, steamOptions =>
                {
                    steamOptions.ApplicationKey = Environment.GetEnvironmentVariable("SteamAPIKey");
                    steamOptions.ClaimsIssuer = SteamAuthenticationDefaults.Authority;
                });

            services.AddScoped<WikiService>();
            services.AddScoped<SteamService>();

            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseReverseProxyHttpsEnforcer();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
