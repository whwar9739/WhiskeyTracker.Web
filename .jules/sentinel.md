## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-04 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The `Create.cshtml.cs` and `Edit.cshtml.cs` page models fetched images from a user-provided `GooglePhotoUrl` using `HttpClient.GetAsync()` without validating the URL. This allowed an attacker to potentially supply arbitrary URLs (e.g., internal network addresses or `file://` URIs) resulting in a Server-Side Request Forgery (SSRF) vulnerability.
**Learning:** Always validate user-supplied URLs before making outbound HTTP requests on behalf of the user. Ensure the URL scheme is HTTPS and restrict requests to known, trusted hosts.
**Prevention:** Implement strict URI validation using `Uri.TryCreate` to ensure the URL is absolute, uses the HTTPS scheme, and its host ends with an approved domain (e.g., `.googleusercontent.com` or `.googleapis.com`).
