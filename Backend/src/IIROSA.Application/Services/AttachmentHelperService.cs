using Microsoft.Extensions.Logging;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Services;
using Framework.Core.SharedServices.Entities;
using IIROSA.Application.Interfaces;

namespace IIROSA.Application.Services;

/// <summary>
/// Attachment Helper Service Implementation
/// Provides reusable attachment handling functionality for all modules
/// </summary>
public class AttachmentHelperService : IAttachmentHelperService
{
    private readonly AttachmentService _attachmentService;
    private readonly ILogger<AttachmentHelperService> _logger;

    public AttachmentHelperService(
        AttachmentService attachmentService,
        ILogger<AttachmentHelperService> logger)
    {
        _attachmentService = attachmentService;
        _logger = logger;
    }

    /// <summary>
    /// Saves an attachment from base64 data
    /// </summary>
    public async Task<Guid?> SaveAttachmentAsync(AttachmentDto attachment, Guid? existingAttachmentId = null, int? attachmentTypeId = null)
    {
        if (attachment == null || string.IsNullOrEmpty(attachment.FileName) || attachment.FileData == null)
        {
            _logger.LogWarning("SaveAttachmentAsync called with invalid attachment data");
            return null;
        }

        // Convert FileData (base64 string) to byte array
        byte[] fileBytes;
        try
        {
            // Remove base64 header if present (e.g., "data:image/png;base64,")
            var base64String = attachment.FileData.ToString() ?? string.Empty;
            if (base64String.Contains(','))
            {
                base64String = base64String.Substring(base64String.IndexOf(',') + 1);
            }

            fileBytes = attachment.FileData;// Convert.FromBase64String(base64String);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert file data to byte array for file: {FileName}", attachment.FileName);
            return null;
        }

        var savedAttachment = _attachmentService.AddOrUpdateAttachment(
            attachment.FileName,
            attachment.ContentType ?? "application/octet-stream",
            fileBytes,
            attachmentTypeId ?? 1, // Default AttachmentTypeId for images/icons
            existingAttachmentId,
            attachment.FileName,
            attachment.FileName
        );

        return savedAttachment?.Id;
    }

    /// <summary>
    /// Saves multiple attachments from base64 data
    /// </summary>
    public async Task<List<Guid>> SaveAttachmentsAsync(IEnumerable<AttachmentDto> attachments, int? attachmentTypeId = null)
    {
        var savedIds = new List<Guid>();

        if (attachments == null)
        {
            _logger.LogWarning("SaveAttachmentsAsync called with null attachments collection");
            return savedIds;
        }

        foreach (var attachment in attachments)
        {
            var id = await SaveAttachmentAsync(attachment, null, attachmentTypeId);
            if (id.HasValue)
            {
                savedIds.Add(id.Value);
            }
        }

        return savedIds;
    }

    /// <summary>
    /// Gets an attachment by ID
    /// </summary>
    public async Task<AttachmentDto?> GetAttachmentAsync(Guid attachmentId, bool loadBytes = false)
    {
        if (attachmentId == Guid.Empty)
        {
            _logger.LogWarning("GetAttachmentAsync called with empty GUID");
            return null;
        }

        var attachment = await _attachmentService.GetAttachment(attachmentId, loadBytes);
        if (attachment == null)
        {
            _logger.LogWarning("Attachment with ID {AttachmentId} not found", attachmentId);
            return null;
        }

        return MapToDto(attachment);
    }

    /// <summary>
    /// Gets multiple attachments by IDs
    /// </summary>
    public async Task<List<AttachmentDto>> GetAttachmentsAsync(List<Guid> attachmentIds, bool loadBytes = false)
    {
        var result = new List<AttachmentDto>();

        if (attachmentIds == null || !attachmentIds.Any())
        {
            _logger.LogWarning("GetAttachmentsAsync called with empty or null IDs list");
            return result;
        }

        var attachments = loadBytes
            ? await _attachmentService.GetAttachmentsWithBytes(attachmentIds)
            : await _attachmentService.GetAttachmentAsync(attachmentIds);

        if (attachments != null)
        {
            result.AddRange(attachments.Select(MapToDto).Where(dto => dto != null).Cast<AttachmentDto>());
        }

        return result;
    }

    /// <summary>
    /// Gets attachment thumbnail as byte array
    /// </summary>
    public async Task<byte[]?> GetThumbnailAsync(Guid attachmentId)
    {
        if (attachmentId == Guid.Empty)
        {
            _logger.LogWarning("GetThumbnailAsync called with empty GUID");
            return null;
        }

        var thumbnail = await _attachmentService.GetAttachmentIMGThumbnailAsync(attachmentId);
        return thumbnail;
    }

    /// <summary>
    /// Deletes an attachment by ID
    /// </summary>
    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId)
    {
        if (attachmentId == Guid.Empty)
        {
            _logger.LogWarning("DeleteAttachmentAsync called with empty GUID");
            return false;
        }

        var result = await _attachmentService.RemoveAsync(attachmentId);
        if (result)
        {
            _logger.LogInformation("Attachment with ID {AttachmentId} deleted successfully", attachmentId);
        }
        else
        {
            _logger.LogWarning("Failed to delete attachment with ID {AttachmentId}", attachmentId);
        }

        return result;
    }

    /// <summary>
    /// Deletes multiple attachments by IDs
    /// </summary>
    public void DeleteAttachments(List<Guid> attachmentIds)
    {
        if (attachmentIds == null || !attachmentIds.Any())
        {
            _logger.LogWarning("DeleteAttachments called with empty or null IDs list");
            return;
        }

        _attachmentService.RemoveRange(attachmentIds);
        _logger.LogInformation("Deleted {Count} attachments", attachmentIds.Count);
    }

    /// <summary>
    /// Maps Attachment entity to AttachmentDto
    /// </summary>
    private static AttachmentDto? MapToDto(Attachment attachment)
    {
        if (attachment == null) return null;

        return new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileData = attachment.FileContent,
            FilePath = attachment.FilePath,
            Description = attachment.DescriptionEn ?? attachment.DescriptionAr,
            Extension = attachment.Extension?.TrimStart('.') ?? string.Empty,
            IsDeleted = false,
            IsNew = false
        };
    }
}
