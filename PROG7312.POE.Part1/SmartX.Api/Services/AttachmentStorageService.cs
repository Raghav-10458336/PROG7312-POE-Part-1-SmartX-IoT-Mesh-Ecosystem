using System.Security.Cryptography;
using System.Text;
using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class AttachmentStorageService
    {
        private const int IvSizeBytes = 16;
        private const int HmacSizeBytes = 32;

        private readonly string _storagePath;

        private readonly List<SensorAttachment> _attachments =
            new();

        private readonly object _attachmentLock =
            new();

        private readonly byte[] _encryptionKey;
        private readonly byte[] _hmacKey;

        public AttachmentStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _storagePath =
                Path.Combine(
                    environment.ContentRootPath,
                    "EncryptedUploads");

            Directory.CreateDirectory(
                _storagePath);

            string? masterSecret =
                configuration[
                    "AttachmentEncryption:MasterKey"];

            if (string.IsNullOrWhiteSpace(
                masterSecret))
            {
                throw new InvalidOperationException(
                    "Attachment encryption key is not configured. " +
                    "Set AttachmentEncryption:MasterKey using " +
                    ".NET User Secrets.");
            }

            byte[] derivedKeyMaterial =
                SHA512.HashData(
                    Encoding.UTF8.GetBytes(
                        masterSecret));

            _encryptionKey =
                derivedKeyMaterial[..32];

            _hmacKey =
                derivedKeyMaterial[32..64];
        }

        public IReadOnlyList<SensorAttachment>
            GetBySensorId(
                Guid sensorId)
        {
            lock (_attachmentLock)
            {
                return _attachments
                    .Where(
                        attachment =>
                            attachment.SensorId ==
                            sensorId)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public async Task<SensorAttachment>
            SaveAsync(
                Guid sensorId,
                IFormFile file)
        {
            string storedFileName =
                $"{Guid.NewGuid():N}.encrypted";

            string fullPath =
                Path.Combine(
                    _storagePath,
                    storedFileName);

            byte[] iv =
                RandomNumberGenerator.GetBytes(
                    IvSizeBytes);

            try
            {
                await EncryptFileAsync(
                    file,
                    fullPath,
                    iv);

                byte[] integrityTag =
                    await CalculateIntegrityTagAsync(
                        fullPath);

                await AppendIntegrityTagAsync(
                    fullPath,
                    integrityTag);

                SensorAttachment attachment =
                    new()
                    {
                        SensorId =
                            sensorId,

                        OriginalFileName =
                            Path.GetFileName(
                                file.FileName),

                        StoredFileName =
                            storedFileName,

                        ContentType =
                            file.ContentType,

                        FileSizeBytes =
                            file.Length,

                        UploadedAtUtc =
                            DateTime.UtcNow
                    };

                lock (_attachmentLock)
                {
                    _attachments.Add(
                        attachment);
                }

                return attachment;
            }
            catch
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw;
            }
        }

        private async Task EncryptFileAsync(
            IFormFile file,
            string fullPath,
            byte[] iv)
        {
            using Aes aes =
                Aes.Create();

            aes.Key =
                _encryptionKey;

            aes.IV =
                iv;

            aes.Mode =
                CipherMode.CBC;

            aes.Padding =
                PaddingMode.PKCS7;

            await using FileStream outputStream =
                new(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

            await outputStream.WriteAsync(
                iv);

            await using CryptoStream encryptionStream =
                new(
                    outputStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write,
                    leaveOpen: true);

            await using Stream uploadStream =
                file.OpenReadStream();

            await uploadStream.CopyToAsync(
                encryptionStream);

            await encryptionStream
                .FlushFinalBlockAsync();
        }

        private async Task<byte[]>
            CalculateIntegrityTagAsync(
                string fullPath)
        {
            using HMACSHA256 hmac =
                new(_hmacKey);

            await using FileStream encryptedStream =
                new(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 81920,
                    useAsync: true);

            return await hmac.ComputeHashAsync(
                encryptedStream);
        }

        private static async Task
            AppendIntegrityTagAsync(
                string fullPath,
                byte[] integrityTag)
        {
            if (integrityTag.Length !=
                HmacSizeBytes)
            {
                throw new CryptographicException(
                    "Unexpected HMAC length.");
            }

            await using FileStream outputStream =
                new(
                    fullPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

            await outputStream.WriteAsync(
                integrityTag);
        }
    }
}
