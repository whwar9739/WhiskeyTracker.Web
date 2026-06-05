🚨 Severity: CRITICAL
💡 Vulnerability: Google Photos integration allowed arbitrary URLs supplied by users to be fetched using `HttpClient.GetAsync()` without validation.
🎯 Impact: Attackers could supply a URL pointing to internal network services or metadata endpoints, potentially leading to Server-Side Request Forgery (SSRF).
🔧 Fix: Added strict URL validation using `Uri.TryCreate` with `UriKind.Absolute` before invoking `HttpClient.GetAsync()`. The fix ensures the URL uses HTTPS and the host matches either `.googleusercontent.com` or `.googleapis.com`.
✅ Verification: Ensure the test suite passes, and trying to supply a non-Google URL or non-HTTPS URL in the `GooglePhotoUrl` field results in a model validation error.
