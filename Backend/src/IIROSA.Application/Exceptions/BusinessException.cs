namespace IIROSA.Application.Exceptions;

/// <summary>
/// Thrown when a business rule refuses the operation. Maps to HTTP 400 / 409 (architecture.md §5.1).
/// Nothing is written when this is thrown — refuse atomically.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class.
    /// </summary>
    public BusinessException(string message)
        : base(message)
    {
    }
}
