# Real Client IP Detection

In modern production environments, ASP.NET Core applications are rarely exposed directly to the internet.

Instead, requests typically pass through one or more infrastructure components before reaching the application, such as:

* NGINX
* Load Balancers
* API Gateways
* Reverse Proxies
* Cloudflare

A simplified architecture may look like this:

```text
Client
   │
   ▼
NGINX Reverse Proxy
   │
   ▼
ASP.NET Core API
```

In this scenario, the incoming connection received by ASP.NET Core originates from the reverse proxy rather than the actual client.

As a result, the following code may return the proxy IP address instead of the user's real IP address:

```csharp
context.Connection.RemoteIpAddress
```

This can create serious issues for security systems that rely on IP-based tracking and blocking.

For example, blocking the reverse proxy IP would effectively block all incoming traffic instead of the malicious client.

---

# Forwarded Headers Middleware

To solve this problem, ASP.NET Core provides built-in support for forwarded headers.

The reverse proxy forwards the original client information through HTTP headers such as:

```http
X-Forwarded-For
X-Forwarded-Proto
```

ASP.NET Core can then restore the original client IP before requests reach the custom middleware.

Configuration typically looks like:

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});
```

And inside the request pipeline:

```csharp
app.UseForwardedHeaders();
```

---

# Why Is This Important?

The entire IP Security Module depends on accurate client identification.

Features such as:

* IP blocking
* Brute force detection
* Security auditing
* Threat monitoring

all rely on knowing the actual source of the request.

Without forwarded header processing, security decisions may be based on the reverse proxy IP rather than the real attacker.

For this reason, enabling forwarded headers is considered a recommended practice whenever the application is deployed behind a reverse proxy.

Once the original client IP has been restored, the middleware can safely perform IP-based security analysis and blocking decisions.
