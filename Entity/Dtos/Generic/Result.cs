using Entity.Dtos.Errors;
using System;

namespace Entity.Dtos.Generic
{
    public class Result<T> //Single Item return
    {
        public T Content { get; set; }
        public Error Error { get; set; }
        public bool IsSuccess => Error == null;
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
    }
}
