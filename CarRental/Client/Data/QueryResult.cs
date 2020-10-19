using System.Collections.Generic;
using CarRental.Controls.Grid;
using CarRental.Model;

namespace CarRental.Client.Data
{
    /// <summary>
    /// Result from query request.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// New <see cref="PageHelper"/> information.
        /// </summary>
        public PageHelper PageInfo { get; set; }

        /// <summary>
        /// A page of <see cref="ICollection{Vehicle}"./>
        /// </summary>
        public ICollection<Vehicle> Vehicles { get; set; }
    }
}
