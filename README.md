# DefendCore

DefendCore is a custom ASP.NET Core security module that adds an additional application-level security layer before requests reach business logic.

The module focuses on detecting suspicious requests, blocking malicious IPs, auditing security events, and mitigating common attack patterns such as XSS, SQL Injection, and brute force attacks.

## Features

* IP-based blocking
* XSS detection
* SQL Injection detection
* Brute force protection
* Security audit logging
* Configuration-driven security policies
* Middleware-based request inspection

## Architecture

<p align="center">
  <img width="500" hieght="500" src="https://github.com/user-attachments/assets/cb71820c-1b75-444f-85a7-b73649ecfa31" alt="High-Level-Architecture">
</p>

## Project Structure

```text
DefendCore
│
├── DefendCore.Domain
├── DefendCore.Application
├── DefendCore.Infrastructure
├── DefendCore.API
└── docs
```

## Security Components

* IpSecurityMiddleware
* IpSecurityService
* SecurityRequestHelper
* LoginAudits
* BlockedIps
* IpSecuritySettings

## Security Configuration

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

## Documentation

Detailed documentation is available in the `/docs` folder.

Topics include:

* Security Architecture
* Middleware Flow
* Request Inspection Strategy
* XSS Detection
* SQL Injection Detection
* Brute Force Protection
* Design Decisions
* Future Improvements

## Technologies

* ASP.NET Core
* Entity Framework Core
* SQL Server
* Middleware Pipeline
* Dependency Injection
* Repository Pattern
* Unit Of Work

## License

Educational and portfolio project.
