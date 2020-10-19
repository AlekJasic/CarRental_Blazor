namespace CarRental.Model
{
    /// <summary>
    /// Interface for filtering.
    /// </summary>
    public interface IVehicleFilters
    {
        /// <summary>
        /// The <see cref="VehicleFilterColumns"/> being filtered on.
        /// </summary>
        VehicleFilterColumns FilterColumn { get; set; }

        /// <summary>
        /// The text of the filter.
        /// </summary>
        string FilterText { get; set; }

        /// <summary>
        /// Paging state in <see cref="PageHelper"/>.
        /// </summary>
        IPageHelper PageHelper { get; set; }

        /// <summary>
        /// Loading new set of data.
        /// </summary>
        bool Loading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the sort is ascending or descending.
        /// </summary>
        bool SortAscending { get; set; }

        /// <summary>
        /// The <see cref="VehicleFilterColumns"/> being sorted.
        /// </summary>
        VehicleFilterColumns SortColumn { get; set; }
    }
}
