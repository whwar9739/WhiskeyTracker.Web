## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-06-09 - Missing SSRF Protection in Google Photos Integration
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) because it blindly passed a user-controlled `GooglePhotoUrl` directly into `HttpClient.GetAsync()` without validating the scheme, host, or URL format. This could allow an attacker to make the server perform HTTP requests to arbitrary internal or external addresses.
**Learning:** Always explicitly validate the URL structure (using `Uri.TryCreate`) and verify that it matches expected parameters (e.g., specific HTTPS domains) before performing HTTP requests based on user input.
**Prevention:** Always validate URLs to ensure they use HTTPS and belong to a trusted domain before invoking `HttpClient.GetAsync()` when fetching external resources.
