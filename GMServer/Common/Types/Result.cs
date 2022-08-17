using Microsoft.AspNetCore.Mvc;
using System;

namespace SRC.Common.Types
{
    public readonly struct Result<T> where T : class
    {
        private readonly T Value;
        private readonly ServerError Error;

        public bool IsSuccess => Error is null;

        public Result(T value)
        {
            Value = value;
            Error = null;
        }

        public Result(ServerError e)
        {
            Error = e;
            Value = default;
        }

        public static implicit operator Result<T>(T value)
        {
            return new Result<T>(value);
        }

        public static implicit operator Result<T>(ServerError e)
        {
            return new Result<T>(e);
        }

        public R Match<R>(Func<T, R> onSuccess, Func<ServerError, R> onFail)
        {
            return IsSuccess ? onSuccess(Value) : onFail(Error);
        }

        public bool TryGetValue(out T result)
        {
            result = IsSuccess ? Value : default;

            return IsSuccess;
        }

        public IActionResult ToResponse()
        {
            return IsSuccess ? new OkObjectResult(Value) : Error;
        }
    }
}