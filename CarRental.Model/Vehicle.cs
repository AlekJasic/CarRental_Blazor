using System.ComponentModel.DataAnnotations;

namespace CarRental.Model
{
    /// <summary>
    /// Vehicle entity.
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// LicenseNumber.
        /// </summary>
        [Required]
        [StringLength(10, ErrorMessage = "License number cannot exceed 10 characters.")]
        public string LicenseNumber { get; set; }

        /// <summary>
        /// Brand.
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "Brand cannot exceed 20 characters.")]
        public string Brand { get; set; }

        /// <summary>
        /// Model.
        /// </summary>
        [StringLength(30, ErrorMessage = "Last name cannot exceed 30 characters.")]
        public string Model { get; set; }

        /// <summary>
        /// Registration date.
        /// </summary>
        public string RegistrationDate { get; set; }

        /// <summary>
        /// Mileage.
        /// </summary>
        [Required]
        public string Mileage { get; set; }

        /// <summary>
        /// Tank.
        /// </summary>
        public string Tank { get; set; }

        /// <summary>
        /// Last updated.
        /// </summary>
        
        public string LastUpdated { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        public int? ClientId { get; set; }

    }
}
