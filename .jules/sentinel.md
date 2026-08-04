## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2024-05-18 - Path Traversal Vulnerability in File Uploads
**Vulnerability:** In `Create.cshtml.cs` and `Edit.cshtml.cs` for Whiskies, user-uploaded image filenames were generated using `Guid.NewGuid().ToString() + "_" + ImageUpload.FileName`. Because the original filename was appended as-is, a malicious filename containing path traversal sequences (like `../../../`) could allow writing files outside the designated `images` folder if the operating system or .NET file handling parsed it that way.
**Learning:** Never trust user-provided filenames directly when saving files to the server.
**Prevention:** Always generate a completely new unique filename server-side and only preserve the original file extension using `Path.GetExtension()`.
