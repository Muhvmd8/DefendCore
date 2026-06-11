namespace DefendCore.Application.Interfaces
{
    public interface IIpSecurityService
    {
        bool ContainsMaliciousPayload(string? body);
        Task<bool> TooManyFailedLogins(string ip, int limit, TimeSpan window);
    }
}
