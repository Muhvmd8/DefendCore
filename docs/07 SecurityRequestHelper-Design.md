# SecurityRequestHelper Design

Inspecting every incoming request may seem like a good security practice at first glance.

However, in real-world applications, unnecessary inspection can negatively impact performance and scalability.

For example, scanning image uploads, large files, or requests that do not contain user input provides little security value while consuming additional CPU and memory resources.

To solve this problem, the module introduces a dedicated helper responsible for deciding whether a request should be inspected before any security analysis begins.

This follows the principle of performing security checks only where they provide meaningful value.

---

## Request Inspection Strategy

The helper first determines whether the HTTP method should be inspected.

```csharp
public static bool ShouldInspectRequest(HttpRequest request)
    => HttpMethods.IsPost(request.Method)
    || HttpMethods.IsPut(request.Method)
    || HttpMethods.IsPatch(request.Method);
```

The module focuses on:

* POST
* PUT
* PATCH

These request types typically contain user-generated input and are therefore more likely to carry malicious payloads.

---

## Content-Type Validation

Not every request body can be inspected using text analysis.

For this reason, the helper validates the content type before inspection begins.

```csharp
public static bool IsSafeContentType(HttpRequest request)
```

Supported content types include:

* application/json
* application/x-www-form-urlencoded
* text/plain

Binary content such as images and videos is ignored.

---

## Payload Size Protection

Reading very large request bodies can consume significant memory and processing power.

To avoid turning the security layer into a performance bottleneck, the helper validates payload size before scanning.

```csharp
public static bool IsPayloadSizeAllowed(
    HttpRequest request,
    int maxScanBodySize)
```

Only requests below the configured threshold are inspected.

This keeps request analysis lightweight and predictable even under high traffic conditions.

## Benefits

* Reduced processing overhead
* Better application performance
* Lower memory consumption
* More scalable request inspection
* Improved separation of concerns

The helper acts as a lightweight filtering layer, ensuring that only relevant requests proceed to the security analysis stage.
