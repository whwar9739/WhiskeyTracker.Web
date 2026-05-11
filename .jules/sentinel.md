## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-05-27 - Server-Side Request Forgery and Path Traversal in Image Uploads
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) and Path Traversal during whiskey image uploads. It retrieved images from user-supplied URLs without verifying the schema or host, and it constructed file paths using the original user-supplied filename (`ImageUpload.FileName`).
**Learning:** Never trust user input when constructing file paths or when making server-side network requests. User-supplied filenames can contain directory traversal sequences (like `../`), and user-supplied URLs can point to internal networks or sensitive external endpoints.
**Prevention:** Always validate that URLs use HTTPS and are restricted to trusted domains (e.g., `.googleusercontent.com` or `.googleapis.com`). For file uploads, discard the original filename, generate a new unique identifier (like a GUID), and safely extract only the file extension.
