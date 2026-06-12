## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2026-06-12 - SSRF and Path Traversal in File Uploads
**Vulnerability:** Google Photos integrations in `WhiskeyTracker.Web/Pages/Whiskies/Create.cshtml.cs` and `Edit.cshtml.cs` lacked URL validation, enabling Server-Side Request Forgery (SSRF) and potential redirect attacks. File uploads combined user-provided filenames with GUIDs, enabling potential path traversal or conflicting naming via `+ "_" + ImageUpload.FileName`.
**Learning:** External URLs provided via client input must be strictly validated for trusted hosts and HTTPS, and `HttpClient` auto-redirects disabled, to prevent SSRF. For file uploads, user-provided filenames should be fully discarded in favor of server-generated unique identifiers, preserving only the extension.
**Prevention:** Use `Uri.TryCreate` with absolute paths, validate `.Host` against an allowlist, use `new HttpClientHandler { AllowAutoRedirect = false }`, and use `Path.GetExtension(filename)` rather than appending the full user-provided filename.
