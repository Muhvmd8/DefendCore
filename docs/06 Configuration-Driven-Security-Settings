# Configuration-Driven Security Settings

Security requirements change over time.

A startup application may allow five failed login attempts before blocking an IP address, while a banking application may require a much stricter policy.

Hardcoding security values inside the source code makes future changes difficult and often requires redeployment whenever security requirements change.

To avoid this limitation, all security-related values are stored in application configuration.

```json
{
  "IpSecuritySettings": {
    "MaxFailedAttempts": 5,
    "BanDurationMinutes": 30,
    "MaxScanSizeKb": 50,
    "FailedLoginWindowMinutes": 5
  }
}
```

This approach allows security policies to be modified without changing the implementation itself.

The settings are mapped into a strongly typed configuration model.

```csharp
public class IpSecuritySettings
{
    public int MaxFailedAttempts { get; set; }

    public int BanDurationMinutes { get; set; }

    public int MaxScanSizeKb { get; set; }

    public int FailedLoginWindowMinutes { get; set; }
}
```

ASP.NET Core loads these values using the Options Pattern.

```csharp
builder.Services.Configure<IpSecuritySettings>(
    builder.Configuration.GetSection(nameof(IpSecuritySettings)));
```

This creates a clean separation between configuration and application logic.

## Benefits

* No hardcoded security values
* Easier policy updates
* Environment-specific configuration
* Improved maintainability
* Better scalability
* Strongly typed access to settings

As security requirements evolve, administrators can update policies without modifying the implementation itself.
