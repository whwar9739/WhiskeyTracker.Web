## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-02-28 - SSRF and Path Traversal in File Uploads
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) by accepting an unvalidated `GooglePhotoUrl` and making outbound HTTP GET requests with a sensitive Bearer token attached. Additionally, the file upload mechanism was vulnerable to Path Traversal, as user-controlled filenames (`ImageUpload.FileName`) were concatenated directly to a GUID, potentially allowing files to be written outside the intended directory.
**Learning:** Never trust external URLs or user-provided filenames. Outbound requests to user-provided URLs can be manipulated to scan internal networks or leak tokens. User-provided filenames can contain malicious sequences like `../../../`.
**Prevention:** Strictly validate external URLs using `Uri.TryCreate` with `UriKind.Absolute` to ensure HTTPS and a trusted host. For file uploads, always extract only the file extension using `Path.GetExtension()` and append it to a randomly generated GUID.
