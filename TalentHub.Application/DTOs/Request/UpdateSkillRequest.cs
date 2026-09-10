using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class UpdateSkillRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
