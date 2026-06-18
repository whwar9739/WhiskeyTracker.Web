## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in HttpClient.GetAsync with User-Supplied URLs
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) because it fetched images using `HttpClient.GetAsync(GooglePhotoUrl)` without validating the scheme, host, or disabling auto-redirects. An attacker could supply a malicious URL to fetch internal network resources.
**Learning:** Even when fetching data from what is supposed to be a trusted external service (like Google Photos), you must explicitly validate the user-supplied URL and configure the HTTP client to prevent redirects to malicious locations.
**Prevention:** Always validate URLs using `Uri.TryCreate` with `UriKind.Absolute`, enforce `https`, and check the host against a whitelist of trusted domains (e.g., `*.googleusercontent.com`, `*.googleapis.com`). Furthermore, always disable auto-redirects on `HttpClient` (`AllowAutoRedirect = false`) when handling user-provided URLs.
