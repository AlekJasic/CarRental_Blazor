using CarRental.DataAccess;
using CarRental.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarRental.Repository
{
    /// <summary>
    /// Implementation of repository for <see cref="VehicleContext"/>.
    /// </summary>
    public class VehicleRepository : IRepository<Vehicle, VehicleContext>
    {
        /// <summary>
        /// Factory to create contexts.
        /// </summary>
        private readonly DbContextFactory<VehicleContext> _factory;
        private bool disposedValue;

        /// <summary>
        /// For longer tracked work.
        /// </summary>
        public VehicleContext PersistedContext { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="VehicleRepository"/> class.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="DbContextFactory{VehicleContext}"/>
        /// to use.
        /// </param>
        public VehicleRepository(DbContextFactory<VehicleContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Performs some work, either using the peristed context or
        /// by generating a new context for the operation.
        /// </summary>
        /// <param name="work">The work to perform (passed a <see cref="VehicleContext"/>).</param>
        /// <param name="user">The current <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="saveChanges"><c>True</c> to save changes when done.</param>
        /// <returns></returns>
        private async Task WorkInContextAsync(
            Func<VehicleContext, Task> work, 
            ClaimsPrincipal user,
            bool saveChanges = false)
        {
            if (PersistedContext != null)
            {
                if (user != null)
                {
                    PersistedContext.User = user;
                }
                // do some work. Save changes flag is ignored because this will be
                // committed later.
                await work(PersistedContext);
            }
            else
            {
                using (var context = _factory.CreateDbContext())
                {
                    context.User = user;
                    await work(context);
                    if (saveChanges)
                    {
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Attaches an item to the <see cref="VehicleContext"/>.
        /// </summary>
        /// <param name="item">The instance of the <see cref="Vehicle"/>.</param>
        public void Attach(Vehicle item)
        {
            if (PersistedContext == null)
            {
                throw new InvalidOperationException("Only valid in a unit of work.");
            }
            PersistedContext.Attach(item);
        }

        /// <summary>
        /// Adds a new <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="item">The <see cref="Vehicle"/> to add.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The <see cref="Vehicle"/> with id set.</returns>
        public async Task<Vehicle> AddAsync(Vehicle item, ClaimsPrincipal user)
        {
            await WorkInContextAsync(context =>
            {
                context.Vehicles.Add(item);
                return Task.CompletedTask;
            }, user, true);
            return item;
        }

        /// <summary>
        /// Delete a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Vehicle"/> to delete.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns><c>True</c> when found and deleted.</returns>
        public async Task<bool> DeleteAsync(int id, ClaimsPrincipal user)
        {
            bool? result = null;
            await WorkInContextAsync(async context =>
            {
                var item = await context.Vehicles.SingleOrDefaultAsync(c => c.Id == id);
                if (item == null)
                {
                    result = false;
                }
                else
                {
                    context.Vehicles.Remove(item);
                }
            }, user, true);
            if (!result.HasValue)
            {
                result = true;
            }
            return result.Value;
        }

        /// <summary>
        /// Not implemented here (see the Blazor WebAssembly client).
        /// </summary>
        /// <returns>The <see cref="ICollection{Vehicle}"/>.</returns>
        public Task<ICollection<Vehicle>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Vehicle"/> to load.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="forUpdate"><c>True</c> to keep tracking on.</param>
        /// <returns>The <see cref="Vehicle"/>.</returns>
        public async Task<Vehicle> LoadAsync(
            int id, 
            ClaimsPrincipal user,
            bool forUpdate = false)
        {
            Vehicle vehicle = null;
            await WorkInContextAsync(async context =>
            {
                var vehicleRef = context.Vehicles;
                if (forUpdate)
                {
                    vehicleRef.AsNoTracking();
                }
                vehicle = await vehicleRef
                    .SingleOrDefaultAsync(c => c.Id == id);
            }, user);
            return vehicle;
        }

        /// <summary>
        /// Query the <see cref="Vehicle"/> database.
        /// </summary>
        /// <param name="query">
        /// A delegate that provides an <see cref="IQueryable{Vehicle}"/>
        /// to build on.
        /// </param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task QueryAsync(Func<IQueryable<Vehicle>, Task> query)
        {
            await WorkInContextAsync(async context =>
            {
                await query(context.Vehicles.AsNoTracking().AsQueryable());
            }, null);
        }

        /// <summary>
        /// Update the <see cref="Vehicle"/> (without a unit of work).
        /// </summary>
        /// <param name="item">The <see cref="Vehicle"/> to update.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The updated <see cref="Vehicle"/>.</returns>
        public async Task<Vehicle> UpdateAsync(Vehicle item, ClaimsPrincipal user)
        {
            await WorkInContextAsync(context =>
            {
                context.Vehicles.Attach(item);
                return Task.CompletedTask;
            }, user, true);
            return item;
        }

        /// <summary>
        /// Grabs the value of a property. Useful for shadow properties.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="item">The <see cref="Vehicle"/> the property is on.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        public async Task<TPropertyType> GetPropertyValueAsync<TPropertyType>(
            Vehicle item, string propertyName)
        {
            TPropertyType value = default;
            await WorkInContextAsync(context =>
            {
                value = context.Entry(item)
                .Property<TPropertyType>(propertyName).CurrentValue;
                return Task.CompletedTask;
            }, null);
            return value;
        }

        /// <summary>
        /// Sets original value. This is useful to check concurrency if you have
        /// disconnected entities and are re-attaching to update.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="item">The <see cref="Vehicle"/> being tracked.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SetOriginalValueForConcurrencyAsync<TPropertyType>(
            Vehicle item, 
            string propertyName,
            TPropertyType value)
        {
            await WorkInContextAsync(context =>
            {
                var tracked = context.Entry(item);
                // we tell EF Core what version we loaded
                tracked.Property<TPropertyType>(propertyName).OriginalValue =
                        value;
                // we tell EF Core to modify entity
                tracked.State = EntityState.Modified;
                return Task.CompletedTask;
            }, null);            
        }

        /// <summary>
        /// Implements the dipose pattern.
        /// </summary>
        /// <param name="disposing"><c>True</c> when disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (PersistedContext != null)
                    {
                        PersistedContext.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Implement <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
