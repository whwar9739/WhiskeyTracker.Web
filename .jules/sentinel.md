## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-15 - SSRF Vulnerability in Google Photo URL Fetch
**Vulnerability:** The `Create.cshtml.cs` and `Edit.cshtml.cs` files fetched user-provided Google Photo URLs using `HttpClient.GetAsync` without any validation or sanitization, creating a Server-Side Request Forgery (SSRF) vulnerability. An attacker could potentially pass internal network addresses or arbitrary external URLs.
**Learning:** Even when integrating with third-party APIs (like Google Photos), any URL provided by the client must be rigorously validated before the server initiates a request to it.
**Prevention:** Always validate user-provided URLs using `Uri.TryCreate` with `UriKind.Absolute`, enforce a strict scheme allowlist (e.g., `Uri.UriSchemeHttps`), explicitly check the `Host` property against a trusted allowlist (e.g., `*.googleusercontent.com` and `*.googleapis.com`), and disable automatic redirects (`AllowAutoRedirect = false`) on the `HttpClientHandler` to prevent the server from being coerced into following redirects to arbitrary locations.
