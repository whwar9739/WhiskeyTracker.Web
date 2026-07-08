## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application fetches images from user-provided URLs (`GooglePhotoUrl`) via `HttpClient.GetAsync()` without validating the URL scheme, host, or disabling automatic redirects in `Create.cshtml.cs` and `Edit.cshtml.cs`. This allows Server-Side Request Forgery (SSRF), where an attacker could provide URLs to internal services or localhost.
**Learning:** Any user-provided URL fetched by the server must be strictly validated. Relying on the frontend to provide a "Google Photos" URL is insufficient. `HttpClient` follows redirects by default, which can be used to bypass initial URL validation if not disabled.
**Prevention:**
1. Use `Uri.TryCreate` with `UriKind.Absolute` to parse and validate the URL.
2. Ensure the `Scheme` is exactly `UriSchemeHttps`.
3. Validate the `Host` against a strict allowlist (e.g., `*.googleusercontent.com` and `*.googleapis.com`).
4. Disable auto-redirects when instantiating `HttpClient` by using `new HttpClientHandler { AllowAutoRedirect = false }` to prevent redirect-based SSRF.
