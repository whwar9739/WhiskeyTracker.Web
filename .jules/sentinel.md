## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2026-05-31 - Missing SSRF Validation on Image Uploads from External URLs
**Vulnerability:** The application was executing `HttpClient.GetAsync()` on user-provided Google Photos URLs without validation, opening up a Server-Side Request Forgery (SSRF) risk where a user could trick the server into downloading malicious or internal files.
**Learning:** `HttpClient.GetAsync()` blindly trusts whatever URL it is fed, and any time a user provides a URL to fetch a resource, it must be explicitly validated.
**Prevention:** Before performing an outbound HTTP request using a user-provided URL, parse the URI using `Uri.TryCreate` with `UriKind.Absolute`, enforce the `https` scheme, and explicitly whitelist acceptable hosts (e.g., `.googleusercontent.com` or `.googleapis.com`).
