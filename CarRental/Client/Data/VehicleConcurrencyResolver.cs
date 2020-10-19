using CarRental.Model;

namespace CarRental.Client.Data
{
    /// <summary>
    /// This class helps track concurrency issues for client/server
    /// scenarios. 
    /// </summary>
    public class VehicleConcurrencyResolver
    {
        /// <summary>
        /// The latest database version.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// The <see cref="Vehicle"/> being updated.
        /// </summary>
        public Vehicle OriginalVehicle { get; set; }

        /// <summary>
        /// The <see cref="Vehicle"/> as it exists in the database.
        /// </summary>
        public Vehicle DatabaseVehicle { get; set; }
    }
}
