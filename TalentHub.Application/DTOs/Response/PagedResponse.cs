using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class PagedResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public IEnumerable<T> Data { get; set; } = [];

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}
