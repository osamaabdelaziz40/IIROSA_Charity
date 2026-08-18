using System;

namespace Framework.Core.SharedServices.Entities
{
    public partial class AttachmentContent
    {
        public Guid Id { get; set; }

        // FK
        public Guid AttachmentId { get; set; }

        public byte[] FileContent { get; set; }

        public Attachment Attachment { get; set; }
    }
}
