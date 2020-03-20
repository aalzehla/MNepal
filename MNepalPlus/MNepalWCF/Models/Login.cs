using System;

namespace MNepalWCF.Models
{
    public class Login
    {
        public int UserID { get; set; }
        public string Mobile { get; set; }
        public string DeviceID { get; set; }

        public bool? IsActiveDevice { get; set; }
        public DateTime? LoginDate { get; set; }
        public string GeneratedPass { get; set; }

        public string Mode { get; set; }
    }
}