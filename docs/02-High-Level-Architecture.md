# High-Level Architecture

Building a secure application requires more than a single protection mechanism.

A common misconception is that Authentication and Authorization alone are enough to secure an API. While these layers are essential, they do not prevent malicious requests from reaching the application.

Attackers can still attempt:

* XSS attacks
* SQL Injection attacks
* Brute force login attempts
* Automated abuse and request flooding

For this reason, the security architecture should consist of multiple independent layers that work together to reduce risk.

This approach is commonly known as **Defense in Depth**.

Instead of relying on a single protection point, the application introduces security controls at different stages of the request lifecycle.

---

# Architecture Overview

The high-level request flow is illustrated below:

```text
Client Request
       │
       ▼
NGINX Reverse Proxy
       │
       ▼
ASP.NET Core Pipeline
       │
       ▼
IpSecurityMiddleware
       │
       ▼
Authentication
       │
       ▼
Authorization
       │
       ▼
Controllers
       │
       ▼
Business Logic
```

Each layer is responsible for a different aspect of security.

This separation ensures that security responsibilities are distributed across the system rather than concentrated in a single component.

---

# Layer 1: NGINX Reverse Proxy

The first security boundary exists before the request even reaches ASP.NET Core.

NGINX acts as a reverse proxy positioned in front of the application.

Its primary responsibilities include:

* Traffic filtering
* Rate limiting
* Request forwarding
* Load balancing
* Basic DDoS mitigation

Because NGINX operates before the application server, it can reject abusive traffic without consuming application resources.

This significantly reduces CPU, memory, and database load.

The role of NGINX will be discussed in more detail in the next section.

# Why NGINX Rate Limiting First?

In a production environment, rate limiting is typically implemented at the infrastructure layer before requests reach the application itself.

A common approach is to configure rate limiting within NGINX, which acts as a reverse proxy in front of the ASP.NET Core application.

By applying rate limiting at the reverse proxy level, abusive traffic can be blocked before consuming application resources such as:

- CPU
- Memory
- Database connections
- Application threads

This approach improves scalability and reduces the impact of automated attacks such as brute force attempts and request flooding.

For this reason, implementing rate limiting at the infrastructure layer is generally considered a recommended practice and is often the first line of defense in modern production systems.

In the context of this project, NGINX rate limiting is discussed as part of the overall security architecture but is not implemented directly.

The focus of this implementation is the application-level security layer built using ASP.NET Core middleware.

The custom IP Security Middleware should therefore be viewed as a complementary layer rather than a replacement for infrastructure-level protections.

Together, NGINX and the application security layer form a stronger defense strategy based on the principle of Defense in Depth.

> **Note**
>
> This project focuses on application-level security implementation using ASP.NET Core. Infrastructure-level protections such as NGINX rate limiting, WAFs, CDN protection,
> and DDoS mitigation are outside the scope of the implementation but are strongly recommended for production environments.

---

# Layer 2: ASP.NET Core Request Pipeline

Once a request passes through the reverse proxy, it enters the ASP.NET Core pipeline.

ASP.NET Core processes requests through a chain of middleware components.

Each middleware can inspect, modify, reject, or forward the request.

This pipeline architecture makes middleware an ideal location for implementing application-level security controls.

---

# Layer 3: IP Security Middleware

The custom IP Security Middleware is the core component of this module.

Its purpose is to intercept requests before they reach authentication and business logic layers.

The middleware performs several security checks, including:

* Blocked IP validation
* Request inspection
* XSS detection
* SQL Injection detection
* Brute force monitoring
* Security audit logging

If suspicious activity is detected, the middleware can immediately terminate the request and prevent further processing.

This follows the **Fail Fast** principle.

---

# Layer 4: Authentication

Authentication is responsible for verifying user identity.

At this stage, the system answers the question:

> Who is making this request?

Common authentication mechanisms include:

* JWT Bearer Tokens
* Cookies
* OAuth Providers
* Identity Providers

Authentication ensures that users are who they claim to be.

However, authentication alone does not guarantee that requests are safe.

A malicious authenticated user can still attempt attacks.

This is one reason why the security middleware executes before the authentication layer.

---

# Layer 5: Authorization

Authorization determines what an authenticated user is allowed to access.

At this stage, the system answers the question:

> What is this user allowed to do?

Authorization policies may include:

* Roles
* Claims
* Permissions
* Resource ownership

Even properly authorized users can submit malicious payloads.

For this reason, authorization should be viewed as one layer of protection rather than the entire security strategy.

---

# Layer 6: Controllers and Business Logic

Only after a request successfully passes all previous security layers does it reach the application endpoints.

At this stage, the request is processed by:

* Controllers
* Services
* Business rules
* Data access components

By filtering malicious requests earlier in the pipeline, the system avoids wasting resources on requests that should never reach business logic in the first place.

---

# Why Multiple Security Layers?

A single security mechanism can fail.

For example:

* Authentication may be bypassed through credential theft.
* Authorization rules may be misconfigured.
* Input validation may contain gaps.
* Infrastructure protections may be incomplete.

When multiple independent layers exist, the failure of one layer does not automatically compromise the entire system.

This is the foundation of Defense in Depth.

Each layer provides additional protection and increases the overall difficulty of a successful attack.

---

# Architectural Benefits

This layered security architecture provides several advantages:

* Early attack detection
* Reduced resource consumption
* Improved scalability
* Better separation of concerns
* Easier maintenance
* Stronger defense against common attack vectors

Most importantly, security becomes a shared responsibility across the entire request lifecycle rather than the responsibility of a single component.

The next section explains why rate limiting is intentionally implemented at the NGINX layer before requests reach ASP.NET Core.
