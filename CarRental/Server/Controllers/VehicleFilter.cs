using CarRental.Controls.Grid;
using CarRental.Model;

namespace CarRental.Server.Controllers
{
    /// <summary>
    /// Simple implementation of <see cref="IVehicleFilters"/> for
    /// serialization across REST endpoints.
    /// </summary>
    public class VehicleFilter : IVehicleFilters
    {
        /// <summary>
        /// Initializes an instance of the <see cref="VehicleFilter"/> class.
        /// </summary>
        public VehicleFilter()
        {
            PageHelper = new PageHelper();
        }

        /// <summary>
        /// The <see cref="VehicleFilterColumns"/> being filtered on.
        /// </summary>
        public VehicleFilterColumns FilterColumn { get; set; }

        /// <summary>
        /// The text of the filter.
        /// </summary>
        public string FilterText { get; set; }

        /// <summary>
        /// Loading indicator.
        /// </summary>
        public bool Loading { get; set; }

        /// <summary>
        /// Paging state.
        /// </summary>
        public PageHelper PageHelper { get; set; }
      
        /// <summary>
        /// Gets or sets a value indicating if the name is rendered first name first.
        /// </summary>
        public bool ShowFirstNameFirst { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the sort is ascending or descending.
        /// </summary>
        public bool SortAscending { get; set; }

        /// <summary>
        /// The <see cref="VehicleFilterColumns"/> being sorted.
        /// </summary>
        public VehicleFilterColumns SortColumn { get; set; }

        /// <summary>
        /// To satisfy the contract.
        /// </summary>
        IPageHelper IVehicleFilters.PageHelper { get => PageHelper; set => throw new System.NotImplementedException(); }
    }
}
