using CarRental.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CarRental.Controls
{
    /// <summary>
    /// Creates the right expressions to filter and sort.
    /// </summary>
    public class GridQueryAdapter
    {
        /// <summary>
        /// Holds state of the grid.
        /// </summary>
        private readonly IVehicleFilters _controls;

        /// <summary>
        /// Expressions for sorting.
        /// </summary>
        private readonly Dictionary<VehicleFilterColumns, Expression<Func<Vehicle, string>>> _expressions
            = new Dictionary<VehicleFilterColumns, Expression<Func<Vehicle, string>>>
            {
                { VehicleFilterColumns.LicenseNumber, c => c.LicenseNumber },
                { VehicleFilterColumns.Brand, c => c.Brand },
                { VehicleFilterColumns.Model, c => c.Model }            
            };

        /// <summary>
        /// Queryables for filtering.
        /// </summary>
        private readonly Dictionary<VehicleFilterColumns, Func<IQueryable<Vehicle>, IQueryable<Vehicle>>> _filterQueries;

        /// <summary>
        /// Creates a new instance of the <see cref="GridQueryAdapter"/> class.
        /// </summary>
        /// <param name="controls">The <see cref="IVehicleFilters"/> to use.</param>
        public GridQueryAdapter(IVehicleFilters controls)
        {
            _controls = controls;

            // set up queries
            _filterQueries = new Dictionary<VehicleFilterColumns, Func<IQueryable<Vehicle>, IQueryable<Vehicle>>>
            {
                { VehicleFilterColumns.LicenseNumber, cs => cs.Where(c => c.LicenseNumber.Contains(_controls.FilterText)) },
                { VehicleFilterColumns.Brand, cs => cs.Where(c => c.Brand.Contains(_controls.FilterText)) },
                { VehicleFilterColumns.Model, cs => cs.Where(c => c.Model.Contains(_controls.FilterText)) }
            };
        }

        /// <summary>
        /// Uses the query to return a count and a page.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable{Vehicle}"/> to work from.</param>
        /// <returns>The resulting <see cref="ICollection{Vehicle}"/>.</returns>
        public async Task<ICollection<Vehicle>> FetchAsync(IQueryable<Vehicle> query)
        {
            query = FilterAndQuery(query);
            await CountAsync(query);
            var collection = await FetchPageQuery(query)
                .ToListAsync();
            if (collection.Count >= _controls.PageHelper.PageSize)
            {
                _controls.PageHelper.PageItems = _controls.PageHelper.PageSize * _controls.PageHelper.Page;
            }
            else
            {
                _controls.PageHelper.PageItems = collection.Count;
            }
            return collection;
        }

        /// <summary>
        /// Get total filtered items count.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable{Vehicle}"/> to use.</param>
        /// <returns>Asynchronous <see cref="Task"/>.</returns>
        public async Task CountAsync(IQueryable<Vehicle> query)
        {
            _controls.PageHelper.TotalItemCount = await query.CountAsync();
        }

        /// <summary>
        /// Build the query to bring back a single page.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable{Vehicle}"/> to modify.</param>
        /// <returns>The new <see cref="IQueryable{Vehicle}"/> for a page.</returns>
        public IQueryable<Vehicle> FetchPageQuery(IQueryable<Vehicle> query)
        {
            return query
                .Skip(_controls.PageHelper.Skip)
                .Take(_controls.PageHelper.PageSize)
                .AsNoTracking();
        }

        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <param name="root">The <see cref="IQueryable{Vehicle}"/> to start with.</param>
        /// <returns>
        /// The resulting <see cref="IQueryable{Vehicle}"/> with sorts and
        /// filters applied.
        /// </returns>
        private IQueryable<Vehicle> FilterAndQuery(IQueryable<Vehicle> root)
        {
            var sb = new System.Text.StringBuilder();

            // apply a filter?
            if (!string.IsNullOrWhiteSpace(_controls.FilterText))
            {
                var filter = _filterQueries[_controls.FilterColumn];
                sb.Append($"Filter: '{_controls.FilterColumn}' ");
                root = filter(root);
            }

            // apply the expression
            var expression = _expressions[_controls.SortColumn];
            sb.Append($"Sort: '{_controls.SortColumn}' ");

            var sortDir = _controls.SortAscending ? "ASC" : "DESC";
            sb.Append(sortDir);

            Debug.WriteLine(sb.ToString());
            // return the unfiltered query for total count, and the filtered for fetching
            return _controls.SortAscending ? root.OrderBy(expression)
                : root.OrderByDescending(expression);
        }
    }
}
