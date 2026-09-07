using FluentValidation.Resources;
using Framework.Core.Data.Repositories;
using Framework.Core.Extensions;
using Framework.Core.Globalization;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Attachment = Framework.Core.SharedServices.Entities.Attachment;

namespace Framework.Core.SharedServices.Services
{
    public class AttachmentService
    {
        private readonly IRepositoryBase<ICommonsDbContext, Attachment> _attachmentRepository;
        private readonly IRepositoryBase<ICommonsDbContext, AttachmentType> _attachmentTypeRepository;
        private readonly AppSettingsService _appSettingsService;

        public AttachmentService(IRepositoryBase<ICommonsDbContext, Attachment> attachmentRepository,
            IRepositoryBase<ICommonsDbContext, AttachmentType> attachmentTypeRepository,
        AppSettingsService appSettingsService)
        {
            _attachmentRepository = attachmentRepository;
            _attachmentTypeRepository = attachmentTypeRepository;
            _appSettingsService = appSettingsService;
        }
        public ReturnResult<Attachment> AddAttachment(IFormFile file, string title = null,
            string contentType = null, int? attachType = null)
        {
            var result = new ReturnResult<Attachment>();
            var attResult = this.AddOrUpdateAttachment(file, attachType, null, title, contentType);
            if (!attResult.IsValid)
            {
                result.Merge(attResult);
                return result;
            }
            result.Value = attResult.Value;
            return result;
        }
        public ReturnResult<Attachment> AddOrUpdateAttachment(
            IFormFile file,
            int? attType,
            Guid? attachmentId = null,
            string title = null, string contentType = null)
        {
            var result = new ReturnResult<Attachment>();
            if (file == null)
            {
                result.AddErrorItem(string.Empty, "FileZeroLengthErrorMessage");
                //result.AddErrorItem(string.Empty, SharedResources.FileZeroLengthErrorMessage);
                return result;
            }

            if (file.Length <= 0)
            {
                result.AddErrorItem(string.Empty, "FileZeroLengthErrorMessage");
                //result.AddErrorItem(string.Empty, SharedResources.FileZeroLengthErrorMessage);
                return result;
            }

            if (!_appSettingsService.SaveFilesToDatabase && string.IsNullOrEmpty(_appSettingsService.AttachmentsPath))
            {
                throw new Exception(
                    "File can not be saved. Current Settings is. SaveFileToDatabase=true and Attachment Path is Missing");
            }

            var fileBytes = new byte[file.Length];

            ////file.InputStream.Read(fileBytes, 0, file.Length);
            var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);

            result.Value = this.AddOrUpdateAttachment(
                file.FileName,
                contentType ?? file.ContentType,
                ms.ToArray(),
                attType,
                attachmentId,
                title,
                title);
            return result;
        }

        public Attachment AddOrUpdateAttachment(
            string fileName,
            string contentType,
            byte[] fileBytes,
            int? attType,
            Guid? attachmentId = null,
            string titleAr = null,
            string titleEn = null,
            string descriptionAr = null,
            string descriptionEn = null,
            int? itemOrder = null)
        {
            var isUpdateFile = attachmentId.HasValue && attachmentId.Value != Guid.Empty;

            var attachment = isUpdateFile
                                 ? _attachmentRepository.GetById(attachmentId.Value)
                                 : new Attachment { Id = Guid.NewGuid().AsSequentialGuid() };

            if (attachment == null)
            {
                throw new Exception("The Attachment File You are trying to update Does Not Exist in the database");
            }

            if (attachment.AttachmentContent == null)
            {
                attachment.AttachmentContent = new AttachmentContent();
            }
            attachment.AttachmentContent.FileContent = fileBytes;
            attachment.TitleAr = titleAr;
            attachment.TitleEn = titleEn;
            attachment.DescriptionAr = descriptionAr ?? attachment.DescriptionAr;
            attachment.DescriptionEn = descriptionEn ?? attachment.DescriptionEn;
            attachment.ContentType = contentType;
            attachment.Extension = new FileInfo(fileName).Extension;
            attachment.FileName = fileName;
            attachment.AttachmentTypeId = attType;
            // in updating delete old file
            if (isUpdateFile)
            {
                this.DeleteAttachmentFromFileSystem(attachment.FilePath);
            }

            attachment.FilePath = _appSettingsService.SaveFilesToDatabase
                                      ? null
                                      : this.SaveAttachmentToFileSystem(attachment, fileBytes);
            attachment.AttachmentContent.Id = attachment.Id;
            attachment.AttachmentContent.AttachmentId = attachment.Id;
            //attachment.AttachmentContent.FileContent =
            //    _appSettingsService.SaveFilesToDatabase ? fileBytes : null;


            if (!isUpdateFile)
            {

                _attachmentRepository.Insert(attachment, true);
            }

            return attachment;
        }

       

