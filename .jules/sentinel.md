## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application fetched Google Photos images from user-supplied URLs (`GooglePhotoUrl`) without any URL validation or restriction on auto-redirects. An attacker could exploit this Server-Side Request Forgery (SSRF) vulnerability to make the server fetch internal network resources or unintended external endpoints by supplying a malicious URL or a URL that redirects to internal resources.
**Learning:** Instantiating `HttpClient` without custom handlers defaults to following redirects. When dealing with user-supplied URLs intended for specific trusted services (like Google Photos), the URL must be strictly validated.
**Prevention:** Validate user-supplied URLs to ensure they use the `https` scheme and resolve to expected trusted hosts (e.g., `*.googleusercontent.com` or `*.googleapis.com`). Additionally, configure the `HttpClient` using `new HttpClientHandler { AllowAutoRedirect = false }` to prevent attackers from using open redirects to bypass host validation.
