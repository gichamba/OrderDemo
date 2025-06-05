namespace OrderDemo.Application.Common.Results {

    /// <summary>
    /// Represents a successful operation result containing the data.
    /// </summary>
    /// <param name="Data">The data resulting from the successful operation.</param>
    public record Success<T>(T Data);

    /// <summary>
    /// Represents a validation failure with a collection of errors.
    /// </summary>
    /// <param name="Errors">The collection of validation errors.</param>
    public record ValidationFailure(IReadOnlyList<ValidationError> Errors);

    /// <summary>
    /// Represents a single validation error.
    /// </summary>
    /// <param name="Message">The error message describing the validation failure.</param>
    /// <param name="MemberNames">The names of the properties that caused the error.</param>
    public record ValidationError(string Message, string[] MemberNames);

    /// <summary>
    /// Represents a failure due to a resource not being found.
    /// </summary>
    /// <param name="Message">The error message describing the not found condition.</param>
    public record NotFound(string Message);

    /// <summary>
    /// Represents a failure due to unauthorized access.
    /// </summary>
    /// <param name="Message">The error message describing the unauthorized condition.</param>
    /// <param name="RequiredRole">The role required for access, if applicable.</param>
    public record Unauthorized(string Message, string? RequiredRole = null);

    /// <summary>
    /// Represents an unexpected error that occurred during processing.
    /// </summary>
    /// <param name="Message">The error message describing the unexpected error.</param>
    /// <param name="InnerException">The inner exception, if any.</param>
    public record UnexpectedError(string Message, Exception? InnerException = null);

    /// <summary>
    /// Represents a failure due to an attempt to create a resource that already exists.
    /// </summary>
    /// <param name="Message">The error message describing the already exists condition.</param>
    public record AlreadyExists(string Message);

    /// <summary>
    /// Represents a failure due to an attempt to delete a record that is currently in use or referenced by other records.
    /// </summary>
    /// <param name="Message">The error message describing why the record cannot be deleted.</param>
    /// <param name="ReferencingEntities">Optional: A list of entities or types that are currently referencing this record.</param>
    public record InUse(string Message, IReadOnlyList<string>? ReferencingEntities = null);
}