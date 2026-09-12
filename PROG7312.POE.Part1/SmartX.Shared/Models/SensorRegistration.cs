using SmartX.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartX.Shared.Models
{
    public class SensorRegistration
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string DeviceIdentifier { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DeploymentLocation { get; set; } = string.Empty;

        [Required]
        public SensorCategory Category { get; set; }

        public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
