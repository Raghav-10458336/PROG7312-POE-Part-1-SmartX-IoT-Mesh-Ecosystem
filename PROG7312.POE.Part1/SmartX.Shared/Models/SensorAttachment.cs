namespace SmartX.Shared.Models
{
    public class SensorAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    }
}