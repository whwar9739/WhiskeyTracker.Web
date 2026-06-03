## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - SSRF and Path Traversal in File Uploads
**Vulnerability:** Server-Side Request Forgery (SSRF) and Path Traversal in Google Photos URL download and file uploads (`Create.cshtml.cs` and `Edit.cshtml.cs`). The `HttpClient.GetAsync` allowed arbitrary URLs and `ImageUpload.FileName` could be injected to traverse directories.
**Learning:** External URLs supplied by users or third-party APIs (like the Google Photos picker) must be validated before being fetched by the server to prevent the server from being used as a proxy to internal resources or executing malicious requests. Uploaded filenames should never be trusted as part of the stored path.
**Prevention:** Strictly validate external URIs using `Uri.TryCreate` with `UriKind.Absolute` and check for HTTPS scheme and trusted hosts (e.g., `.googleusercontent.com`, `.googleapis.com`) before fetching. Generate unique, safe filenames (e.g., using `Guid.NewGuid()`) and preserve only the extension (`Path.GetExtension()`) from original uploads.
