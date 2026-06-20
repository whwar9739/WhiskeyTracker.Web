## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-02-21 - Server-Side Request Forgery (SSRF) in Google Photos Integration
**Vulnerability:** The application fetched Google Photos images from user-provided URLs in `Create.cshtml.cs` and `Edit.cshtml.cs` without validating the host or disabling HTTP redirects. An attacker could provide a malicious URL to make the server scan internal network services or connect to arbitrary external domains.
**Learning:** Even when a feature is intended to integrate with a specific third-party service (like Google Photos), `HttpClient.GetAsync()` will blindly follow user-provided input. `HttpClient` also automatically follows HTTP redirects by default, allowing attackers to bypass simple string checks.
**Prevention:** Strictly validate that the URI uses HTTPS, ensure the host matches trusted domains (e.g., `*.googleusercontent.com`, `*.googleapis.com`), and explicitly set `AllowAutoRedirect = false` on `HttpClientHandler` before issuing the request.
