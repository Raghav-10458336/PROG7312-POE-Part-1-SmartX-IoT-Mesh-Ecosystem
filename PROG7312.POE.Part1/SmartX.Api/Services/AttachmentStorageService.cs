using System.Security.Cryptography;
using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class AttachmentStorageService
    {
        private readonly string _storagePath;
        private readonly List<SensorAttachment> _attachments = new();

        private readonly byte[] _encryptionKey;

        public AttachmentStorageService(IWebHostEnvironment environment)
        {
            _storagePath = Path.Combine(
                environment.ContentRootPath,
                "EncryptedUploads");

            Directory.CreateDirectory(_storagePath);

            using SHA256 sha256 = SHA256.Create();

            _encryptionKey = sha256.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes(
                    "SmartX-Development-Encryption-Key"));
        }

        public IReadOnlyList<SensorAttachment> GetBySensorId(Guid sensorId)
        {
            return _attachments
                .Where(attachment => attachment.SensorId == sensorId)
                .ToList()
                .AsReadOnly();
        }

        public async Task<SensorAttachment> SaveAsync(
            Guid sensorId,
            IFormFile file)
        {
            string storedFileName =
                $"{Guid.NewGuid():N}.encrypted";

            string fullPath =
                Path.Combine(_storagePath, storedFileName);

            byte[] iv = RandomNumberGenerator.GetBytes(16);

            using Aes aes = Aes.Create();

            aes.Key = _encryptionKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            await using FileStream outputStream =
                new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);

            await outputStream.WriteAsync(iv);

            await using CryptoStream encryptionStream =
                new CryptoStream(
                    outputStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write);

            await using Stream uploadStream =
                file.OpenReadStream();

            await uploadStream.CopyToAsync(encryptionStream);

            await encryptionStream.FlushFinalBlockAsync();

            SensorAttachment attachment = new()
            {
                SensorId = sensorId,
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length
            };

            _attachments.Add(attachment);

            return attachment;
        }
    }
}
