using DefendCore.Application.Interfaces;
using DefendCore.Domain.Entities.Security;
using DefendCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DefendCore.Infrastructure.Services.Security
{
    public class IpSecurityService : IIpSecurityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IpSecurityService> _logger;

        public IpSecurityService(IUnitOfWork unitOfWork, ILogger<IpSecurityService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> TooManyFailedLogins(string ip, int maxAttempts, TimeSpan window)
        {
            var cutoff = DateTime.UtcNow - window;
            var failed = await _unitOfWork.GetRepository<LoginAudit, int>()
                .CountAsync(x => x.IpAddress == ip && x.Status == "Failed" && x.CreatedAt >= cutoff);

            return failed >= maxAttempts;
        }
        public bool ContainsMaliciousPayload(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return false;

            body = body.ToLower();

            return ContainsXss(body)
                || ContainsSqlInjection(body);
        }

        private bool ContainsXss(string body)
        {
            string[] xssPatterns =
            {
                "<script",
                "</script",
                "javascript:",
                "onerror=",
                "onload=",
                "onmouseover=",
                "onclick=",
                "eval(",
                "document.cookie",
                "window.location"
            };

            return xssPatterns.Any(body.Contains);
        }
        private bool ContainsSqlInjection(string body)
        {
            string[] sqlPatterns =
            {
                "' or 1=1",
                "\" or 1=1",
                "--",
                "/*",
                "*/",
                "union select",
                "drop table",
                "insert into",
                "update set",
                "delete from",
                "xp_cmdshell",
                "exec("
            };

            return sqlPatterns.Any(body.Contains);
        }
    }
}


