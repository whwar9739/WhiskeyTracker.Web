## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-02-20 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) when fetching images via the Google Photos integration (`GooglePhotoUrl`). User-supplied URLs were passed directly to `HttpClient.GetAsync()` without validation, allowing a malicious user to force the server to make requests to arbitrary internal or external addresses.
**Learning:** External URL parameters must always be strictly validated before being used in server-side HTTP requests, even when they are part of a seemingly secure feature.
**Prevention:** Always validate external URIs using `Uri.TryCreate` with `UriKind.Absolute` and strictly allowlist expected schemes (e.g., HTTPS) and domains (e.g., `.googleusercontent.com` or `.googleapis.com`).
