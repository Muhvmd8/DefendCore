using DefendCore.Application.Interfaces;
using DefendCore.Domain.Entities.Security;
using DefendCore.Domain.Settings;
using DefendCore.Infrastructure.Presistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static DefendCore.API.Helpers.SecurityRequestHelper;

namespace DefendCore.API.Middlewares
{
    public class IpSecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IpSecuritySettings _settings;
        private readonly ILogger<IpSecurityMiddleware> _logger;

        public IpSecurityMiddleware(
            RequestDelegate next,
            IOptions<IpSecuritySettings> settings,
            ILogger<IpSecurityMiddleware> logger)
        {
            _next = next;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext context,
            ApplicationDbContext db,
            IIpSecurityService security)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            bool riskDetected = false;
            string? body = null;

            // 1. Check IP ban early 
            var blocked = await db.BlockedIps.FirstOrDefaultAsync(b =>
                b.IpAddress == ip &&
                (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow));

            if (blocked != null)
            {
                _logger.LogWarning(
                    "Blocked IP tried access. IP: {IP}, Path: {Path}",
                    ip,
                    context.Request.Path);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Your IP is banned.");
                return;
            }

            // 2. Read request body safely
            if (ShouldInspectRequest(context.Request)
                && IsSafeContentType(context.Request)
                && IsPayloadSizeAllowed(
                    context.Request,
                    _settings.MaxScanSizeKb * 1024))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    leaveOpen: true);

                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // 3. XSS detection rule
            if (security.ContainsMaliciousPayload(body))
            {
                _logger.LogWarning(
                    "XSS attempt detected. IP: {IP}, Path: {Path}",
                    ip,
                    context.Request.Path);

                db.BlockedIps.Add(new BlockedIp
                {
                    IpAddress = ip,
                    Reason = "XSS Attempt",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.BanDurationMinutes)
                });

                riskDetected = true;

                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Your IP was banned due to malicious activity.");

                await db.SaveChangesAsync();
                return;
            }

            // 4. Continue pipeline
            await _next(context);

            // 5. Brute force detection
            if (await security.TooManyFailedLogins(
                ip,
                _settings.MaxFailedAttempts,
                TimeSpan.FromMinutes(_settings.FailedLoginWindowMinutes)))
            {
                _logger.LogWarning(
                    "Brute force detected. IP: {IP}, Path: {Path}",
                    ip,
                    context.Request.Path);

                db.BlockedIps.Add(new BlockedIp
                {
                    IpAddress = ip,
                    Reason = "Brute Force Detected",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.BanDurationMinutes)
                });

                riskDetected = true;
            }

            // 6. Audit logging
            if (riskDetected)
            {
                _logger.LogInformation(
                    "Security audit. IP: {IP}, User: {User}, Path: {Path}, Status: {Status}",
                    ip,
                    context.User?.Identity?.Name ?? "Anonymous",
                    context.Request.Path,
                    context.Response.StatusCode);

                db.LoginAudits.Add(new LoginAudit
                {
                    Username = context.User?.Identity?.Name ?? "Anonymous",
                    IpAddress = ip,
                    Endpoint = context.Request.Path,
                    Status = context.Response.StatusCode < 400 ? "Success" : "Failed",
                    RequestBody = body
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
