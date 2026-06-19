## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-02-23 - Missing SSRF Mitigations in Image Fetching
**Vulnerability:** Fetching user-provided image URLs (`GooglePhotoUrl`) without validation or redirect prevention allowed for potential Server-Side Request Forgery (SSRF). An attacker could provide a malicious URL to scan internal network infrastructure or access sensitive internal endpoints.
**Learning:** Even when intended for a specific service (e.g., Google Photos), `HttpClient.GetAsync` will gladly resolve and fetch from any internal or external host provided if not explicitly restricted.
**Prevention:** Always validate user-provided URLs using `Uri.TryCreate` with `UriKind.Absolute`, enforce HTTPS (`Uri.UriSchemeHttps`), explicitly whitelist allowed hosts (e.g., `endsWith(".googleusercontent.com")`), and disable HTTP auto-redirects on the `HttpClientHandler` (`AllowAutoRedirect = false`).
