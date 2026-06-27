## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in Google Photos Integration
**Vulnerability:** The application was fetching images from user-provided URLs in `Create.cshtml.cs` and `Edit.cshtml.cs` without validating the host or scheme, allowing Server-Side Request Forgery (SSRF) attacks against internal services.
**Learning:** `HttpClient.GetAsync` will happily fetch from local/internal IPs (e.g., `127.0.0.1`, `169.254.169.254`) or use non-HTTPS schemes if not explicitly restricted. Furthermore, `HttpClient` follows redirects by default, which can bypass initial URL validation if a malicious external server redirects to an internal one.
**Prevention:** Always validate user-provided URLs (using `Uri.TryCreate` with `UriKind.Absolute`), enforce HTTPS, restrict the host to a trusted allowlist (e.g., `*.googleusercontent.com`), and explicitly disable auto-redirects on `HttpClient` (`AllowAutoRedirect = false`).
