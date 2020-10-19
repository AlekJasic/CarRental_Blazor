using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CarRental.Server.Data;
using CarRental.Server.Models;
using CarRental.DataAccess;
using CarRental.Repository;
using CarRental.Model;
using CarRental.BaseRepository;
using Microsoft.AspNetCore.Identity;

namespace CarRental.Server
{
    public class Startup
    {
        private static readonly string DefaultConnection 
            = nameof(DefaultConnection);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationAuditDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString(DefaultConnection)));
            

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationAuditDbContext>();

            services.AddDbContextFactory<VehicleContext>(opt =>
                opt.UseSqlite(
                    Configuration.GetConnectionString(DefaultConnection))
                .EnableSensitiveDataLogging());

            // add the repository
            services.AddScoped<IRepository<Vehicle, VehicleContext>, 
                VehicleRepository>();
            services.AddScoped<IBasicRepository<Vehicle>>(sp =>
                sp.GetService<IRepository<Vehicle, VehicleContext>>());
            services.AddScoped<IUnitOfWork<Vehicle>, UnitOfWork<VehicleContext, Vehicle>>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationAuditDbContext>(options =>
                {
                    options.IdentityResources["profile"].UserClaims.Add("firstname");
                    options.IdentityResources["profile"].UserClaims.Add("lastname");
                });

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
