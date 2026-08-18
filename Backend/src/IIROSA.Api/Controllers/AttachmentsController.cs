using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Framework.Core.SharedServices.Services;

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
                return StatusCode(500, new { message = "Error downloading attachment", error = ex.Message });
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
                return StatusCode(500, new { message = "Error retrieving attachment info", error = ex.Message });
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
                return StatusCode(500, new { message = "Error retrieving attachment image", error = ex.Message });
            }
        }
    }
}
