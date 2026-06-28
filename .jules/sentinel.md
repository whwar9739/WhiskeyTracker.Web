## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in Google Photos Integration
**Vulnerability:** The Google Photos integration used `HttpClient.GetAsync` on a user-provided URL (`GooglePhotoUrl`) in `Create.cshtml.cs` and `Edit.cshtml.cs` without validating the host, scheme, or disabling automatic redirects, which exposed the server to Server-Side Request Forgery (SSRF).
**Learning:** External user input used directly in outbound HTTP requests must always be strictly validated and sanitized to prevent SSRF attacks.
**Prevention:** Always validate URLs using `Uri.TryCreate` to enforce HTTPS and trusted hosts (e.g., `*.googleusercontent.com`), and explicitly disable auto-redirects on `HttpClientHandler` (`AllowAutoRedirect = false`).
