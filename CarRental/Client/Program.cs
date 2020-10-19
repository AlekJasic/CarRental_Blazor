using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using CarRental.BaseRepository;
using CarRental.Client.Data;
using CarRental.Controls;
using CarRental.Controls.Grid;
using CarRental.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Client
{
    public class Program
    {
        public const string BaseVehicle = "CarRental.ServerAPI";
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>(nameof(App).ToLowerInvariant());

            builder.Services.AddHttpClient(BaseVehicle,
                client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp =>
            sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient(BaseVehicle));

            builder.Services.AddApiAuthorization();

            // vehicle implementation
            builder.Services.AddScoped<IBasicRepository<Vehicle>, WasmRepository>();
            builder.Services.AddScoped<IUnitOfWork<Vehicle>, WasmUnitOfWork>();

            // references to control filters and sorts
            builder.Services.AddScoped<IPageHelper, PageHelper>();
            builder.Services.AddScoped<IVehicleFilters, VehicleFilters>();
            builder.Services.AddScoped<GridQueryAdapter>();
            builder.Services.AddScoped<EditService>();

            builder.Services.AddScoped<LocalVehiclesStore>();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            // not used here but would be useful on the server
            builder.Services.AddScoped(sp =>
                new ClaimsPrincipal(new ClaimsIdentity()));

            await builder.Build().RunAsync();
        }
    }
}
