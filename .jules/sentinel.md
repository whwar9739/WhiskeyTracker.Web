## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application fetches image URLs directly from user input (GooglePhotoUrl) via `HttpClient.GetAsync()` without validating the scheme or the host, which introduces a potential Server-Side Request Forgery (SSRF) vulnerability.
**Learning:** External URL fetching must strictly validate the destination URL to prevent the server from making requests to internal infrastructure or untrusted domains.
**Prevention:** Always parse untrusted URIs securely and enforce a strict allowlist of both URI scheme (HTTPS only) and host/domain before making external HTTP requests.
