using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.ManagementServer.Server.Models;
using VolunteerComputing.ManagementServer.Server.Hubs;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Newtonsoft.Json;

namespace VolunteerComputing.ManagementServer.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
                {
                    options.IdentityResources["openid"].UserClaims.Add("name");
                    options.ApiResources.Single().UserClaims.Add("name");
                    options.IdentityResources["openid"].UserClaims.Add("role");
                    options.ApiResources.Single().UserClaims.Add("role");
                });

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddSignalR()
                .AddNewtonsoftJsonProtocol(o => o.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .AddMessagePackProtocol();

            services.AddResponseCompression();

            services.AddControllersWithViews();
            services.AddRazorPages()
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
            };
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            app
                .UseHttpsRedirection()
                .UseBlazorFrameworkFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseResponseCompression()
                .UseForwardedHeaders(options)
                /*.Use(async (ctx, next) =>
                {
                    Console.WriteLine($"ctx.Request.Scheme: {ctx.Request.Scheme}; ctx.Request.Host: {ctx.Request.Host}; ctx.Request.Method: {ctx.Request.Method}");
                    //ctx.Request.Scheme = "https";
                    //ctx.Request.Host = new Microsoft.AspNetCore.Http.HostString("localhost:5001");
                    await next();
                })*/
                .UseIdentityServer()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("index.html");
                    endpoints.MapHub<TaskManagementHub>("/tasks");
                });

            ConfigureRoles(serviceProvider).Wait();
        }

        static async Task ConfigureRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            var editorRoleName = "Editor";
            var adminRoleName = "Admin";
            if (!await roleManager.RoleExistsAsync(editorRoleName))
                await roleManager.CreateAsync(new IdentityRole(editorRoleName));
            if (!await roleManager.RoleExistsAsync(adminRoleName))
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));

            var admin = await userManager.FindByNameAsync("a@b.c");
            if (admin != null && !await userManager.IsInRoleAsync(admin, adminRoleName))
            {
                await userManager.AddToRoleAsync(admin, adminRoleName);
            }
        }
    }
}
