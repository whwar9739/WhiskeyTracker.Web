## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - [Fix SSRF vulnerability in image upload]
**Vulnerability:** Unvalidated user-supplied URLs passed directly to `HttpClient.GetAsync()` could allow Server-Side Request Forgery (SSRF) and access to internal network resources.
**Learning:** By directly calling `HttpClient.GetAsync()` on unvalidated URLs such as `GooglePhotoUrl`, an attacker could provide URLs to internal services and use the server as a proxy, bypassing network firewalls. Furthermore, `HttpClient` by default follows redirects, allowing external servers to redirect the application to local internal addresses.
**Prevention:** To prevent SSRF when fetching resources from user-provided URLs, strictly validate that the URI uses HTTPS and a trusted host (e.g., via `Uri.TryCreate` with `UriKind.Absolute`) and disable auto-redirects on the `HttpClient` (`new HttpClientHandler { AllowAutoRedirect = false }`) before invoking `GetAsync()`. In this specific case, only `.googleusercontent.com` and `.googleapis.com` are permitted.
