using DefendCore.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace DefendCore.Domain.Entities.Security
{
    [Table("BlockedIps", Schema = "Security")]
    public class BlockedIp : BaseEntity<int>
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = "Auto Ban";
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } // null = Permanent ban
    }
}
