using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class CompanyImagesResponse
    {
        public int CompanyId { get; set; }
        public string ImageUrl { get; set; } = null!;

        public string? Caption { get; set; }

        public bool IsCover { get; set; }
    }
}
