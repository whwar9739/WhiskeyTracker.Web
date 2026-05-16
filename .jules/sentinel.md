## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-16 - Prevent SSRF and Path Traversal in Image Handling
**Vulnerability:** GooglePhotoUrl was fetched via HttpClient without validation, allowing Server-Side Request Forgery (SSRF). Additionally, ImageUpload.FileName was appended directly to the upload path, allowing Path Traversal.
**Learning:** External URLs provided by users must always be validated to ensure they are absolute, use HTTPS, and point to trusted domains before fetching content. User-uploaded filenames cannot be trusted and should be sanitized or stripped down to just the extension.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` and check `uri.Scheme` and `uri.Host` for SSRF protection. For file uploads, generate a unique filename (e.g., GUID) and append only the sanitized extension using `Path.GetExtension()`.
