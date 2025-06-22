namespace NomNomGo.IdentityService.Domain.Exceptions;

public class AlreadyExistsException : ApplicationException
{
    public AlreadyExistsException(string message, string code = null) : base(message, code)
    {
    }

    public AlreadyExistsException(string entitiyName, string paramName, string paramValue, string code = null) : base(
        $"{entitiyName} with {paramName} : {paramValue} is already exists",code)
    {
    }
}