using System;
using System.Threading.Tasks;
using CarRental.BaseRepository;
using CarRental.Model;
using CarRental.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Repository;
using VehicleConcurrencyResolver = CarRental.Client.Data.VehicleConcurrencyResolver;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Server.Controllers
{
    /// <summary>
    /// Services the <see cref="Vehicle"/> C.R.U.D. operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IBasicRepository<Vehicle> _repo;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="VehiclesController"/>.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Vehicle}"/> repo to work with.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> for dependency resolution.</param>
        public VehiclesController(IBasicRepository<Vehicle> repo,
            IServiceProvider provider)
        {
            _repo = repo;
            _serviceProvider = provider;
        }

        /// <summary>
        /// Get a <see cref="Vehicle"/>.
        /// </summary>
        /// <example>GET /api/vehicles/1?forUpdate=true</example>
        /// <param name="id">The id of the <see cref="Vehicle"/>.</param>
        /// <param name="forUpdate"><c>True</c> to fetch additional concurrency info.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id,
            [FromQuery] bool forUpdate = false)
        {
            if (id < 1)
            {
                return new NotFoundResult();
            }
            if (forUpdate)
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork<Vehicle>>();
                HttpContext.Response.RegisterForDispose(unitOfWork);
                var result = await unitOfWork.Repo.LoadAsync(id, User, true);

                // return version for tracking on vehicle. It is not
                // part of the C# class so it is tracked as a "shadow property"
                var concurrencyResult = new VehicleConcurrencyResolver
                {
                    OriginalVehicle = result,
                    RowVersion = result == null ? null :
                    await unitOfWork.Repo.GetPropertyValueAsync<byte[]>(
                        result, VehicleContext.RowVersion)
                };
                return new OkObjectResult(concurrencyResult);
            }
            else
            {
                var result = await _repo.LoadAsync(id, User);
                return result == null ? (IActionResult)new NotFoundResult() :
                    new OkObjectResult(result);
            }
        }

        /// <summary>
        /// Add a new <see cref="Vehicle"/>.
        /// </summary>
        /// <example>POST /api/vehicles</example>
        /// <param name="vehicle"></param>
        /// <returns>The <see cref="Vehicle"/> with id.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync(
            [FromBody] Vehicle vehicle)
        {
            vehicle.Tank = Vehicles.Shared.FuelLevel.Full.ToString(); 
            return vehicle == null
                ? new BadRequestResult()
                : ModelState.IsValid ?
                new OkObjectResult(await _repo.AddAsync(vehicle, User)) :
                (IActionResult)new BadRequestObjectResult(ModelState);
        }

        /// <summary>
        /// Update a <see cref="Vehicle"/>.
        /// </summary>
        /// <example>PUT /api/vehicles/1</example>
        /// <param name="id">The id of the <see cref="Vehicle"/>.</param>
        /// <param name="value">The <see cref="VehicleConcurrencyResolver"/> payload.</param>
        /// <returns>An <see cref="IActionResult"/> of OK or Conflict.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id,
            [FromBody] VehicleConcurrencyResolver value)
        {
            // I got nothing
            if (value == null || value.OriginalVehicle == null
                || value.OriginalVehicle.Id != id)
            {
                return new BadRequestResult();
            }
            if (ModelState.IsValid)
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork<Vehicle>>();
                HttpContext.Response.RegisterForDispose(unitOfWork);
                unitOfWork.SetUser(User);
                // this gets the vehicle on the board for EF Core
                unitOfWork.Repo.Attach(value.OriginalVehicle);
                await unitOfWork.Repo.SetOriginalValueForConcurrencyAsync(
                    value.OriginalVehicle, VehicleContext.RowVersion, value.RowVersion);
                try
                {
                    await unitOfWork.CommitAsync();
                    return new OkResult();
                }
                catch (RepoConcurrencyException<Vehicle> dbex)
                {
                    // oops it has been updated, so send back the database version
                    // and the new RowVersion in case the user wants to override
                    value.DatabaseVehicle = dbex.DbEntity;
                    value.RowVersion = dbex.RowVersion;
                    return new ConflictObjectResult(value);
                }
            }
            else
            {
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Delete a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Vehicle"/> to delete.</param>
        /// <returns>The <see cref="IActionResult"/>.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var result = await _repo.DeleteAsync(id, User);
                return result ?
                    new OkResult() :
                    (IActionResult)new NotFoundResult();
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
