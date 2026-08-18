using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.SharedServices.Dto
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] FileData { get; set; }
        public string FilePath { get; set; }
        public string? Description { get; set; }
        public string Extension { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsNew { get; set; }
    }
}
