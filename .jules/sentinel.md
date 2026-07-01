## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-07-01 - SSRF Vulnerability in Google Photo URL Fetching
**Vulnerability:** The application fetches image content directly from user-provided URLs in `GooglePhotoUrl` without validation, allowing a Server-Side Request Forgery (SSRF) attack.
**Learning:** `HttpClient.GetAsync` blindly follows redirects and fetches URLs on any host by default.
**Prevention:** Always validate user-provided URIs to enforce HTTPS, restrict to trusted hosts (like `*.googleusercontent.com`), and disable auto-redirects (`AllowAutoRedirect = false`) on the `HttpClientHandler`.
