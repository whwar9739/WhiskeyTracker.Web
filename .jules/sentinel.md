## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-17 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) because it blindly accepted and fetched user-provided URLs (`GooglePhotoUrl`) in `Create.cshtml.cs` and `Edit.cshtml.cs` using `HttpClient.GetAsync()`. A malicious user could provide internal network URLs or malicious external URLs.
**Learning:** Never trust user-provided URLs for server-side fetching. Always validate the scheme, host, and path before initiating an outbound request.
**Prevention:** Implement strict URI validation using `Uri.TryCreate` with `UriKind.Absolute` and enforce an allowlist of trusted hosts (e.g., `.googleusercontent.com`, `.googleapis.com`) and require HTTPS.
