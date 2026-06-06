## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-02-23 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application was directly passing a user-provided `GooglePhotoUrl` to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This allowed potential Server-Side Request Forgery (SSRF).
**Learning:** External integrations that accept URLs must strictly validate the scheme and host to ensure they only connect to intended, trusted endpoints.
**Prevention:** Always use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL structure, and explicitly check that the scheme is HTTPS and the host matches an allowlist of trusted domains before making outbound HTTP requests.
