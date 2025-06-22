using FluentValidation.Results;
using Exceptions_ApplicationException = NomNomGo.IdentityService.Domain.Exceptions.ApplicationException;

namespace NomNomGo.IdentityService.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    public class ValidationException : ApplicationException
    {
        /// <summary>
        /// Gets the validation errors
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        public ValidationException()
            : base("One or more validation failures have occurred.", "ValidationFailed")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        /// <param name="message">Error message</param>
        public ValidationException(string message)
            : base(message, "ValidationFailed")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with errors
        /// </summary>
        /// <param name="errors">Dictionary of validation errors</param>
        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.", "ValidationFailed")
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }
        
        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        /// <param name="failures">Validation failures</param>
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("One or more validation failures have occurred.", "ValidationFailed")
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(
                    failureGroup => failureGroup.Key,
                    failureGroup => failureGroup.ToArray());
        }
    }
}