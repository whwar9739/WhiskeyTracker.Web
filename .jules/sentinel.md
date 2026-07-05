## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-04-16 - Unmitigated Server-Side Request Forgery (SSRF) in Photo Integration
**Vulnerability:** The application accepted a user-provided URL (`GooglePhotoUrl`) during whiskey creation and editing, fetching the content using an unconstrained `HttpClient.GetAsync()`. This allowed an attacker to supply internal or non-HTTPS URLs, potentially causing the server to make requests to unintended internal services (SSRF).
**Learning:** `HttpClient.GetAsync()` will automatically follow redirects and make requests to any scheme/host provided if not explicitly restricted. User-supplied URLs intended for specific external integrations must be tightly validated against an expected list of hosts.
**Prevention:** To prevent SSRF, use `Uri.TryCreate(url, UriKind.Absolute)` to validate the URL format and scheme (HTTPS only), restrict the `Host` to trusted domains (e.g., `*.googleusercontent.com`, `*.googleapis.com`), and instantiate the `HttpClient` with an `HttpClientHandler` configured with `AllowAutoRedirect = false`.
