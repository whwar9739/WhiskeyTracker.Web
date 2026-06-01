## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application accepted user-provided URLs (`GooglePhotoUrl`) in the Create and Edit whiskey pages and blindly fetched them using `HttpClient.GetAsync()`. This could allow an attacker to make the server issue requests to internal network resources or arbitrary external domains (Server-Side Request Forgery).
**Learning:** External URLs provided by users must always be strictly validated against an allowlist of trusted domains and schemes before being processed by the server.
**Prevention:** Added URI validation using `Uri.TryCreate` with `UriKind.Absolute` to ensure the URL scheme is HTTPS and the host ends with a trusted Google domain (`.googleusercontent.com` or `.googleapis.com`).
