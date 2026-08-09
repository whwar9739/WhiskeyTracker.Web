## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application allowed users to provide a `GooglePhotoUrl` that was passed directly to `HttpClient.GetAsync()` without any validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server send arbitrary HTTP requests to internal or external systems.
**Learning:** External URLs provided by users must be strictly validated before being used in server-side HTTP requests to prevent SSRF vulnerabilities.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL format, enforce HTTPS (`Uri.UriSchemeHttps`), and restrict the host to an allowlist of trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).

## 2024-05-18 - Server-Side Request Forgery (SSRF) in Image Upload via Redirects
**Vulnerability:** The application mitigated SSRF by checking the host of a user-provided `GooglePhotoUrl` before passing it to `HttpClient.GetAsync()`. However, `HttpClient` automatically follows HTTP redirects by default. An attacker could supply a URL pointing to an allowed domain (e.g., via an open redirect vulnerability on `googleusercontent.com`) that then redirects to an internal server or IP address, bypassing the initial host validation.
**Learning:** Checking the initial URL is insufficient if the HTTP client automatically follows redirects. Attackers can use redirects to reach forbidden internal endpoints.
**Prevention:** Always disable automatic redirects when fetching data from user-provided URLs (`new HttpClientHandler { AllowAutoRedirect = false }`) so the client does not inadvertently make requests to unvalidated destinations. Ensure that any redirects are explicitly handled and validated if necessary.
