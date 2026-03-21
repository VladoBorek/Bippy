namespace ResultPattern
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailed => !IsSuccess;
        public string? Error { get; }

        protected Result(bool isSuccess, string? error)
        {
            if (isSuccess && error is not null)
                throw new InvalidOperationException("A successful result cannot carry an error.");
            if (!isSuccess && string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException("A failed result must carry an error message.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Ok() => new(true, null);

        public static Result Fail(string error) => new(false, error);

        public static Result Fail(Result result) => new(false, result.Error);

        public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

        public static Result<T> Fail<T>(string error) => Result<T>.Fail(error);

        public static Result<T> Fail<T>(Result result) =>
            Result<T>.Fail(result.Error ?? "Cloned Result did not have Error set");
    }

    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        public T Value =>
            IsSuccess
                ? _value!
                : throw new InvalidOperationException(
                    $"Cannot access Value of a failed result. Error: {Error}"
                );

        private Result(T value)
            : base(true, null)
        {
            _value = value;
        }

        private Result(string error)
            : base(false, error) { }

        public static Result<T> Ok(T value)
        {
            return new(value);
        }

        public static new Result<T> Fail(string error)
        {
            return new(error);
        }
    }
}
