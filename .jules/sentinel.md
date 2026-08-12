## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2024-05-20 - SSRF Bypass via Open Redirects in Image Upload
**Vulnerability:** Even though user-provided URLs were restricted to trusted Google domains (`.googleusercontent.com`, `.googleapis.com`), `HttpClient.GetAsync()` was configured with its default behavior to automatically follow HTTP redirects. An attacker could exploit an open redirect vulnerability on a trusted Google domain to bounce the request to an arbitrary internal or external endpoint, bypassing the initial URL validation.
**Learning:** Validating the initial URI is insufficient if the HTTP client automatically follows redirects to unvalidated destinations.
**Prevention:** Always configure `HttpClient` with an `HttpClientHandler` setting `AllowAutoRedirect = false` when fetching resources from user-provided URLs, even if the initial domain is validated.
