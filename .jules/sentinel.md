## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF via Unrestricted HttpClient.GetAsync
**Vulnerability:** The application fetched images from user-provided URLs (`GooglePhotoUrl` in `Create.cshtml.cs` and `Edit.cshtml.cs`) using `HttpClient.GetAsync()` without any validation. This exposed the application to Server-Side Request Forgery (SSRF), allowing an attacker to coerce the server into making requests to internal or external resources.
**Learning:** Never pass unsanitized user input directly to HTTP clients. Always validate that URIs are safe and meet expected criteria.
**Prevention:** Always use `Uri.TryCreate` to ensure the URI is absolute, enforces HTTPS, and strictly whitelist allowed hosts (e.g., `.googleusercontent.com` or `.googleapis.com`) before invoking `HttpClient.GetAsync()`.
