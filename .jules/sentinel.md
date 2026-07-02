## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) when processing Google Photos image URLs in `Create.cshtml.cs` and `Edit.cshtml.cs`. The user-provided URL was directly passed to `HttpClient.GetAsync` without host validation or disabling automatic redirects, potentially allowing attackers to scan internal networks or access sensitive internal services.
**Learning:** Never trust user-provided URLs when making server-side HTTP requests, even if they appear to originate from a known feature (like Google Photos). Automatic HTTP redirects can bypass initial URL validation checks if not explicitly disabled.
**Prevention:** Strictly validate that the URI uses HTTPS and a trusted host (e.g., `*.googleusercontent.com` or `*.googleapis.com`) using `Uri.TryCreate` with `UriKind.Absolute`. Disable automatic redirects on the `HttpClient` (`new HttpClientHandler { AllowAutoRedirect = false }`) to ensure the request only goes to the validated host.
