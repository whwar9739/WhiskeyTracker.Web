## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-15 - SSRF and Path Traversal in Whiskey Uploads
**Vulnerability:** The application was fetching images from a user-provided URL (`GooglePhotoUrl`) without validating the scheme or host, leading to a Server-Side Request Forgery (SSRF) vulnerability. Additionally, file uploads used the original `ImageUpload.FileName` directly in the generated filename, presenting a Path Traversal vulnerability risk.
**Learning:** Always explicitly validate the scheme (HTTPS) and allowed hosts for user-provided URLs before making outward HTTP requests. For file uploads, generate a strictly controlled unique filename (e.g., via GUID) and only append the safely extracted extension (`Path.GetExtension()`).
**Prevention:** Use `Uri.TryCreate` with absolute URI kind to restrict URLs to expected hostnames (like `.googleusercontent.com`). Never trust user-provided filenames; always generate new random identifiers on the server.
