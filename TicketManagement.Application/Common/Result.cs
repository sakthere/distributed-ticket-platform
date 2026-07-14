using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Common
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }
        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }
}
