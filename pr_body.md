🚨 Severity: CRITICAL
💡 Vulnerability: Server-Side Request Forgery (SSRF) when fetching user-provided Google Photo URLs in `Create.cshtml.cs` and `Edit.cshtml.cs`.
🎯 Impact: Attackers could send unauthorized requests to internal network services or malicious endpoints via the application's backend.
🔧 Fix: Validated URLs using `Uri.TryCreate` with `UriKind.Absolute`, enforced `https`, restricted valid hosts to `.googleusercontent.com` and `.googleapis.com`, and disabled auto-redirects on the `HttpClient` to prevent bypass.
✅ Verification: Ran the full test suite (`dotnet test`), verifying compilation and functionality without regressions.
