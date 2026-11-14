namespace QuestForge.Core;

public static class ResultExtensions
{
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> transform) =>
        result.IsSuccess ? Result<U>.Success(transform(result.Value!)) : Result<U>.Failure(result.Error!);

    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> next) =>
        result.IsSuccess ? next(result.Value!) : Result<U>.Failure(result.Error!);
}
