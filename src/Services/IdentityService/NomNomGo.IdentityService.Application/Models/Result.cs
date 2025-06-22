namespace NomNomGo.IdentityService.Application.Models
{
    /// <summary>
    /// Стандартный результат выполнения операции
    /// </summary>
    /// <typeparam name="T">Тип данных результата</typeparam>
    public class Result<T>
    {
        public bool Succeeded { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }

        private Result(bool succeeded, T? data, string? error)
        {
            Succeeded = succeeded;
            Data = data;
            Error = error;
        }

        public static Result<T> Success(T data) => new(true, data, null);
        public static Result<T> Failure(string error) => new(false, default, error);
    }

    /// <summary>
    /// Стандартный результат выполнения операции без данных
    /// </summary>
    public class Result
    {
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }

        private Result(bool succeeded, string? error)
        {
            Succeeded = succeeded;
            Error = error;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
    }
}