        public async Task<Attachment> GetAttachment(Guid attachmentId, bool loadBytes = false)
        {
            if (loadBytes)
                return await GetAttachmentWithBytes(attachmentId);
            var attachment =await _attachmentRepository
                .TableNoTracking.Include(x => x.AttachmentType)
                
                .FirstOrDefaultAsync(at => at.Id == attachmentId);
            return attachment;
        }
        public async Task<List<Attachment>> GetAttachmentAsync(List<Guid> attachmentIds)
        {
            var attachment = await _attachmentRepository.GetAsync(filter: at => attachmentIds.Contains(at.Id) , includeProperties :a=>a.AttachmentContent);
            return attachment;
        }
        public async Task<Attachment> GetAttachmentWithBytes(Guid attachmentId)
        {
            var attachment =await _attachmentRepository
                .TableNoTracking.Include(a => a.AttachmentContent).Include(x => x.AttachmentType).FirstOrDefaultAsync(at => at.Id == attachmentId);
            if (attachment != null)
            {
                // to match with frontend dto 
                attachment.FileContent = attachment.AttachmentContent?.FileContent;
                if (attachment.AttachmentContent != null)
                {
                    attachment.AttachmentContent.Attachment = null;
                }
            }
            return attachment;
        }

       
        public async Task<List<Attachment>> GetAttachmentsWithBytes(List<Guid> attachmentIds)
        {
            var attachments = await _attachmentRepository
              .TableNoTracking.Include(a => a.AttachmentContent).Include(x => x.AttachmentType).Where(at => attachmentIds.Contains(at.Id)).ToListAsync();
           foreach (var attachment in attachments)
            {
                // to match with frontend dto 
                attachment.FileContent = attachment.AttachmentContent?.FileContent;
                if (attachment.AttachmentContent != null)
                {
                    attachment.AttachmentContent.Attachment = null;
                }
            }
            return attachments;
        }


