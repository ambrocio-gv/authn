using Authn.Data;
using Authn.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
            services.AddDbContext<AuthDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<UserService>();


            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) previous scheme but jyst for cookies
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = "okta";
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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

                    //options.Events = new CookieAuthenticationEvents()
                    //{
                    //   OnSigningIn = async context =>
                    //   {
                    //       var scheme = context.Properties.Items.Where(k => k.Key == ".AuthScheme").FirstOrDefault();
                    //       var claim = new Claim(scheme.Key, scheme.Value);
                    //       var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                    //       claimsIdentity.AddClaim(claim);


                    //   }
                    //};
                })

                // google built-in using nuget package, this is built on 
                //.AddGoogle(options =>
                //{
                //    options.ClientId = "695986022435-69hdlu11mk1oaq4u9gddvv5jlhihvb44.apps.googleusercontent.com";
                //    options.ClientSecret = "YrpJF6PtmbOuZxUNNbsavn5a";
                //    options.CallbackPath = "/auth";
                //    options.AuthorizationEndpoint += "?prompt=consent";
                //});


                .AddOpenIdConnect("google", options =>
                {
                    //options.Authority = "https://accounts.google.com";
                    //options.ClientId = "695986022435-69hdlu11mk1oaq4u9gddvv5jlhihvb44.apps.googleusercontent.com";
                    //options.ClientSecret = "YrpJF6PtmbOuZxUNNbsavn5a";
                    //options.CallbackPath = "/auth";


                    options.Authority = Configuration["GoogleOpenId:Authority"];
                    options.ClientId = Configuration["GoogleOpenId:ClientId"];
                    options.ClientSecret = Configuration["GoogleOpenId:ClientSecret"];
                    options.CallbackPath = Configuration["GoogleOpenId:CallbackPath"];



                    options.SaveTokens = true;
                    options.Events = new OpenIdConnectEvents()
                    {
                        OnTokenValidated = async context =>
                        {
                        if (context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value == "112365468345858178959")//name identifier got through debug
                            {
                                var claim = new Claim(ClaimTypes.Role, "Admin");
                                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                                claimsIdentity.AddClaim(claim);


                            }



                               
                        }
                    };
                })

                //OKTA is an IDP
                .AddOpenIdConnect("okta", options =>
                {
                    //options.Authority = "https://dev-01777934.okta.com";// this is initial
                    //options.Authority = "https://dev-01777934.okta.com/oauth2/default";                    
                    //options.ClientId = "0oa17zw88376EOkdB5d7";                   
                    //options.ClientSecret = "WaigDHaFGSA8UzdiYzEMiMmFrY-A-qbTfLy5BfPR";
                    //options.CallbackPath = "/okta-auth";
                    //options.SignedOutCallbackPath = "/okta-signout";

                    options.Authority = Configuration["OktaOpenId:Authority"];
                    options.ClientId = Configuration["OktaOpenId:ClientId"];
                    options.ClientSecret = Configuration["OktaOpenId:ClientSecret"];
                    options.CallbackPath = Configuration["OktaOpenId:CallbackPath"];
                    options.SignedOutCallbackPath = Configuration["OktaOpenId:SignedOutCallbackPath"];


                    options.ResponseType = OpenIdConnectResponseType.Code; // this is just a constant called "code"
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.SaveTokens = true;
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
