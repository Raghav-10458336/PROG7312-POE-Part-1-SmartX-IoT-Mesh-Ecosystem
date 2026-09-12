using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/sensors/{sensorId:guid}/attachments")]
    public class AttachmentsController : ControllerBase
    {
        private const long MaxFileSize = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".txt",
            ".log",
            ".json",
            ".docx",
            ".xlsx",
            ".xml",
            ".csv",
            ".jpg",
            ".jpeg",
            ".png",
            ".pdf"
        };

        private readonly AttachmentStorageService _attachmentStorage;
        private readonly SensorRegistryService _sensorRegistry;

        public AttachmentsController(
            AttachmentStorageService attachmentStorage,
            SensorRegistryService sensorRegistry)
        {
            _attachmentStorage = attachmentStorage;
            _sensorRegistry = sensorRegistry;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            Guid sensorId,
            IFormFile file)
        {
            SensorRegistration? sensor =
                _sensorRegistry.GetById(sensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor not found."
                });
            }

            if (file is null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "A non-empty file is required."
                });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new
                {
                    message = "File size cannot exceed 10 MB."
                });
            }

            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message = "Unsupported file type."
                });
            }

            SensorAttachment attachment =
                await _attachmentStorage.SaveAsync(
                    sensorId,
                    file);

            return Created(
                $"/api/sensors/{sensorId}/attachments/{attachment.Id}",
                attachment);
        }

        [HttpGet]
        public IActionResult GetAttachments(Guid sensorId)
        {
            SensorRegistration? sensor =
                _sensorRegistry.GetById(sensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor not found."
                });
            }

            return Ok(
                _attachmentStorage.GetBySensorId(sensorId));
        }
    }
}