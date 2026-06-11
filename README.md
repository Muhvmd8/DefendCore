# DefendCore

DefendCore is a custom ASP.NET Core security module designed to provide an additional application-level security layer against common web attacks.

The module focuses on detecting suspicious requests, blocking malicious IP addresses, auditing security events, and protecting APIs from common attack patterns before requests reach the application's business logic.

The design follows the Defense in Depth principle by combining multiple security mechanisms instead of relying on a single protection point.

---

# Features

* IP-based blocking system
* XSS detection
* SQL Injection detection
* Brute force protection
* Security audit logging
* Configuration-driven security policies
* Request inspection filtering
* Middleware-based request interception
* Security event tracking
* Temporary and permanent IP bans

---

# High-Level Architecture

Client Request

↓

NGINX (Rate Limiting)

↓

ASP.NET Core Pipeline

↓

IpSecurityMiddleware

↓

Authentication

↓

Authorization

↓

Controllers

↓

Business Logic

The module acts as an additional security layer before requests reach the application's core functionality.

---

# Project Structure

```text
DefendCore
│
├── DefendCore.Domain
│   │
│   ├── Entities
│   │   │
│   │   ├── Common
│   │   │   └── BaseEntity.cs
│   │   │
│   │   └── Security
│   │       ├── LoginAudit.cs
│   │       └── BlockedIp.cs
│   │
│   ├── Interfaces
│   │   ├── IGenericRepository.cs
│   │   └── IUnitOfWork.cs
│   │
│   └── Settings
│       └── IpSecuritySettings.cs
│
├── DefendCore.Application
│   │
│   └── Interfaces
│       └── IIpSecurityService.cs
│
├── DefendCore.Infrastructure
│   │
│   ├── Persistence
│   │   │
│   │   ├── Contexts
│   │   │   └── ApplicationDbContext.cs
│   │   │
│   │   ├── Repositories
│   │   │   ├── GenericRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   │
│   │   └── Migrations
│   │
│   └── Services
│       │
│       └── Security
│           └── IpSecurityService.cs
│
├── DefendCore.API
│   │
│   ├── Helpers
│   │   └── SecurityRequestHelper.cs
│   │
│   ├── Middlewares
│   │   │
│   │   ├── Extensions
│   │   │   └── IpSecurityMiddlewareExtension.cs
│   │   │
│   │   └── IpSecurityMiddleware.cs
│   │
│   ├── Program.cs
│   └── appsettings.json
│
└── docs
    ├── 01-Introduction.md
    ├── 02-High-Level-Architecture.md
    ├── 03-Nginx-Rate-Limiting.md
    ├── 04-Security-Schema.md
    ├── 05-Blocked-Ip-Management.md
    ├── 06-Audit-Logging.md
    ├── 07-Configuration-Driven-Security.md
    ├── 08-SecurityRequestHelper.md
    ├── 09-IpSecurityService.md
    ├── 10-Xss-Detection.md
    ├── 11-Sql-Injection-Detection.md
    ├── 12-Brute-Force-Detection.md
    ├── 13-Middleware-Flow.md
    ├── 14-Design-Decisions.md
    ├── 15-Limitations.md
    ├── 16-Future-Improvements.md
    └── 17-Conclusion.md
```

# Architecture Layers

### Domain

Contains the core business entities, repository abstractions, and security configuration models.

### Application

Contains application contracts and service abstractions used by higher layers.

### Infrastructure

Contains data access implementation, Entity Framework Core configuration, repositories, Unit of Work implementation, and security services.

### API

Contains middleware, request helpers, dependency registration, and application entry point.

### Docs

Contains detailed technical documentation explaining the architecture, design decisions, security strategies, and implementation details.

---

# Security Components

## Middleware

The middleware is responsible for intercepting incoming requests and orchestrating all security checks.

## Security Service

The service layer contains security analysis logic and attack detection rules.

## SecurityRequestHelper

Responsible for deciding whether a request should be inspected before security analysis begins.

## LoginAudits

Stores security-related events for monitoring and investigation.

## BlockedIps

Stores blocked IP addresses and ban information.

## Configuration Settings

Security policies are managed through application configuration using ASP.NET Core Options Pattern.

---

# Security Policies

Example configuration:

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

These values can be adjusted without modifying the application code.

---

# Design Principles

* Defense in Depth
* Separation of Concerns
* Single Responsibility Principle
* Fail Fast
* Configuration over Hardcoding
* Maintainability
* Scalability

---

# Documentation

Detailed documentation is available inside the `/docs` directory.

Topics include:

* Security Architecture
* Middleware Design
* Request Inspection Strategy
* XSS Detection
* SQL Injection Detection
* Brute Force Protection
* Audit Logging
* Design Decisions
* Future Improvements

---

# Future Improvements

* Redis distributed tracking
* Web Application Firewall integration
* Security dashboards
* AI-based anomaly detection
* IP reputation services
* Distributed rate limiting
* Threat intelligence integration

---

# Technologies

* ASP.NET Core
* C#
* Entity Framework Core
* SQL Server
* Middleware Pipeline
* Dependency Injection
* Repository Pattern
* Unit Of Work
* Options Pattern

---

# License

This project is intended for educational, learning, and portfolio purposes.
