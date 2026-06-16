## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application fetched Google Photos images using a user-provided `GooglePhotoUrl` without validating the host, scheme, or restricting HTTP redirects. This could allow an attacker to make the server fetch resources from internal networks or arbitrary endpoints (SSRF).
**Learning:** Even when integrating with third-party services like Google Photos, user-provided URLs must be strictly validated to ensure they point to expected domains and use secure protocols. Default `HttpClient` behavior follows redirects, which can bypass initial URL validation if the remote server responds with a redirect to an internal IP.
**Prevention:** To prevent SSRF when fetching resources from user-provided URLs, strictly validate that the URI uses HTTPS and a trusted host (e.g., `*.googleusercontent.com`, `*.googleapis.com`). Additionally, disable auto-redirects on the `HttpClient` (`new HttpClientHandler { AllowAutoRedirect = false }`) before invoking `GetAsync()`.
