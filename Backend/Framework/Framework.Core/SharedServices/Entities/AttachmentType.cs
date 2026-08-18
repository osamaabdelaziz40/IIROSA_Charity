// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttachmentType.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Framework.Core.Data;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.X86;

namespace Framework.Core.SharedServices.Entities
{
    #region usings

    #endregion

    public class AttachmentType : FullAuditedEntityBase<int>
    {
        public AttachmentType()
        {
        //    Attachments = new HashSet<Attachment>();
        }


        public string NameAr { get; set; }

        public string NameEn { get; set; }
        public string Code { get; set; }

        public string AllowedFilesExtension { get; set; }

        public int? ImageMaxHeight { get; set; }

        public int? ImageMaxWidth { get; set; }

        public bool IsImage { get; set; }

        public bool IsMandatory { get; set; }

        public int MaxSizeInMegabytes { get; set; }

        public int? RequestType { get; set; }
        public int? FileNumber { get; set; } = 1;

        
        //public ICollection<Attachment> Attachments { get; set; }
    }


    public enum AttachmentTypes
    {
        GeneralFileAttachment = 1, // All of extensions below
        GeneralImageAttachment = 2, // 'jpg,jpeg,png,tif,tiff,gif,bmp'
        GeneralAudioAttachment = 3, // 'wma,mp3,m4a'
        GeneralVideoAttachment = 4, // 'wmv,wav,mpg,mpeg,mp4,mov,flv,avi'
        GeneralTextAttachment = 5, // txt,rtf,pdf'
        GeneralCommaSeparatedAttachment = 6, // 'csv,xls,xlsx,xlt,xltx'
        GeneralWordDocumentAttachment = 7, // 'doc,docx,dot,dotx'
        GeneralPowerPointAttachment = 8, // 'ppt,ppsx,pptx,sldx'
        GeneralArchiveCompressedAttachment = 9, // 'zip,rar'
        GeneralPDFAttachment = 10, // 'pdf'
    }

    #region AttachmentFileExtensions
    public class AttachmentFileExtensions
    {
        public static string[] GeneralFileAttachment { get; set; } =
            { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".gif", ".bmp", ".wma",".mp3",".m4a", ".wmv",
            ".wav",".mpg",".mpeg",".mp4",".mov",".flv",".avi", ".txt", ".rtf", ".pdf", ".csv",".xls",".xlsx",".xlt",".xltx",
            ".doc", ".docx", ".dot", ".dotx", ".ppt", ".ppsx", ".pptx", ".sldx", ".zip", ".rar"};
        public static string[] GeneralImageAttachment { get; set; } = { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".gif", ".bmp" };
        public static string[] GeneralAudioAttachment { get; set; } = { ".wma", ".mp3", ".m4a" };
        public static string[] GeneralVideoAttachment { get; set; } = 
            { ".wmv", ".wav", ".mpg", ".mpeg", ".mp4", ".mov", ".flv", ".avi" };
        public static string[] GeneralTextAttachment { get; set; } = { ".txt", ".rtf", ".pdf" };
        public static string[] GeneralCommaSeparatedAttachment { get; set; } = { ".csv", ".xls", ".xlsx", ".xlt", ".xltx" };
        public static string[] GeneralWordDocumentAttachment { get; set; } = { ".doc", ".docx", ".dot", ".dotx" };
        public static string[] GeneralPowerPointAttachment { get; set; } = { ".ppt", ".ppsx", ".pptx", ".sldx" };
        public static string[] GeneralArchiveCompressedAttachment { get; set; } = { ".zip", ".rar" };
        public static string[] GeneralPDFAttachment { get; set; } = { ".pdf" };
    }

    #endregion

}