        /// <summary>
        /// Metadata-only batch read (UC-SYS-03): rows for the given ids without loading file bytes.
        /// </summary>
        public async Task<List<Attachment>> GetAttachmentsMetadataAsync(List<Guid> attachmentIds)
        {
            return await _attachmentRepository
                .TableNoTracking.Include(x => x.AttachmentType)
                .Where(at => attachmentIds.Contains(at.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Content lengths (database-stored files) for the given ids, translated to DATALENGTH — no bytes over the wire.
        /// </summary>
        public async Task<Dictionary<Guid, long>> GetAttachmentSizesAsync(List<Guid> attachmentIds)
        {
            return await _attachmentRepository
                .TableNoTracking
                .Where(at => attachmentIds.Contains(at.Id) && at.AttachmentContent != null)
                .Select(at => new { at.Id, Length = (long?)at.AttachmentContent.FileContent.Length })
                .ToDictionaryAsync(x => x.Id, x => x.Length ?? 0);
        }

        public async Task<Attachment> GetAttachmentForDownload(Guid? attachmentId)
        {
            var attachment = _attachmentRepository.TableNoTracking.Include(c=>c.AttachmentContent).Where(at => at.Id == attachmentId).SingleOrDefault();
            attachment.AttachmentContent.Attachment = null;
            return attachment;
        }

        public async Task<byte[]> GetAttachmentIMGThumbnailAsync(Guid? attachmentId)
        {
            return await _attachmentRepository.TableNoTracking.Where(at => at.Id == attachmentId).Select(at => at.Thumbnail)
                 .AsNoTracking().SingleOrDefaultAsync();
        }

        public void RemoveRange(List<Guid> deleteIds)
        {
            this._attachmentRepository.Delete(a => deleteIds.Contains(a.Id), true);
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            return await this._attachmentRepository.DeleteAsync(a => a.Id == id, true);
        }
       
        public async Task<bool> UpdateAttachmentsTitlesAsync(List<Guid> ids, List<string> titles)
        {
            if (ids.IsNullOrEmpty())
                return false;

            var attachments = await _attachmentRepository.Table
                              .Where(a => ids.Contains(a.Id))
                              .OrderBy(a => a.Id)
                              .ToListAsync();

            if (attachments.Count != ids.Count)
                return false;

            for (int i = 0; i < ids.Count; i++)
            {
                for (int j = 0; j < attachments.Count; j++)
                {
                    Attachment attachment = attachments[j];
                    if (ids[i] == attachment.Id)
                    {
                        attachment.TitleEn = titles[i];
                        await _attachmentRepository.UpdateAsync(attachment, true);
                    }
                }
            }

            return true;
        }

        public void DeleteAttachmentFromFileSystem(string fileRelativePath)
        {
            if (string.IsNullOrEmpty(fileRelativePath))
            {
                return;
            }

            var filePath = $@"{_appSettingsService.AttachmentsPath}{fileRelativePath}";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task DeleteAttachmentFromDbAndFileSystem(Guid attachmentId)
        {
            if (attachmentId == Guid.Empty)
            {
                return;
            }
            var attachment = _attachmentRepository.TableNoTracking.FirstOrDefault(a => a.Id == attachmentId);
            var filePath = $@"{_appSettingsService.AttachmentsPath}{attachment.FilePath}";
            if (File.Exists(filePath))
            {
                //remove from file system
                File.Delete(filePath);
            }
            //remove from db
            await RemoveAsync(attachmentId);
        }
        //public async Task DeleteAttachmentsByTypeAndRequestId()
        //{

        //}
        private byte[] GenerateThumbnail(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var thumb = new Bitmap(220, 220);
                using (var bmp = Image.FromStream(ms))
                {
                    using (var g = Graphics.FromImage(thumb))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawImage(bmp, 0, 0, 220, 220);
                    }
                }

                using (var msWrite = new MemoryStream())
                {
                    thumb.Save(msWrite, ImageFormat.Png);
                    return msWrite.ToArray();
                }
            }
        }

        public byte[] GetThumbnailOfFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var thumbnail = GenerateThumbnail(bytes);
            return thumbnail;
        }

        private string SaveAttachmentToFileSystem(Attachment attach, byte[] fileBytes)
        {
            var relativeFolderPath = $"\\{DateTime.Now.Year}\\{DateTime.Now.Month}\\{DateTime.Now.Day}";
            var fullFolderPath = $"{_appSettingsService.AttachmentsPath}{relativeFolderPath}";
            var attachmentIdFormated = attach.Id.ToString("N").ToLower();
            var attachmentName = attach.FileName.ToLower().Trim().RemoveFromEnd(attach.Extension);
            var fileName = attachmentName + "_" + attachmentIdFormated[(attachmentIdFormated.Length - 4)..] + attach.Extension;
            var fileRelativePath = $@"{relativeFolderPath}\{fileName}";

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }
            using (var bw = new BinaryWriter(File.Open(Path.Combine(fullFolderPath, fileName), FileMode.OpenOrCreate)))
            {
                bw.Write(fileBytes);
            }
            return fileRelativePath;
        }

     

        public async Task<AttachmentType> GetAttachmentTypeByCode(string code)
        {
           return await  _attachmentTypeRepository.GetSingleAsync(a=>a.Code.StartsWith(code));//There is undefined space in codes
        }

    }
}
