## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-06-11 - SSRF via Google Photos Integration
**Vulnerability:** The application was vulnerable to SSRF because it used a raw `HttpClient` to fetch user-provided URLs in `GooglePhotoUrl` without URL scheme or domain validation, and allowing auto-redirects.
**Learning:** External image fetch features must validate that URLs are not only syntactically correct but restricted to trusted schemes (HTTPS) and specific trusted host domains (e.g. `googleusercontent.com`).
**Prevention:** Always use `Uri.TryCreate` with `UriKind.Absolute` to validate the schema, explicitly check hostnames against an allowlist, and use `HttpClientHandler { AllowAutoRedirect = false }` to prevent redirect chaining to internal network resources.
