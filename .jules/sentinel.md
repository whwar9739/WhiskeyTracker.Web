## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-06 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) during the Google Photos import process (`Create.cshtml.cs` and `Edit.cshtml.cs`). The `GooglePhotoUrl` parameter was passed directly to `HttpClient.GetAsync()` without any validation, allowing an attacker to force the server to make requests to internal network resources or unauthorized external domains.
**Learning:** Never trust user-provided URLs or assume they only point to intended external services, even if they are populated by a trusted client-side script. The backend must always perform its own validation.
**Prevention:** Always strictly validate user-provided URIs before passing them to an HTTP client. Ensure the URI is absolute, uses the HTTPS scheme, and targets a trusted, whitelisted host (e.g., `.googleusercontent.com` or `.googleapis.com`).
