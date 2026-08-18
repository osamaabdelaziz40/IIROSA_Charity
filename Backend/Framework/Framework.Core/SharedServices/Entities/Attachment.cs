// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Attachment.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Framework.Core.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Framework.Core.SharedServices.Entities
{
    #region usings

    #endregion

    public partial class Attachment : FullAuditedEntityBase<Guid>
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public string TitleAr { get; set; }
        public string TitleEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public int? AttachmentTypeId { get; set; }
        public byte[] Thumbnail { get; set; }
        [NotMapped]
        public byte[] FileContent { get; set; }

        public AttachmentType AttachmentType { get; set; }
       

        public AttachmentContent AttachmentContent { get; set; }
    }

}


