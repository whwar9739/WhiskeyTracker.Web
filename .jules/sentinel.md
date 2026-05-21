## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2026-05-21 - Server-Side Request Forgery (SSRF) in Google Photos Integration
**Vulnerability:** The `GooglePhotoUrl` parameter in both `Create.cshtml.cs` and `Edit.cshtml.cs` was passed directly to `HttpClient.GetAsync()` without any validation. An attacker could provide a malicious URL pointing to internal network resources, potentially leading to Server-Side Request Forgery (SSRF).
**Learning:** Always validate user-provided URLs before making outbound HTTP requests, especially when fetching resources from external services.
**Prevention:** Strictly validate that the URI uses HTTPS and a trusted host (e.g., `.googleusercontent.com` or `.googleapis.com`) using `Uri.TryCreate` with `UriKind.Absolute` before invoking `HttpClient.GetAsync()`.
