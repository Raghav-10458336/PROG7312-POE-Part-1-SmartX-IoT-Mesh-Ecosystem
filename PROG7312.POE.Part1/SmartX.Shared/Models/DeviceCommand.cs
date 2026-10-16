namespace SmartX.Shared.Models
{
    public class DeviceCommand
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string CommandType { get; set; } = string.Empty;

        public bool PreviousState { get; set; }

        public bool NewState { get; set; }

        public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsUndone { get; set; }
    }
}