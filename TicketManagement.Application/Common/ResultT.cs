using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Common
{
    public class Result<T> : Result
    {
        public T? Value { get; }

        public Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, Error.None);
        public static Result<T> Failure(Error error) => new(false, default, error);
    }
}
