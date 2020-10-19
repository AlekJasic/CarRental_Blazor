using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Model;
using CarRental.DataAccess;
using CarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BaseRepository;
using System;
using Microsoft.Extensions.DependencyInjection;
using CarRental.Controls;

namespace CarRental.Server.Controllers
{
    /// <summary>
    /// Controller for queries of <see cref="Vehicle"/>.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QueryController : ControllerBase
    {
        private readonly IBasicRepository<Vehicle> _repo;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="QueryController"/>.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Vehicle}"/> repo to use.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> for dependency resolution.</param>
        public QueryController(IBasicRepository<Vehicle> repo,
            IServiceProvider provider)
        {
            _repo = repo;
            _serviceProvider = provider;
        }

        /// <summary>
        /// This is a POST to take on filter information.
        /// </summary>
        /// <example>POST /api/query</example>
        /// <param name="filter">The <see cref="VehicleFilter"/> to apply.</param>
        /// <returns>An <see cref="ICollection{Vehicle}"/>.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync(
            [FromBody] VehicleFilter filter)
        {
           
            var adapter = new GridQueryAdapter(filter);
            ICollection<Vehicle> vehicles = null;
            // this call both executes a count to get total items and
            // updates the paging information
            await _repo.QueryAsync(
                async query => vehicles = await adapter.FetchAsync(query));
            return new OkObjectResult(new
            {
                PageInfo = filter.PageHelper,
                Vehicles = vehicles
            });
        }
    }
}
