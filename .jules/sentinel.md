## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in Photo Upload via External URLs
**Vulnerability:** The application fetched external images via `HttpClient.GetAsync(GooglePhotoUrl)` without validating the scheme, hostname, or restricting automatic redirects. This could allow an attacker to send requests to internal network services or malicious endpoints (SSRF).
**Learning:** Instantiating `HttpClient` without proper URL sanitization and disabling auto-redirects (`AllowAutoRedirect = false`) exposes internal resources when handling user-provided URLs.
**Prevention:** Always validate URLs using `Uri.TryCreate` with `UriKind.Absolute`, enforce `https`, restrict hosts to known safe domains (e.g., `*.googleusercontent.com`), and explicitly pass `new HttpClientHandler { AllowAutoRedirect = false }` to `HttpClient`.
