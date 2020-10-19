using CarRental.Model;

namespace CarRental.Client.Data
{
    /// <summary>
    /// I get by with a little help from my friends.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Transfers the new page information over.
        /// </summary>
        /// <param name="helper">The <see cref="PageHelper"/> to use.</param>
        /// <param name="newData">The new data to transfer.</param>
        public static void Refresh(this IPageHelper helper, IPageHelper newData)
        {
            helper.PageSize = newData.PageSize;
            helper.PageItems = newData.PageItems;
            helper.Page = newData.Page;
            helper.TotalItemCount = newData.TotalItemCount;
        }

        /// <summary>
        /// Helper to transfer concurrency information from the repo
        /// to the data object.
        /// </summary>
        /// <param name="vehicle">The <see cref="Vehicle"/> being resolved.</param>
        /// <param name="repo">The <see cref="WasmRepository"/> holding the concurrency values.</param>
        /// <returns>The <see cref="VehicleConcurrencyResolver"/> instance.</returns>
        public static VehicleConcurrencyResolver ToConcurrencyResolver(
            this Vehicle vehicle, WasmRepository repo)
        {
            return new VehicleConcurrencyResolver()
            {
                OriginalVehicle = vehicle,
                RowVersion = repo.RowVersion
            };
        }
    }
}
