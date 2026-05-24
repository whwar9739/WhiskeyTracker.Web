## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application allowed server-side requests to arbitrary URLs when importing photos from Google. The `GooglePhotoUrl` property was fetched directly via `HttpClient.GetAsync()` without any validation of the URI scheme or host.
**Learning:** Always validate URLs provided by users (even through indirect integrations like photo pickers) before fetching them server-side to prevent SSRF attacks. Ensure the URI is absolute, uses HTTPS, and targets a trusted host.
**Prevention:** Implement strict URI validation using `Uri.TryCreate` and check both the scheme and host against an allowlist before using `HttpClient`.
