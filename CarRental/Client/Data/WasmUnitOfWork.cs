using System.Security.Claims;
using System.Threading.Tasks;
using CarRental.BaseRepository;
using CarRental.Model;

namespace CarRental.Client.Data
{
    /// <summary>
    /// Vehicle <see cref="IUnitOfWork"/> implementation that simply tracks versions.
    /// </summary>
    public class WasmUnitOfWork : IUnitOfWork<Vehicle>
    {
        /// <summary>
        /// The <see cref="Vehicle"/> being edited.
        /// </summary>
        public Vehicle OriginalVehicle { get => _repo.OriginalVehicle; }

        /// <summary>
        /// The <see cref="Vehicle"/> that is in the database.
        /// </summary>
        public Vehicle DatabaseVehicle { get => _repo.DatabaseVehicle; }

        /// <summary>
        /// True if there is a conflict (only exists if that happens).
        /// </summary>
        public bool HasConcurrencyConflict => _repo.DatabaseVehicle != null;

        /// <summary>
        /// The version of the last read <see cref="Vehicle"/>.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Repository instance.
        /// </summary>
        private readonly WasmRepository _repo;

        /// <summary>
        /// Expose the <see cref="IBasicRepository{Vehicle}"/> interface.
        /// </summary>
        public IBasicRepository<Vehicle> Repo { get => _repo; }

        /// <summary>
        /// Creates a new instance of the <see cref="WasmUnitOfWork"/> class.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Vehicle}"/> implementation.</param>
        public WasmUnitOfWork(IBasicRepository<Vehicle> repo)
        {
            _repo = repo as WasmRepository;
        }

        /// <summary>
        /// Time to commit.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task CommitAsync()
        {
            return Repo.UpdateAsync(OriginalVehicle, null);
        }

        /// <summary>
        /// Get rid of references.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="user"></param>
        public void SetUser(ClaimsPrincipal user)
        {
            throw new System.NotImplementedException();
        }
    }
}
