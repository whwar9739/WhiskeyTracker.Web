## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF Vulnerability in External URL Fetching
**Vulnerability:** The application fetched external images via `HttpClient.GetAsync()` using a user-provided or client-side supplied URL (`GooglePhotoUrl`) without performing any validation on the destination URL scheme or host. This could allow Server-Side Request Forgery (SSRF) attacks where an attacker could force the server to make requests to internal network resources or malicious external sites.
**Learning:** Directly using user-supplied URLs in server-side HTTP requests is extremely dangerous and can expose internal infrastructure or be used as a proxy for attacks.
**Prevention:** Always strictly validate user-provided URLs before making external requests. Use `Uri.TryCreate` with `UriKind.Absolute` and explicitly enforce the `HTTPS` scheme and whitelist trusted, expected hosts (e.g., `.EndsWith(".googleusercontent.com")` or `.EndsWith(".googleapis.com")`).
