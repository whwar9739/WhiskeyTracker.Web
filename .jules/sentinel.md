## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2026-08-07 - SSRF Bypass via HttpClient Auto-Redirects
**Vulnerability:** Despite validating the scheme and host of `GooglePhotoUrl` before use, the `HttpClient` default configuration allowed auto-redirects. An attacker could supply a URL to a trusted domain that redirects to an internal or unauthorized resource, bypassing the initial validation and leading to Server-Side Request Forgery (SSRF).
**Learning:** URL validation at the entry point is insufficient if the HTTP client automatically follows redirects to potentially unvalidated or untrusted destinations.
**Prevention:** Always explicitly disable auto-redirects (`new HttpClientHandler { AllowAutoRedirect = false }`) when using `HttpClient` to fetch user-provided URLs.
