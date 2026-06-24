## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in Google Photos Integration
**Vulnerability:** Server-Side Request Forgery (SSRF). The application used a user-provided URL (`GooglePhotoUrl`) directly in an `HttpClient.GetAsync` call without validating the scheme, host, or disabling auto-redirects. This allowed arbitrary requests to internal or external endpoints.
**Learning:** External integrations relying on user-provided inputs must strictly validate the input to ensure it points to the intended service and prevent arbitrary request forwarding.
**Prevention:** Validate user-provided URLs using `Uri.TryCreate` to enforce HTTPS and trusted hosts (e.g., `*.googleusercontent.com`, `*.googleapis.com`), and explicitly disable auto-redirects (`HttpClientHandler { AllowAutoRedirect = false }`) to stop redirection to internal/untrusted endpoints.
