namespace DefendCore.Domain.Settings
{
    public class IpSecuritySettings
    {
        public int MaxFailedAttempts { get; set; }
        public int BanDurationMinutes { get; set; }
        public int MaxScanSizeKb { get; set; }
        public int FailedLoginWindowMinutes { get; set; }
    }
}
