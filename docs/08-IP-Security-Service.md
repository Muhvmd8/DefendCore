# IP Security Service

The security middleware is responsible for intercepting incoming requests and controlling the security workflow.

However, placing all security-related logic directly inside the middleware would make the implementation difficult to maintain as the application grows.

To keep responsibilities separated, the module introduces a dedicated service layer responsible for security analysis and decision making.

```csharp
public class IpSecurityService : IIpSecurityService
```

This service acts as the central location for security-related business rules.

The middleware orchestrates the request flow, while the service performs the actual security checks.

This separation improves maintainability, readability, and testability.

---

# Constructor Dependencies

The service depends on two primary components.

```csharp
private readonly IUnitOfWork _unitOfWork;
private readonly ILogger<IpSecurityService> _logger;
```

The `IUnitOfWork` provides access to repositories and database operations.

The `ILogger` enables security-related logging and diagnostics.

These dependencies are injected through ASP.NET Core Dependency Injection.

```csharp
public IpSecurityService(
    IUnitOfWork unitOfWork,
    ILogger<IpSecurityService> logger)
{
    _unitOfWork = unitOfWork;
    _logger = logger;
}
```

This design follows the Dependency Inversion Principle by depending on abstractions rather than concrete implementations.

---

# Service Responsibilities

The service currently provides two core capabilities:

* Malicious payload detection
* Brute force attack detection

<img width="4262" height="1255" alt="Client Authentication-2026-06-12-162416" src="https://github.com/user-attachments/assets/f61c128c-877f-4651-b120-4acd9f6a43e5" />

Keeping these responsibilities inside a dedicated service prevents the middleware from becoming overloaded with business logic.

---

# Malicious Payload Detection

One of the primary responsibilities of the service is inspecting request payloads for suspicious content.

```csharp
public bool ContainsMaliciousPayload(string? body)
```

The method acts as a central entry point for all payload validation rules.

```csharp
return ContainsXss(body)
    || ContainsSqlInjection(body);
```

This creates a simple and extensible security pipeline.

As additional attack detection mechanisms are introduced, they can be added without modifying the middleware itself.

---

# Security Validation Pipeline

The payload inspection process follows a layered approach.

<img width="1800" height="1255" alt="Client Request Flow-2026-06-12-162840" src="https://github.com/user-attachments/assets/12e8889e-20a2-4297-8ca3-e173b50f75a7" />

Each method focuses on a single security concern.

This aligns with the Single Responsibility Principle and keeps the implementation easier to maintain.

---

# Brute Force Detection

The second responsibility of the service is identifying brute force login attacks.

A brute force attack typically involves repeatedly attempting authentication until valid credentials are discovered.

To detect this behavior, the service analyzes historical login audit records.

```csharp
public async Task<bool> TooManyFailedLogins(
    string ip,
    int maxAttempts,
    TimeSpan window)
```

The method determines whether an IP address has exceeded the configured failure threshold within a specific time window.

---

# Sliding Time Window

The service first calculates the beginning of the monitoring window.

```csharp
var cutoff = DateTime.UtcNow - window;
```

For example:

```text
Current Time: 10:00
Window: 5 Minutes

Cutoff: 09:55
```

Only login attempts occurring after the cutoff time are considered.

This approach is commonly known as a Sliding Window strategy.

---

# Login Audit Analysis

The service then queries the audit records.

```csharp
var failed = await _unitOfWork
    .GetRepository<LoginAudit, int>()
    .CountAsync(x =>
        x.IpAddress == ip &&
        x.Status == "Failed" &&
        x.CreatedAt >= cutoff);
```

The query counts failed login attempts associated with the same IP address during the configured time period.

This allows the system to identify repeated authentication failures that may indicate an automated attack.

---

# Security Decision

After counting failed attempts, the service compares the result against the configured threshold.

```csharp
return failed >= maxAttempts;
```

If the threshold is exceeded, the service signals that suspicious activity has been detected.

The middleware can then take appropriate action, such as blocking the IP address.

---

# Why Use a Service Layer?

Moving security logic into a dedicated service provides several benefits:

* Better separation of concerns
* Easier unit testing
* Improved maintainability
* Cleaner middleware implementation
* Better extensibility for future security rules

The middleware remains responsible for request orchestration, while the service becomes responsible for security analysis and decision making.

This results in a cleaner architecture that is easier to evolve as new security requirements emerge.

---

# Next Section

The next section explores how the service detects Cross-Site Scripting (XSS) attacks and why pattern-based payload inspection is used as an additional security layer.
