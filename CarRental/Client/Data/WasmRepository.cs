using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using CarRental.BaseRepository;
using CarRental.Controls.Grid;
using CarRental.Model;

namespace CarRental.Client.Data
{
    /// <summary>
    /// Vehicle implementation of the <see cref="IBasicRepository{Vehicle}"/>.
    /// </summary>
    public class WasmRepository : IBasicRepository<Vehicle>
    {
        private readonly HttpClient _apiVehicle;
        private readonly IVehicleFilters _controls;

        private const string ApiPrefix = "/api/";
        private string ApiVehicles => $"{ApiPrefix}vehicles/";
        private string ApiQuery => $"{ApiPrefix}query/";
        private string ForUpdate => "?forUpdate=true";

        /// <summary>
        /// Vehicle as loaded then modified by the user.
        /// </summary>
        public Vehicle OriginalVehicle { get; set; }

        /// <summary>
        /// Vehicle on the database.
        /// </summary>
        public Vehicle DatabaseVehicle { get; set; }

        /// <summary>
        /// The row version of the last vehicle loaded.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// This will serialize a response from JSON and return null
        /// if the status code is 404 - not found.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<TEntity> SafeGetFromJsonAsync<TEntity>(string url)
        {
            var result = await _apiVehicle.GetAsync(url);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadFromJsonAsync<TEntity>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WasmRepository"/>.
        /// </summary>
        /// <param name="vehicleFactory">The <see cref="IHttpClientFactory"/> for communication with the server.</param>
        /// <param name="controls">The <see cref="VehicleFilters"/> to parse queries and filters.</param>
        public WasmRepository(IHttpClientFactory vehicleFactory, IVehicleFilters controls)
        {
            _apiVehicle = vehicleFactory.CreateClient(Program.BaseVehicle);
            _controls = controls;
        }

        /// <summary>
        /// Add a vehicle
        /// </summary>
        /// <param name="item">The <see cref="Vehicle"/> to add.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The added <see cref="Vehicle"/>.</returns>
        public async Task<Vehicle> AddAsync(Vehicle item, ClaimsPrincipal user)
        {
            var result = await _apiVehicle.PostAsJsonAsync(ApiVehicles, item);
            return await result.Content.ReadFromJsonAsync<Vehicle>();
        }

        /// <summary>
        /// Delete a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Vehicle"/>.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param
        /// <returns><c>True</c> when successfully deleted.</returns>
        public async Task<bool> DeleteAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                await _apiVehicle.DeleteAsync($"{ApiVehicles}{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Vehicle"/> to load.</param>
        /// <param name="_">Unused <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="forUpdate"><c>True</c> when concurrency information should be loaded.</param>
        /// <returns></returns>
        public Task<Vehicle> LoadAsync(
            int id, 
            ClaimsPrincipal _, 
            bool forUpdate = false)
        {
            if (forUpdate)
            {
                return LoadAsync(id);
            }
            return SafeGetFromJsonAsync<Vehicle>($"{ApiVehicles}{id}");
        }

        /// <summary>
        /// Load a <see cref="Vehicle"/> for updates.
        /// </summary>
        /// <param name="id">The id of the <see cref="Vehicle"/> to load.</param>
        /// <returns></returns>
        public async Task<Vehicle> LoadAsync(int id)
        {
            OriginalVehicle = null;
            DatabaseVehicle = null;
            RowVersion = null;

            var result = await SafeGetFromJsonAsync<VehicleConcurrencyResolver>
                    ($"{ApiVehicles}{id}{ForUpdate}");

            if (result == null)
            {
                return null;
            }

            // our instance
            OriginalVehicle = result.OriginalVehicle;

            // save the version
            RowVersion = result.RowVersion;

            return result.OriginalVehicle;
        }

        /// <summary>
        /// Gets a page of <see cref="Vehicle"/> items.
        /// </summary>
        /// <returns>The result <see cref="ICollection{Vehicle}"/>.</returns>
        public async Task<ICollection<Vehicle>> GetListAsync()
        {
            var result = await _apiVehicle.PostAsJsonAsync(
                ApiQuery, _controls);
            var queryInfo = await result.Content.ReadFromJsonAsync<QueryResult>();

            // transfer page information
            _controls.PageHelper.Refresh(queryInfo.PageInfo);
            return queryInfo.Vehicles;
        }

        /// <summary>
        /// Update a <see cref="Vehicle"/> with concurrency checks.
        /// </summary>
        /// <param name="item">The <see cref="Vehicle"/> to update.</param>
        /// <param name="user">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The updated <see cref="Vehicle"/>.</returns>
        public async Task<Vehicle>
            UpdateAsync(Vehicle item, ClaimsPrincipal user)
        {
            // send down the vehicle with the version we have tracked
            var result = await _apiVehicle.PutAsJsonAsync(
                $"{ApiVehicles}{item.Id}",
                item.ToConcurrencyResolver(this));
            if (result.IsSuccessStatusCode)
            {
                return null;
            }
            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                // concurrency issue, so extract what the updated information is
                var resolver = await
                    result.Content.ReadFromJsonAsync<VehicleConcurrencyResolver>();
                DatabaseVehicle = resolver.DatabaseVehicle;
                var ex = new RepoConcurrencyException<Vehicle>(item, new Exception())
                {
                    DbEntity = resolver.DatabaseVehicle
                };
                RowVersion = resolver.RowVersion; // for override
                throw ex;
            }
            throw new HttpRequestException($"Bad status code: {result.StatusCode}");
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="item">Don't care.</param>
        public void Attach(Vehicle item)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Throws an <exception cref="NotImplementedException">exception</exception>.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable{Vehicle}"/> delegate.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task QueryAsync(Func<IQueryable<Vehicle>, Task> query)
        {
            return GetListAsync();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Task<TPropertyType> GetPropertyValueAsync<TPropertyType>(Vehicle item, string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task SetOriginalValueForConcurrencyAsync<TPropertyType>(Vehicle item, string propertyName, TPropertyType value)
        {
            throw new NotImplementedException();
        }
    }
}
