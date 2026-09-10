using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class UpdateCompanyImageRequest
    {
        public string? Caption { get; set; }
        public bool IsCover { get; set; }
    }
}
