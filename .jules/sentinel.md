## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2024-05-18 - Path Traversal via Image Upload
**Vulnerability:** The image upload logic concatenated the user-supplied `ImageUpload.FileName` directly into the final `uniqueFileName` (`Guid.NewGuid().ToString() + "_" + ImageUpload.FileName`). An attacker could supply a filename with directory traversal characters (e.g., `../../malicious.exe`) potentially leading to arbitrary file write.
**Learning:** Never trust the user-supplied filename when saving uploaded files to disk.
**Prevention:** Generate a fully unique and secure filename on the server (like a `Guid`), and extract only the extension from the user's file (using `Path.GetExtension()`) to append it securely.

## 2024-05-18 - SSRF Redirect Bypass
**Vulnerability:** Even when URL inputs (like `GooglePhotoUrl`) are restricted by an allowlist, if the `HttpClient` allows automatic redirection, an attacker can provide an allowed URL that redirects to an internal endpoint (e.g., `http://169.254.169.254/latest/meta-data/`).
**Learning:** A URL allowlist check is insufficient against SSRF if the HTTP client automatically follows redirects.
**Prevention:** When making outbound HTTP requests to user-supplied URLs, disable auto-redirects by setting `AllowAutoRedirect = false` on the `HttpClientHandler` to ensure the request is not routed elsewhere.
