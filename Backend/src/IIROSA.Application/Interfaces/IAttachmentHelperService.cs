using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Attachment Helper Service Interface
/// Provides reusable attachment handling functionality for all modules
/// </summary>
public interface IAttachmentHelperService
{
    /// <summary>
    /// Saves an attachment from base64 data
    /// </summary>
    /// <param name="attachment">The attachment DTO containing base64 file data</param>
    /// <param name="existingAttachmentId">Optional existing attachment ID for updates</param>
    /// <param name="attachmentTypeId">Attachment type ID (default: 2 for images/icons)</param>
    /// <returns>The ID of the saved attachment, or null if save failed</returns>
    Task<Guid?> SaveAttachmentAsync(AttachmentDto attachment, Guid? existingAttachmentId = null, int? attachmentTypeId = null);

    /// <summary>
    /// Saves multiple attachments from base64 data
    /// </summary>
    /// <param name="attachments">Collection of attachment DTOs containing base64 file data</param>
    /// <param name="attachmentTypeId">Attachment type ID (default: 2 for images/icons)</param>
    /// <returns>List of saved attachment IDs</returns>
    Task<List<Guid>> SaveAttachmentsAsync(IEnumerable<AttachmentDto> attachments, int? attachmentTypeId = null);

    /// <summary>
    /// Gets an attachment by ID
    /// </summary>
    /// <param name="attachmentId">The attachment ID</param>
    /// <param name="loadBytes">Whether to load file content bytes</param>
    /// <returns>Attachment DTO or null if not found</returns>
    Task<AttachmentDto?> GetAttachmentAsync(Guid attachmentId, bool loadBytes = false);

    /// <summary>
    /// Gets multiple attachments by IDs
    /// </summary>
    /// <param name="attachmentIds">Collection of attachment IDs</param>
    /// <param name="loadBytes">Whether to load file content bytes</param>
    /// <returns>List of Attachment DTOs</returns>
    Task<List<AttachmentDto>> GetAttachmentsAsync(List<Guid> attachmentIds, bool loadBytes = false);

    /// <summary>
    /// Gets attachment thumbnail as byte array
    /// </summary>
    /// <param name="attachmentId">The attachment ID</param>
    /// <returns>Thumbnail bytes or null if not found</returns>
    Task<byte[]?> GetThumbnailAsync(Guid attachmentId);

    /// <summary>
    /// Deletes an attachment by ID
    /// </summary>
    /// <param name="attachmentId">The attachment ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAttachmentAsync(Guid attachmentId);

    /// <summary>
    /// Deletes multiple attachments by IDs
    /// </summary>
    /// <param name="attachmentIds">Collection of attachment IDs to delete</param>
    void DeleteAttachments(List<Guid> attachmentIds);
}
