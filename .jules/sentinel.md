## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2024-05-18 - Path Traversal and SSRF Mitigation in Image Uploads
**Vulnerability:** The application allowed file uploads where the original file name was appended directly to a GUID (e.g., `Guid.NewGuid().ToString() + "_" + ImageUpload.FileName`). This could allow path traversal attacks if the original file name contained characters like `../`. Additionally, SSRF was mitigated by disabling auto-redirects on the `HttpClient` used to fetch external images.
**Learning:** File names provided by users cannot be trusted. Even when prepending a GUID, using the original filename can lead to path traversal vulnerabilities. Furthermore, auto-redirects in `HttpClient` should be disabled when fetching user-provided URLs to prevent SSRF bypasses via redirects.
**Prevention:** Generate unique filenames using `Guid.NewGuid().ToString()` and only preserve the file extension from the user-provided filename using `Path.GetExtension()`. For `HttpClient`, initialize it with an `HttpClientHandler` that has `AllowAutoRedirect` set to `false`.
