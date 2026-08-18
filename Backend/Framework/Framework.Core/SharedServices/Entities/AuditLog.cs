using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Framework.Core.SharedServices.Entities
{
    /// <summary>
    /// Comprehensive audit log entity for tracking all database changes and user actions
    /// Captures Create, Update, Delete operations on entities as well as Login/Logout events
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Unique identifier for this audit log entry
        /// </summary>
        [Key]
        public Guid AuditLogId { get; set; }

        /// <summary>
        /// The type of entity that was affected (e.g., "Family", "Orphan", "Charity", "UserLogin")
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string EntityType { get; set; }

        /// <summary>
        /// The ID of the entity that was affected
        /// Null for system-wide events like FailedLogin
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// The operation performed (Create, Update, Delete, Login, Logout, FailedLogin, Export, Restore)
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Operation { get; set; }

        /// <summary>
        /// ID of the user who performed the action
        /// Null for system-generated events or failed logins
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Username of the user who performed the action
        /// For failed logins, contains the attempted username
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// IP address from which the action was performed
        /// </summary>
        [MaxLength(64)]
        public string IpAddress { get; set; }

        /// <summary>
        /// UTC timestamp when the action occurred
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// JSON string containing detailed field changes
        /// Format: {"FieldName": {"old": "oldValue", "new": "newValue"}}
        /// For Create: contains all field values after creation (old is null)
        /// For Delete: contains all field values before deletion (new is null)
        /// For Update: contains only changed fields
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string FieldChanges { get; set; }

        /// <summary>
        /// Additional context information as JSON
        /// May include failure reasons, additional metadata, etc.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string AdditionalContext { get; set; }

        /// <summary>
        /// Optional correlation ID for linking related audit events
        /// Useful for tracking a series of related operations
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// User agent string from the HTTP request
        /// </summary>
        [MaxLength(512)]
        public string UserAgent { get; set; }
    }
}
