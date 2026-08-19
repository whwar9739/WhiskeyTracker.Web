## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2024-08-19 - SSRF (Redirect Bypass) and Path Traversal in Image Uploads
**Vulnerability:** The application was vulnerable to Path Traversal during file uploads because it used the original filename concatenated with a GUID, which could contain path characters. Additionally, while the `GooglePhotoUrl` was validated for SSRF, the `HttpClient` default behavior followed auto-redirects, meaning an attacker could provide a trusted Google domain URL that redirects to an internal endpoint, bypassing the SSRF mitigation.
**Learning:** Using original user-provided filenames is inherently unsafe, and SSRF mitigations must account for HTTP redirects when using `HttpClient`.
**Prevention:** Always extract only the extension using `Path.GetExtension()` when generating unique filenames. When mitigating SSRF, strictly enforce `AllowAutoRedirect = false` on the `HttpClientHandler` to prevent redirect-based bypasses.
