using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Dtos.Common
{
    public class JsonResponseModel<T>
    {
        public string? Status { get; set; }   // e.g. "Success" | "Error"
        public string? Message { get; set; }
        public T? Data { get; set; }
        public static JsonResponseModel<T> Ok(T data, string? message=null) =>
            new()
            {
                Status = "Success",
                Message = message,
                Data = data
            };
        public static JsonResponseModel<T> Fail(string? message) =>
            new()
            {
                Status = "Error",
                Message = message,
                Data = default
            };
    }
}
