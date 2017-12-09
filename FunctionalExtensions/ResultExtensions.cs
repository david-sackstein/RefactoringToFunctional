using System;

namespace FunctionalExtensions
{
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this Maybe<T> maybe, string errorMessage) where T : class
        {
            if (maybe.HasNoValue)
                return Result.Fail<T>(errorMessage);

            return Result.Ok<T>(maybe.Value);
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
            {
                return Result.Fail<K>(result.Error);
            }
            return Result.Ok<K>(func(result.Value));
        }

        public static Result<K> OnSuccess<T, K>(this Result<T> result, Func<T, K> function)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return Result.Ok<K>(function(result.Value));
        }

        public static Result<K> OnSuccess<T, K>(this Result<T> result, Func<T, Result<K>> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return func(result.Value);
        }



        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsFailure)
                return Result.Fail<T>(result.Error);

            action(result.Value);

            return result;
        }

        public static Result<K> OnSuccess<K>(this Result result, Func<K> function)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return Result.Ok<K>(function());
        }

        // Considering using this one
        public static Result<K> OnEither<T, K>(this Result<T> result, 
            Func<string, Result<K>> onFailure,
            Func<T, Result<K>> onSuccess)
        {
            if (result.IsFailure)
            {
                return onFailure(result.Error);
            }

            return onSuccess(result.Value);
        }

        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Fail<T>(errorMessage);

            return result;
        }

        public static K OnBoth<T, K>(this Result<T> result, Func<Result<T>, K> function)
        {
            return function(result);
        }
    }
}