## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2026-06-26 - File Upload SSRF and Path Traversal Mitigation
**Vulnerability:** The application was vulnerable to SSRF in Google Photo uploads (no URL validation, auto-redirect enabled) and Path Traversal in direct image uploads (user-provided FileName appended to GUID).
**Learning:** File upload logic requires defense-in-depth: untrusted URLs must be strictly validated against an allowlist before HttpClient fetch, and file names provided by external sources must never be used directly to construct local file paths.
**Prevention:** Always validate external URLs using Uri.TryCreate and strict host matching. When handling file uploads, generate a fresh unique filename (like a GUID) and only preserve the original file extension using Path.GetExtension().
