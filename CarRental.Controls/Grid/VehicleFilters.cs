using System.ComponentModel;
using CarRental.Model;

namespace CarRental.Controls.Grid
{
    /// <summary>
    /// State of grid filters.
    /// </summary>
    public class VehicleFilters : IVehicleFilters
    {
        /// <summary>
        /// Keep state of paging.
        /// </summary>
        public IPageHelper PageHelper { get; set; }

        /// <summary>
        /// Default: take scoped instance of page helper
        /// </summary>
        /// <param name="pageHelper">The <see cref="IPageHelper"/> instance.</param>
        public VehicleFilters(IPageHelper pageHelper)
        {
            PageHelper = pageHelper;
        }

        /// <summary>
        /// Avoid multiple concurrent requests.
        /// </summary>
        public bool Loading { get; set; }

        /// <summary>
        /// Column to sort by.
        /// </summary>
        public VehicleFilterColumns SortColumn { get; set; } = VehicleFilterColumns.LicenseNumber;
        
        /// <summary>
        /// True when sorting ascending, otherwise sort descending.
        /// </summary>
        public bool SortAscending { get; set; }

        /// <summary>
        /// Column filtered text is against.
        /// </summary>
        public VehicleFilterColumns FilterColumn { get; set; } = VehicleFilterColumns.LicenseNumber;

        /// <summary>
        /// Text to filter on.
        /// </summary>
        public string FilterText { get; set; }
    }
}
