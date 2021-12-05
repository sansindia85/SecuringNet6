using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            //Claims transformation : Keeping the original claim types
            //Clear the browsing data in the browser
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            // create an HttpClient used for accessing the API
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44366/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            //Create an HttpClient used for accessing the IDP
            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44318");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });
           

            //Logging in with authentication code flow
            services.AddAuthentication(options =>
            {
                //Different domains require different schemas. So that cookies do not interfere with each other.
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options=>
            {
                options.AccessDeniedPath = "/Authorization/AccessDenied";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options=>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                
                //To read metadata from discovery end point .well-known/openid-configuration. So that it will discover other endpoints.
                options.Authority = "https://localhost:44318/"; 
                
                //Match the client id at the idp level
                options.ClientId = "imagegalleryclient";
                
                //Use the Authorization code flow.
                options.ResponseType = "code";

                //For each request to the authorization endpoint a secret is created.
                options.UsePkce = true;
                
                //In case if need to change the sigin-oidc configured at "https://localhost:44389/signin-oidc"
                //optiions.CallbackPath = new PathString("....")
                //Available by default w.r.t openid and profile. We are adding it to see what exactly is going on.
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("address");
                options.Scope.Add("roles");
                options.SaveTokens = true;
                options.ClientSecret = "secret";

                #region Redirect after logging out

                //With this we can set the URI we can redirect after logging out from Identity Provider
                //options.SignedOutCallbackPath

                #endregion

                #region Get addition claims from UserInfoEnd point
                options.GetClaimsFromUserInfoEndpoint = true;
                #endregion

                //Just for demo purpose
                //options.ClaimActions.Remove("nbf");
                //Address is not there in the claims identity
              
                //It contains sid (session id) and idp which can be removed at the level of idp
                //sid and idp are default claims and not part of scope.
                options.ClaimActions.DeleteClaim("sid");
                options.ClaimActions.DeleteClaim("idp");
                options.ClaimActions.DeleteClaim("s_hash");
                options.ClaimActions.DeleteClaim("auth_time");

                //The first parameter is the the claim type role returned from Identity Provider.
                //The second parameter is the claim type we want to include as part of claims identity.
                options.ClaimActions.MapUniqueJsonKey("role", "role");

                //It allows to specify role claim types
                //Role based authorization - To access user role in view _Layout.cshtml
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role
                };
            });

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
 
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Best place to add in between routing and useendpoints
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
