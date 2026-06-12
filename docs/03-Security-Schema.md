# Security Schema Design

Modern security systems generate valuable information that should not be mixed with business data.

Examples include:

* Failed login attempts
* Suspicious requests
* Blocked IP addresses
* Security audit records

Storing these records alongside business entities can make the database harder to maintain and more difficult to analyze.

To solve this problem, the module introduces a dedicated database schema responsible for security-related data.

This approach creates a clear separation between application data and security data.

---

# Why Use a Dedicated Security Schema?

As applications grow, security information becomes increasingly important.

Security teams often need to answer questions such as:

* Which IP addresses have been blocked?
* How many brute force attempts occurred this week?
* Which endpoints are most frequently targeted?
* What suspicious payloads were submitted?

Keeping security records isolated makes these investigations significantly easier.

A dedicated schema also improves organization and maintainability by grouping all security-related entities in a single location.

---

# Security Schema Structure

The module introduces a dedicated schema named:

```sql
Security
```

The schema currently contains two primary entities:

* BlockedIps
* LoginAudits

These tables work together to support attack detection, incident investigation, and automated blocking.

---

# Security Data Model

The overall structure is intentionally simple.

```text
Security
│
├── BlockedIps
│
└── LoginAudits
```

Each table serves a different purpose within the security workflow.

---

# BlockedIps Entity

The first security entity is responsible for tracking banned IP addresses.

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

Whenever the system identifies malicious behavior, the offending IP address can be stored in this table.

Future requests originating from the same IP can then be blocked before reaching the application.

---

# Temporary and Permanent Bans

The entity supports both temporary and permanent blocking.

```csharp
public DateTime? ExpiresAt { get; set; }
```

When a value exists:

```text
Current Time < ExpiresAt
```

the ban remains active.

Once the expiration time is reached, the IP is automatically considered unblocked.

If:

```csharp
ExpiresAt == null
```

the ban becomes permanent.

This provides flexibility for different security policies.

---

# Why Store the Ban Reason?

Security decisions should be explainable.

For this reason, the entity stores the reason that triggered the block.

```csharp
public string Reason { get; set; }
```

Examples include:

* XSS Attempt
* SQL Injection Attempt
* Brute Force Detected
* Manual Administrator Ban

This information becomes valuable when investigating incidents or reviewing security reports.

---

# LoginAudits Entity

The second entity is responsible for storing security-related activity.

```csharp
[Table("LoginAudits", Schema = "Security")]
public class LoginAudit : BaseEntity<int>
{
    public string Username { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string Status { get; set; } = "Failed";

    public string? RequestBody { get; set; }
}
```

This table acts as the primary source of security telemetry within the system.

---

# Audit Trail Design

An audit trail is a historical record of security-related actions.

The purpose is to answer questions such as:

* Who attempted to access the system?
* From which IP address?
* Which endpoint was targeted?
* Was the operation successful?
* What payload was submitted?

Without audit records, investigating security incidents becomes extremely difficult.

For this reason, logging is considered a fundamental component of modern security architectures.

---

# Why Store Request Information?

When suspicious activity occurs, the request itself often contains valuable evidence.

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

Capturing relevant request information allows administrators to understand what happened and how the attack was attempted.

This information can later be used for:

* Incident response
* Security monitoring
* Threat analysis
* Rule improvements

---

# Database Context Registration

Both entities are exposed through the application's database context.

```csharp
public DbSet<LoginAudit> LoginAudits { get; set; }

public DbSet<BlockedIp> BlockedIps { get; set; }
```

This allows Entity Framework Core to manage the tables and generate the corresponding database schema through migrations.

---

# Security Schema Benefits

Using a dedicated security schema provides several advantages:

* Better separation of concerns
* Easier security investigations
* Improved maintainability
* Cleaner database organization
* Centralized security records
* Better support for future reporting and analytics

Most importantly, security data becomes a first-class component of the system rather than being scattered throughout business tables.

---

# Next Section

The next section explores how blocked IP addresses are used by the middleware to immediately reject malicious requests using the Fail Fast principle.
