# Blocked IP Management

One of the primary objectives of the security module is preventing repeated attacks from the same source.

When a malicious request is detected, allowing the attacker to continue interacting with the application provides little value and unnecessarily consumes system resources.

To address this problem, the module introduces an IP-based blocking mechanism.

The purpose of this mechanism is simple:

* Identify malicious activity
* Record the offending IP address
* Reject future requests originating from that IP

This creates an automated response system capable of slowing down or completely stopping common attack patterns.

---

# Why Block IP Addresses?

Attackers rarely stop after a single attempt.

A successful attack often involves multiple requests sent over a short period of time.

Examples include:

* Brute force login attempts
* Automated scanning tools
* XSS payload testing
* SQL Injection probing

Without an automated blocking mechanism, the application would continue processing requests from the same malicious source.

This increases resource consumption and creates unnecessary load on the system.

By introducing IP blocking, the application can stop suspicious traffic before additional security checks are performed.

---

# The BlockedIps Entity

Blocked IP addresses are stored in a dedicated security table.

```csharp
[Table("BlockedIps", Schema = "Security")]
public class BlockedIp : BaseEntity<int>
{
    public string IpAddress { get; set; } = string.Empty;

    public string Reason { get; set; } = "Auto Ban";

    public DateTime BlockedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }
}
```

Each record represents a security decision made by the system.

The table contains:

* The blocked IP address
* The reason for the block
* The time the block occurred
* The expiration date of the ban

---

# Temporary Blocking Strategy

In many scenarios, a permanent ban is unnecessary.

For example, a user may accidentally trigger security rules by repeatedly entering an incorrect password.

For this reason, the module supports temporary bans.

When suspicious activity is detected, the middleware creates a block record with an expiration date.

```csharp
db.BlockedIps.Add(new BlockedIp
{
    IpAddress = ip,
    Reason = "Brute Force Detected",
    ExpiresAt = DateTime.UtcNow
        .AddMinutes(_settings.BanDurationMinutes)
});
```

The duration is controlled through configuration.

```json
{
  "IpSecuritySettings": {
    "BanDurationMinutes": 30
  }
}
```

This approach provides a balance between security and usability.

---

# Permanent Blocking Support

Some organizations may choose to permanently block highly suspicious sources.

The entity supports this behavior through a nullable expiration field.

```csharp
public DateTime? ExpiresAt { get; set; }
```

If:

```csharp
ExpiresAt == null
```

the ban remains active indefinitely.

This design allows both temporary and permanent blocking strategies without requiring additional database structures.

---

# Fail Fast Validation

The first operation performed by the middleware is checking whether the incoming IP address is already blocked.

```csharp
var blocked = await db.BlockedIps
    .FirstOrDefaultAsync(b =>
        b.IpAddress == ip &&
        (b.ExpiresAt == null ||
         b.ExpiresAt > DateTime.UtcNow));
```

This query determines whether:

* The IP exists in the block list
* The ban is still active

If a matching record is found, the request is immediately rejected.

---

# Immediate Request Rejection

Once a blocked IP is detected, the middleware terminates the request pipeline.

```csharp
if (blocked != null)
{
    context.Response.StatusCode = 403;

    await context.Response.WriteAsync(
        "Your IP is banned.");

    return;
}
```

The request never reaches:

* Authentication
* Authorization
* Controllers
* Business Logic
* Database Operations

This follows the **Fail Fast** principle.

Rejecting malicious traffic as early as possible reduces resource consumption and improves application performance.

---

# Why Use HTTP 403?

The middleware returns:

```http
403 Forbidden
```

This status code indicates that the server understood the request but refuses to process it.

Using a dedicated response code makes it easier to:

* Identify blocked requests
* Monitor security events
* Integrate with logging systems
* Analyze attack patterns

---

# Logging Blocked Requests

Whenever a blocked IP attempts to access the system, the event is logged.

```csharp
_logger.LogWarning(
    "Blocked IP tried access. IP: {IP}, Path: {Path}",
    ip,
    context.Request.Path);
```

This provides visibility into:

* Repeated attack attempts
* Targeted endpoints
* Block effectiveness

Logging is especially important for incident investigation and threat analysis.

---

# Automatic Blocking Workflow

The complete blocking workflow can be summarized as follows:

```text
Request Arrives
       │
       ▼
Check BlockedIps Table
       │
       ▼
IP Found?
   ┌───────┐
   │ Yes   │
   └───────┘
       │
       ▼
Return 403 Forbidden
       │
       ▼
Stop Request Processing
```

If the IP is not blocked, the request continues through the remaining security checks.

---

# Security Benefits

The IP blocking mechanism provides several advantages:

* Reduces repeated attack attempts
* Minimizes unnecessary processing
* Protects application resources
* Supports automated threat response
* Improves security visibility
* Implements Fail Fast behavior

Most importantly, the system can actively respond to suspicious behavior rather than simply detecting it.

---

# Next Section

The next section explores how security audit logging is implemented using the `LoginAudits` table and why audit trails are essential for monitoring and investigating security events.
