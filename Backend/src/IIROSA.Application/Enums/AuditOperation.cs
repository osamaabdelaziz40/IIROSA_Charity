namespace IIROSA.Application.Enums
{
    /// <summary>
    /// Defines the types of operations that can be logged in the audit system
    /// </summary>
    public enum AuditOperation
    {
        /// <summary>
        /// Entity creation operation
        /// </summary>
        Create = 1,

        /// <summary>
        /// Entity update operation
        /// </summary>
        Update = 2,

        /// <summary>
        /// Entity deletion operation
        /// </summary>
        Delete = 3,

        /// <summary>
        /// User login operation
        /// </summary>
        Login = 4,

        /// <summary>
        /// User logout operation
        /// </summary>
        Logout = 5,

        /// <summary>
        /// Failed login attempt
        /// </summary>
        FailedLogin = 6,

        /// <summary>
        /// Audit log export operation
        /// </summary>
        Export = 7,

        /// <summary>
        /// Entity restoration from audit log
        /// </summary>
        Restore = 8
    }
}
