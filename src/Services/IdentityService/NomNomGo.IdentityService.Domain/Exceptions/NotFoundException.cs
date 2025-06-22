using ApplicationException = NomNomGo.IdentityService.Domain.Exceptions.ApplicationException;

namespace NomNomGo.IdentityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class
    /// </summary>
    /// <param name="message">Error message</param>
    public NotFoundException(string message) 
        : base(message, "NotFound")
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class for a specific entity
    /// </summary>
    /// <param name="name">Entity name</param>
    /// <param name="id">Entity identifier</param>
    public NotFoundException(string name, object id)
        : base($"Entity \"{name}\" with id {id} was not found.", "NotFound")
    {
    }
}