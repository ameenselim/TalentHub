using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Domain.Entities
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
        public bool IsUsed { get; set; } = false;
    }
}
