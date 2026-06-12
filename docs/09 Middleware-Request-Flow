# Middleware Request Flow

The middleware is the entry point of the entire security module.

Its primary responsibility is intercepting incoming requests before they reach authentication, authorization, controllers, or business logic.

By executing early in the ASP.NET Core pipeline, the middleware can identify suspicious activity and stop malicious requests before additional application resources are consumed.

This follows the **Fail Fast** principle, which aims to reject dangerous requests as early as possible.

---

# Middleware Overview

The security layer is implemented as a custom ASP.NET Core middleware.

```csharp
public class IpSecurityMiddleware
```

The middleware acts as a coordinator for the entire security workflow.

It does not contain the detection logic itself.

Instead, it delegates security decisions to the `IpSecurityService` while controlling the overall request flow.

---

# Constructor Dependencies

The middleware depends on three primary components.

```csharp
private readonly RequestDelegate _next;
private readonly IpSecuritySettings _settings;
private readonly ILogger<IpSecurityMiddleware> _logger;
```

### RequestDelegate

```csharp
private readonly RequestDelegate _next;
```

Represents the next middleware in the ASP.NET Core pipeline.

The middleware uses `_next` to continue request processing once security checks are completed.

---

### Configuration Settings

```csharp
private readonly IpSecuritySettings _settings;
```

Provides access to all security-related configuration values such as:

* Maximum failed login attempts
* Ban duration
* Request size limits
* Detection windows

This keeps security policies externalized from implementation code.

---

### Logging

```csharp
private readonly ILogger<IpSecurityMiddleware> _logger;
```

Used to record security-related events including:

* Blocked IP access attempts
* XSS attacks
* SQL Injection attempts
* Brute force detection

These logs become valuable during monitoring and incident investigations.

---

# Middleware Dependencies

The `Invoke` method receives the services required during request execution.

```csharp
public async Task Invoke(
    HttpContext context,
    ApplicationDbContext db,
    IIpSecurityService security)
```

This provides access to:

* Current HTTP request
* Database operations
* Security analysis services

Together, these dependencies allow the middleware to inspect requests and enforce security policies.

---

# Real Client IP Detection

The first step is identifying the client IP address.

```csharp
var ip = context.Connection
    .RemoteIpAddress?
    .ToString() ?? "unknown";
```

This IP address becomes the foundation for:

* Blocking decisions
* Brute force tracking
* Audit logging

In production environments running behind NGINX or another reverse proxy, ASP.NET Core should use:

```csharp
app.UseForwardedHeaders();
```

This ensures the middleware receives the real client IP rather than the reverse proxy IP.

Without forwarded headers, IP-based security controls may become unreliable.

---

# Blocked IP Validation

Before performing any expensive processing, the middleware checks whether the IP address is already blocked.

```csharp
var blocked = await db.BlockedIps
    .FirstOrDefaultAsync(...)
```

If an active ban exists, the request is rejected immediately.

```csharp
context.Response.StatusCode = 403;

await context.Response.WriteAsync(
    "Your IP is banned.");

return;
```

The request never reaches:

* Authentication
* Authorization
* Controllers
* Business Logic

This minimizes unnecessary resource consumption.

---

# Request Body Inspection

Many attacks are hidden inside request payloads.

Examples include:

```json
{
  "email": "<script>alert('xss')</script>"
}
```

or

```json
{
  "username": "' OR 1=1 --"
}
```

To analyze incoming data, the middleware must read the request body.

However, this introduces an important challenge.

---

# Understanding Request.Body

Inside ASP.NET Core, the request body is represented as a stream.

```csharp
context.Request.Body
```

Internally, HTTP payloads arrive as a sequence of bytes.

```text
Client Request
      │
      ▼
Byte Stream
      │
      ▼
Request.Body
```

A stream is designed for forward-only reading.

Once consumed, its contents are no longer available.

---

# The Problem with Reading Streams

Suppose the middleware reads the request body.

```csharp
var body =
    await reader.ReadToEndAsync();
```

Without additional configuration, the controller would later receive an empty body because the stream has already been consumed.

This would break normal request processing.

---

# Enabling Request Buffering

To solve this problem, ASP.NET Core provides request buffering.

```csharp
context.Request.EnableBuffering();
```

Buffering allows the request body to be read multiple times.

Internally, ASP.NET Core temporarily stores the request content in memory and may use temporary files for larger payloads.

This enables both:

* The security middleware
* The controller

to access the same request body.

---

# Reading the Request Payload

Once buffering is enabled, the middleware reads the incoming payload.

```csharp
using var reader = new StreamReader(
    context.Request.Body,
    leaveOpen: true);

body = await reader.ReadToEndAsync();
```

The byte stream is converted into text so it can be analyzed for suspicious patterns.

Examples include:

* XSS payloads
* SQL Injection payloads
* Malicious user input

This inspection occurs before the request reaches application logic.

---

# Resetting Stream Position

After reading is complete, the stream position must be restored.

```csharp
context.Request.Body.Position = 0;
```

This is a critical step.

Without resetting the position, downstream middleware and controllers would not be able to read the request body.

Think of it as rewinding a video back to the beginning after watching it.

---

# Payload Validation

After reading the request body, the middleware delegates analysis to the security service.

```csharp
security.ContainsMaliciousPayload(body)
```

The service performs:

* XSS detection
* SQL Injection detection

If malicious content is detected, the request is blocked immediately.

---

# Automatic IP Blocking

When malicious activity is detected, the offending IP address is added to the security database.

```csharp
db.BlockedIps.Add(new BlockedIp
{
    IpAddress = ip,
    Reason = "XSS Attempt",
    ExpiresAt = DateTime.UtcNow
        .AddMinutes(_settings.BanDurationMinutes)
});
```

Future requests originating from the same IP can then be rejected instantly.

---

# Continuing the Pipeline

If all security checks pass, the request continues through the remaining middleware components.

```csharp
await _next(context);
```

This transfers execution to:

* Authentication
* Authorization
* Controllers
* Business Logic

The request proceeds normally as if the security middleware never intervened.

---

# Post-Execution Security Checks

Some security decisions require information that only becomes available after the request has been processed.

One example is brute force detection.

```csharp
await security.TooManyFailedLogins(...)
```

This check occurs after `_next(context)` because login audit records may only exist once authentication processing has completed.

---

# Security Audit Logging

When suspicious activity is detected, the middleware records a security audit entry.

```csharp
db.LoginAudits.Add(new LoginAudit
{
    Username = ...,
    IpAddress = ip,
    Endpoint = context.Request.Path,
    Status = ...,
    RequestBody = body
});
```

These records provide visibility into:

* Attack attempts
* User activity
* Security incidents
* Investigation data

Audit logs become an important source of information during forensic analysis.

---

# Complete Middleware Flow

The complete workflow can be summarized as follows.

```text
Request
   │
   ▼
Get Client IP
   │
   ▼
Blocked IP Check
   │
   ▼
Enable Buffering
   │
   ▼
Read Request Body
   │
   ▼
XSS Detection
   │
   ▼
SQL Injection Detection
   │
   ▼
Next Middleware
   │
   ▼
Brute Force Detection
   │
   ▼
Audit Logging
   │
   ▼
Response
```

The middleware acts as the central coordinator of the entire security module, ensuring that suspicious requests are identified, analyzed, logged, and blocked before they can negatively impact the application.
