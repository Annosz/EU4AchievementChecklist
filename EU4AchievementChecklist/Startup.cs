using AspNet.Security.OpenId.Steam;
using EU4AchievementChecklist.Helpers.Middlewares;
using EU4AchievementChecklist.Helpers.Misc;
using EU4AchievementChecklist.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;

namespace EU4AchievementChecklist
{
    public class Startup
    {
        public Startup() { }

        public void ConfigureServices(IServiceCollection services)
        {
            // Same site cookie policy edits needed for external OpenIdConnect authentication
            // Based on https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext => SameSiteCookieCompatibility.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => SameSiteCookieCompatibility.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            // Solve Azure infinite redirect issue
            // Based on https://stackoverflow.com/questions/48479608/infinite-redirect-loop-in-asp-net-core-while-enforcing-ssl
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("20.40.202.15"), 0));
            });

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

            services.AddMemoryCache();

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
                app.UseForwardedHeaders();
                app.UseHsts();
                app.UseReverseProxyHttpsEnforcer();
            }

            app.UseCookiePolicy();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseResponseCaching();

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
