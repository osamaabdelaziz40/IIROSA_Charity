using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Framework.Core.SharedServices.Services;
using System.Drawing;

namespace IIROSA.Api.Controllers
{
    /// <summary>
    /// Attachments Controller - Handles file upload, download, and retrieval
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AttachmentsController : ControllerBase
    {
        private const int BatchIdsCap = 50;

        private readonly AttachmentService _attachmentService;
        private readonly AppSettingsService _appSettingsService;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            AttachmentService attachmentService,
            AppSettingsService appSettingsService,
            ILogger<AttachmentsController> _logger)
        {
            _attachmentService = attachmentService;
            _appSettingsService = appSettingsService;
            this._logger = _logger;
        }

        /// <summary>
        /// UC-SYS-01: Upload a file. Storage destination (database vs file system) follows the
        /// SaveFilesToDatabase system setting; size / allowed-type / image-dimension limits come
        /// from the Attachments* system settings and are enforced before the write.
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest(new { message = "No file was uploaded", errors = new { file = "No file was uploaded" } });
            }

            // Size limit (AttachmentsMaxSize is configured in megabytes; 0 = unlimited)
            var maxSizeMb = _appSettingsService.AttachmentsMaxSize;
            if (maxSizeMb > 0 && file.Length > maxSizeMb * 1024L * 1024L)
            {
                return BadRequest(new
                {
                    message = $"File exceeds the maximum allowed size of {maxSizeMb} MB",
                    errors = new { file = $"File exceeds the maximum allowed size of {maxSizeMb} MB" }
                });
            }

            // Allowed types (comma-separated extensions, e.g. ".pdf,.png"; empty = unrestricted)
            var allowedTypes = _appSettingsService.AttachmentsAllowedTypes;
            if (!string.IsNullOrWhiteSpace(allowedTypes))
            {
                var extension = System.IO.Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
                var allowed = allowedTypes
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.ToLowerInvariant())
                    .ToList();
                if (allowed.Count > 0 && !allowed.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message = $"File type '{extension}' is not allowed",
                        errors = new { file = $"File type '{extension}' is not allowed" }
                    });
                }
            }

            // Image dimension limits (apply only to images; 0 = unlimited)
            var maxWidth = _appSettingsService.AttachmentsAllowedWidth;
            var maxHeight = _appSettingsService.AttachmentsAllowedHeight;
            if ((maxWidth > 0 || maxHeight > 0)
                && !string.IsNullOrEmpty(file.ContentType)
                && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var image = Image.FromStream(file.OpenReadStream());
                    if (maxWidth > 0 && image.Width > maxWidth)
                    {
                        return BadRequest(new
                        {
                            message = $"Image width {image.Width}px exceeds the maximum of {maxWidth}px",
                            errors = new { file = $"Image width {image.Width}px exceeds the maximum of {maxWidth}px" }
                        });
                    }
                    if (maxHeight > 0 && image.Height > maxHeight)
                    {
                        return BadRequest(new
                        {
                            message = $"Image height {image.Height}px exceeds the maximum of {maxHeight}px",
                            errors = new { file = $"Image height {image.Height}px exceeds the maximum of {maxHeight}px" }
                        });
                    }
                }
                catch (ArgumentException)
                {
                    return BadRequest(new
                    {
                        message = "The uploaded file is not a valid image",
                        errors = new { file = "The uploaded file is not a valid image" }
                    });
                }
                catch (OutOfMemoryException)
                {
                    // GDI+ reports corrupt image bytes as OOM — same refusal as the decode
                    // failure above, never a 500 (review finding P5, 2026-08-26).
                    return BadRequest(new
                    {
                        message = "The uploaded file is not a valid image",
                        errors = new { file = "The uploaded file is not a valid image" }
                    });
                }
            }

            try
            {
                var result = _attachmentService.AddAttachment(file, file.FileName, file.ContentType);
                if (!result.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "The file was rejected",
                        errors = result.Errors.GroupBy(e => e.Name).ToDictionary(g => g.Key, g => string.Join("; ", g.Select(e => e.Value)))
                    });
                }

                var attachment = result.Value;
                return Ok(new
                {
                    id = attachment.Id.ToString(),
                    fileName = attachment.FileName,
                    contentType = attachment.ContentType,
                    fileSizeBytes = file.Length,
                    createdOn = attachment.CreatedOn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment '{FileName}'", file.FileName);
                return StatusCode(500, new { message = "Error uploading attachment" });
            }
        }

        /// <summary>
        /// UC-SYS-03: Batch metadata read for a set of attachment ids (report file manifests,
        /// detail screens). Unknown ids are omitted; capped at 50 ids per call.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAttachmentsBatch([FromQuery] List<Guid> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return Ok(Array.Empty<object>());
                }

                if (ids.Count > BatchIdsCap)
                {
                    return BadRequest(new { message = $"A maximum of {BatchIdsCap} attachment ids per request is allowed" });
                }

                var attachments = await _attachmentService.GetAttachmentsMetadataAsync(ids);
                var sizes = await _attachmentService.GetAttachmentSizesAsync(ids);

                return Ok(attachments.Select(a => new
                {
                    id = a.Id.ToString(),
                    fileName = a.FileName,
                    contentType = a.ContentType,
                    fileSizeBytes = sizes.TryGetValue(a.Id, out var size) ? size : 0,
                    createdOn = a.CreatedOn
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading attachment batch metadata");
                return StatusCode(500, new { message = "Error reading attachment metadata" });
            }
        }

        /// <summary>
        /// Download attachment by ID
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAttachment(Guid id)
        {
            try
            {
                var attachment = await _attachmentService.GetAttachmentForDownload(id);
                if (attachment == null)
                {
                    return NotFound(new { message = $"Attachment with ID '{id}' not found" });
                }

                // If file is stored in database
                if (attachment.AttachmentContent?.FileContent != null)
                {
                    return File(
                        attachment.AttachmentContent.FileContent,
                        attachment.ContentType ?? "application/octet-stream",
                        attachment.FileName);
                }

                // If file is stored on file system
                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    var attachmentsPath = _appSettingsService.AttachmentsPath ?? "";
                    var fullPath = Path.Combine(attachmentsPath, attachment.FilePath.TrimStart('\\'));

                    if (!System.IO.File.Exists(fullPath))
                    {
                        _logger.LogWarning("File not found on disk: {FullPath}", fullPath);
                        return NotFound(new { message = "File not found on server" });
                    }

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                    return File(fileBytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
                }

                return NotFound(new { message = "No file content available" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment: {Id}", id);
                return StatusCode(500, new { message = "Error downloading attachment" });
            }
        }

        /// <summary>
        /// Get attachment info by ID
        /// </summary>
        [HttpGet("{id}/info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttachmentInfo(Guid id)
        {
            try
            {
                var attachment = await _attachmentService.GetAttachment(id, loadBytes: false);
                if (attachment == null)
                {
                    return NotFound(new { message = $"Attachment with ID '{id}' not found" });
                }

                return Ok(new
                {
                    id = attachment.Id.ToString(),
                    fileName = attachment.FileName,
                    contentType = attachment.ContentType,
                    fileSizeBytes = attachment.AttachmentContent?.FileContent?.Length ?? 0,
                    createdOn = attachment.CreatedOn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment info: {Id}", id);
                return StatusCode(500, new { message = "Error retrieving attachment info" });
            }
        }

        /// <summary>
        /// Get attachment image by ID (returns image directly for use in img tags)
        /// </summary>
        [HttpGet("{id}/image")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttachmentImage(Guid id)
        {
            try
            {
                var attachment = await _attachmentService.GetAttachment(id, loadBytes: true);
                if (attachment == null)
                {
                    return NotFound(new { message = $"Attachment with ID '{id}' not found" });
                }

                // Check if it's an image content type
                if (string.IsNullOrEmpty(attachment.ContentType) || !attachment.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { message = "Attachment is not an image" });
                }

                // If file is stored in database
                if (attachment.AttachmentContent?.FileContent != null)
                {
                    return File(attachment.AttachmentContent.FileContent, attachment.ContentType);
                }

                // If file is stored on file system
                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    var attachmentsPath = _appSettingsService.AttachmentsPath ?? "";
                    var fullPath = Path.Combine(attachmentsPath, attachment.FilePath.TrimStart('\\'));

                    if (!System.IO.File.Exists(fullPath))
                    {
                        _logger.LogWarning("Image file not found on disk: {FullPath}", fullPath);
                        return NotFound(new { message = "Image file not found on server" });
                    }

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                    return File(fileBytes, attachment.ContentType);
                }

                return NotFound(new { message = "No image content available" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment image: {Id}", id);
                return StatusCode(500, new { message = "Error retrieving attachment image" });
            }
        }
    }
}
