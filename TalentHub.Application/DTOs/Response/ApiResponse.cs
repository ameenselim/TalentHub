using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public List<string>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = "")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public ApiResponse(string message, List<string>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }
    }
}
