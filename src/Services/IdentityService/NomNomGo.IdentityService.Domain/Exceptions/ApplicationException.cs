namespace NomNomGo.IdentityService.Domain.Exceptions;

/// <summary>
/// Base exception for all application-specific exceptions
/// </summary>
public abstract class ApplicationException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of the ApplicationException class
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="code">Error code</param>
    protected ApplicationException(string message, string code = null) 
        : base(message)
    {
        Code = code ?? GetType().Name;
    }
}