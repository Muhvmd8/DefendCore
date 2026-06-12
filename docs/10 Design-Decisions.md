# Design Decisions

Building a security module is not only about detecting attacks.

Equally important are the architectural decisions that determine how the module behaves under load, how easy it is to maintain, and how well it integrates with the rest of the application.

This section explains the most important design decisions behind the implementation and the reasoning that motivated each choice.

---

# Middleware-Based Security

The security layer is implemented as a custom ASP.NET Core middleware.

```csharp
app.UseIpSecurity();
```

This decision allows security checks to execute early in the request pipeline before requests reach:

- Authentication
- Authorization
- Controllers
- Business Logic

By validating requests as early as possible, the system reduces unnecessary processing and follows the Fail Fast principle.

---

# Service-Based Security Logic

The middleware is responsible for orchestration only.

Actual security decisions are delegated to:

```csharp
IpSecurityService
```

This separation follows the Single Responsibility Principle.

Responsibilities become clearly separated:

```text
Middleware
    │
    ▼
Request Coordination

Service
    │
    ▼
Security Analysis
```

This approach improves:

- Maintainability
- Testability
- Readability
- Extensibility

---

# Dedicated Security Schema

Security-related data is isolated inside a dedicated database schema.

```text
Security
│
├── BlockedIps
│
└── LoginAudits
```

Rather than mixing security information with business entities, the module stores security records separately.

Benefits include:

- Cleaner database organization
- Easier monitoring
- Simpler investigations
- Better scalability

---

# Configuration-Driven Policies

Security thresholds are stored in configuration instead of source code.

```json
{
  "IpSecuritySettings": {
    "MaxFailedAttempts": 5,
    "BanDurationMinutes": 30
  }
}
```

This allows administrators to modify security policies without changing implementation details.

The result is a more flexible and maintainable solution.

---

# Fail Fast Approach

The middleware checks blocked IP addresses before performing any expensive operations.

```csharp
if (blocked != null)
{
    return;
}
```

This prevents unnecessary:

- Database operations
- Authentication processing
- Authorization checks
- Controller execution

Failing early improves both security and performance.

---

# Selective Request Inspection

Not every request is inspected.

The helper limits inspection to:

- POST
- PUT
- PATCH

Requests must also satisfy:

- Supported content type
- Acceptable payload size

This avoids wasting resources on requests that provide little security value.

---

# Buffered Request Reading

The module reads the request body before controllers execute.

Since ASP.NET Core request bodies are streams that can normally be read only once, buffering is enabled.

```csharp
context.Request.EnableBuffering();
```

This allows:

- Security inspection
- Controller processing

to access the same request body safely.

Without buffering, reading the payload inside the middleware would prevent downstream components from accessing it.

---

# Temporary IP Blocking

The module uses temporary bans rather than permanent bans by default.

```csharp
ExpiresAt = DateTime.UtcNow
    .AddMinutes(30);
```

This reduces the risk of accidentally blocking legitimate users forever while still slowing down malicious activity.

Temporary bans provide a better balance between security and usability.

---

# Database-Driven Brute Force Detection

Failed login attempts are tracked using audit records stored in the database.

```csharp
LoginAudits
```

Instead of storing counters in memory, the module relies on persistent audit data.

Benefits include:

- Historical visibility
- Better auditing
- Survives application restarts
- Simpler implementation

Although distributed caching solutions such as Redis may scale better, database-based tracking is sufficient for many applications and easier to understand.

---

# Defense in Depth

The module is intentionally designed as an additional security layer rather than the primary security mechanism.

It complements existing protections such as:

- Authentication
- Authorization
- Parameterized Queries
- Content Security Policy (CSP)
- NGINX Rate Limiting
- Web Application Firewalls (WAF)

No single security control should be trusted on its own.

The overall design follows the Defense in Depth principle, where multiple independent layers work together to reduce risk.

---

# Production Considerations

In a production environment, additional infrastructure-level protections should be implemented before requests reach the ASP.NET Core application.

Examples include:

- NGINX Rate Limiting
- Cloudflare Protection
- API Gateways
- WAF Solutions

These layers can block malicious traffic before it consumes backend resources.

The IP Security Module should therefore be viewed as an application-level security layer rather than a replacement for infrastructure security controls.

---

# Summary

The module was designed around several key engineering principles:

- Separation of Concerns
- Single Responsibility Principle
- Fail Fast
- Defense in Depth
- Configuration-Driven Design
- Maintainability
- Scalability

Together, these decisions create a security architecture that remains simple enough for educational purposes while still reflecting many of the patterns commonly used in real-world ASP.NET Core applications.
