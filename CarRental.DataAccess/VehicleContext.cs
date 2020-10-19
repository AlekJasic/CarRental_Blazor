using CarRental.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CarRental.DataAccess
{
    /// <summary>
    /// Context for the vehicles database.
    /// </summary>
    public class VehicleContext : DbContext, ISupportUser
    {
        /// <summary>
        /// Tracking lifetime of contexts.
        /// </summary>
        private readonly Guid _id;

        /// <summary>
        /// For audit info
        /// </summary>
        private readonly VehicleAuditAdapter _adapter = new VehicleAuditAdapter();

        /// <summary>
        /// The logged in <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Magic string.
        /// </summary>
        public static readonly string RowVersion = nameof(RowVersion);

        /// <summary>
        /// Who created it?
        /// </summary>
        public static readonly string CreatedBy = nameof(CreatedBy);

        /// <summary>
        /// When was it created?
        /// </summary>
        public static readonly string CreatedOn = nameof(CreatedOn);

        /// <summary>
        /// Who last modified it?
        /// </summary>
        public static readonly string ModifiedBy = nameof(ModifiedBy);

        /// <summary>
        /// When was it last modified?
        /// </summary>
        public static readonly string ModifiedOn = nameof(ModifiedOn);

        /// <summary>
        /// Magic strings.
        /// </summary>
        public static readonly string CarRentalDb =
            nameof(CarRentalDb);

        /// <summary>
        /// Inject options.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{VehicleContext}"/>
        /// for the context
        /// </param>
        public VehicleContext(DbContextOptions<VehicleContext> options)
            : base(options)
        {
            _id = Guid.NewGuid();
            Debug.WriteLine($"{_id} context created.");
        }

        /// <summary>
        /// Override the save operation to generate audit information.
        /// </summary>
        /// <param name="token">The <seealso cref="CancellationToken"/>.</param>
        /// <returns>The result.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken token
            = default)
        {
            return await _adapter.ProcessVehicleChangesAsync(
                User, this, async () => await base.SaveChangesAsync(token));            
        }

        /// <summary>
        /// List of <see cref="Vehicle"/>.
        /// </summary>
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleAudit> VehicleAudits { get; set; }

        /// <summary>
        /// Define the model.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var vehicle = modelBuilder.Entity<Vehicle>();

            // this property isn't on the C# class
            // so we set it up as a "shadow" property and use it for concurrency
            vehicle.Property<byte[]>(RowVersion).IsRowVersion();

            // audit fields
            vehicle.Property<string>(ModifiedBy);
            vehicle.Property<DateTimeOffset>(ModifiedOn);
            vehicle.Property<string>(CreatedBy);
            vehicle.Property<DateTimeOffset>(CreatedOn);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        public override void Dispose()
        {
            Debug.WriteLine($"{_id} context disposed.");
            base.Dispose();
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/></returns>
        public override ValueTask DisposeAsync()
        {
            Debug.WriteLine($"{_id} context disposed async.");
            return base.DisposeAsync();
        }
    }
}
