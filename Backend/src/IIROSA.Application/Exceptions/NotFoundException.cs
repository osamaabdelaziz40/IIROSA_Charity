namespace IIROSA.Application.Exceptions;

/// <summary>
/// Thrown when the requested record does not exist. Maps to HTTP 404 (architecture.md §5.1).
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException(Type entityType, object entityId)
        : base($"{entityType.Name} with id '{entityId}' was not found")
    {
        EntityType = entityType.Name;
        EntityId = entityId.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with an entity
    /// name (for call sites without the Type at hand, e.g. <c>nameof(Orphan)</c>).
    /// </summary>
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found")
    {
        EntityType = entityName;
        EntityId = entityId.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Gets the name of the entity type that was not found.
    /// </summary>
    public string? EntityType { get; }

    /// <summary>
    /// Gets the identifier of the record that was not found.
    /// </summary>
    public string? EntityId { get; }
}
