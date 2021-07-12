using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Authn
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

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) previous scheme but jyst for cookies
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "GoogleOpenID";
            })
                .AddCookie(options => {
                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/denied";
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnSigningIn = async context =>
                        {
                            var principal = context.Principal;
                            if (principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                            {
                                if (principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value == "gerard")
                                {
                                    var claimsIdentity = principal.Identity as ClaimsIdentity;
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                                }
                            }
                            await Task.CompletedTask;
                        },
                        OnSignedIn = async context =>
                        {
                            await Task.CompletedTask;
                        },
                        OnValidatePrincipal = async context =>
                        {
                            await Task.CompletedTask;
                        }

                    };
                })
                .AddOpenIdConnect("GoogleOpenID", options =>
                {
                    options.Authority = "https://accounts.google.com";
                    options.ClientId = "695986022435-69hdlu11mk1oaq4u9gddvv5jlhihvb44.apps.googleusercontent.com";
                    options.ClientSecret = "YrpJF6PtmbOuZxUNNbsavn5a";
                    options.CallbackPath = "/auth";
                    options.SaveTokens = true;
                    options.Events = new OpenIdConnectEvents()
                    {
                        OnTokenValidated = async context =>
                        {
                        if (context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value == "112365468345858178959")
                            {
                                var claim = new Claim(ClaimTypes.Role, "Admin");
                                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                                claimsIdentity.AddClaim(claim);


                            }



                               
                        }
                    };
                });

                //.AddGoogle(options =>
                //{
                //    options.ClientId = "695986022435-69hdlu11mk1oaq4u9gddvv5jlhihvb44.apps.googleusercontent.com";
                //    options.ClientSecret = "YrpJF6PtmbOuZxUNNbsavn5a";
                //    options.CallbackPath = "/auth";
                //    options.AuthorizationEndpoint += "?prompt=consent";
                //});



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
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();

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
