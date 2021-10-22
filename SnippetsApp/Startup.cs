// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Graph;

namespace SnippetsApp
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
            // Add Microsoft Identity Platform sign-in
            services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options => {
                    Configuration.Bind("AzureAd", options);

                    options.Prompt = "select_account";

                    options.Events.OnTokenValidated = async context => {
                        var tokenAcquisition = context.HttpContext.RequestServices
                            .GetRequiredService<ITokenAcquisition>();

                        var graphClient = new GraphServiceClient(
                            new DelegateAuthenticationProvider(async (request) => {
                                var token = await tokenAcquisition
                                    .GetAccessTokenForUserAsync(GraphConstants.DefaultScopes, user:context.Principal);
                                request.Headers.Authorization =
                                    new AuthenticationHeaderValue("Bearer", token);
                            })
                        );

                        // Get user information from Graph
                        var user = await graphClient.Me.Request()
                            .Select(u => new {
                                u.DisplayName,
                                u.Mail,
                                u.UserPrincipalName,
                                u.MailboxSettings
                            })
                            .GetAsync();

                        context.Principal.AddUserGraphInfo(user);

                        if (context.Principal.IsPersonalAccount())
                        {
                            // Personal accounts do not support getting their
                            // photo via Graph
                            // Support is there in the beta API
                            context.Principal.AddUserGraphPhoto(null);
                            return;
                        }

                        // Get the user's photo
                        // If the user doesn't have a photo, this throws
                        try
                        {
                            var photo = await graphClient.Me
                                .Photos["48x48"]
                                .Content
                                .Request()
                                .GetAsync();

                            context.Principal.AddUserGraphPhoto(photo);
                        }
                        catch (ServiceException ex)
                        {
                            if (ex.IsMatch("ErrorItemNotFound"))
                            {
                                context.Principal.AddUserGraphPhoto(null);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    };

                    options.Events.OnAuthenticationFailed = context => {
                        var error = WebUtility.UrlEncode(context.Exception.Message);
                        context.Response
                            .Redirect($"/Home/ErrorWithMessage?message=Authentication+error&debug={error}");
                        context.HandleResponse();

                        return Task.FromResult(0);
                    };

                    options.Events.OnRemoteFailure = context => {
                        if (context.Failure is OpenIdConnectProtocolException)
                        {
                            var error = WebUtility.UrlEncode(context.Failure.Message);
                            context.Response
                                .Redirect($"/Home/ErrorWithMessage?message=Sign+in+error&debug={error}");
                            context.HandleResponse();
                        }

                        return Task.FromResult(0);
                    };
                })
                // Add ability to call web API (Graph)
                // and get access tokens
                .EnableTokenAcquisitionToCallDownstreamApi(options => {
                    Configuration.Bind("AzureAd", options);
                }, GraphConstants.DefaultScopes)
                // Add a GraphServiceClient via dependency injection
                .AddMicrosoftGraph(options => {
                    options.Scopes = string.Join(' ', GraphConstants.DefaultScopes);
                })
                // Use in-memory token cache
                // See https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization
                .AddInMemoryTokenCaches();

            // Require authentication
            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            // Add the Microsoft Identity UI pages for signin/out
            .AddMicrosoftIdentityUI();

            services.AddRazorPages()
                .AddRazorRuntimeCompilation();
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

            app.UseRouting();

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
