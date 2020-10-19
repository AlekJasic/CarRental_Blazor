using CarRental.Model;
using System.Collections.Generic;

namespace CarRental.Controls.Grid
{
    /// <summary>
    /// Provides class attributes for columns
    /// </summary>
    public class ColumnService
    {
        /// <summary>
        /// Map columns
        /// </summary>
        private readonly Dictionary<VehicleFilterColumns, string> _columnMappings =
            new Dictionary<VehicleFilterColumns, string>
            {
                { VehicleFilterColumns.LicenseNumber, "col-8 col-lg-2 col-sm-3"},
                { VehicleFilterColumns.Brand, "col-8 col-lg-2 col-sm-3" },
                { VehicleFilterColumns.Model, "d-none d-sm-block col-lg-2 col-sm-2" },
                { VehicleFilterColumns.Mileage, "col-8 col-lg-2 col-sm-3"},
                { VehicleFilterColumns.RegistrationDate, "col-8 col-lg-2 col-sm-3"}

            }; // 2 2 1 1 2 1

        /// <summary>
        /// Left edit column.
        /// </summary>
        public string EditColumn => "col-1 col-sm-1";

        /// <summary>
        /// Delete confirmation column.
        /// </summary>
        public string DeleteConfirmation => "col-lg-9 col-sm-8";

        /// <summary>
        /// Get attributes for column
        /// </summary>
        /// <param name="column">The <see cref="VehicleFilterColumns"/> to reference.</param>
        /// <returns>A <see cref="string"/> representing the classes.</returns>
        public string GetClassForColumn(VehicleFilterColumns column) => _columnMappings[column];
    }
}
