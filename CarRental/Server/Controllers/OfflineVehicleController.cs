using Vehicles.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRental.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OfflineVehicleController : ControllerBase
    {
        ApplicationAuditDbContext db;

        public OfflineVehicleController(ApplicationAuditDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<Vehicle> ChangedVehicles([FromQuery] DateTime since)
        {
            return db.Vehicles.Where(v => v.LastUpdated >= since).Include(v => v.Notes);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWithNotes(Vehicle vehicle)
        {
            var licenseNumber = vehicle.LicenseNumber;
            var existingNotes = (await db.Vehicles.AsNoTracking().Include(v => v.Notes).SingleAsync(v => v.LicenseNumber == licenseNumber)).Notes;
            var retainedNotes = vehicle.Notes.ToLookup(n => n.InspectionNoteId);
            var notesToDelete = existingNotes.Where(n => !retainedNotes.Contains(n.InspectionNoteId));
            db.RemoveRange(notesToDelete);

            vehicle.LastUpdated = DateTime.Now;
   
            vehicle.ClientId = null;
             
            db.Vehicles.Update(vehicle);

            await db.SaveChangesAsync();
            return Ok();

        }
    }
}
