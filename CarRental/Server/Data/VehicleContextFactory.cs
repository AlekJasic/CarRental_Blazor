using CarRental.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace CarRental.Server.Data
{
    /// <summary>
    /// Factory to enable running migrations from the command line
    /// </summary>
    public class VehicleContextFactory : IDesignTimeDbContextFactory<VehicleContext>
    {
        public VehicleContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment")
                ?? "Development";
            var basePath = AppContext.BaseDirectory;

            // grab connection string
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString(VehicleContext.CarRentalDb);
            var optionsBuilder = new DbContextOptionsBuilder<VehicleContext>();
            
            // use SQL Server and place migrations in this assembly
            optionsBuilder.UseSqlServer(connstr, builder =>
            builder.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
            return new VehicleContext(optionsBuilder.Options);
        }
    }
}
