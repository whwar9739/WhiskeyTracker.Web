## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-20 - Server-Side Request Forgery (SSRF) in Image Upload
**Vulnerability:** The application accepted user-provided URLs (`GooglePhotoUrl`) and immediately fetched them using `HttpClient.GetAsync` without validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to force the server to make requests to internal network resources or malicious external sites.
**Learning:** Always validate user-supplied URLs before making server-side requests. Even if the feature is intended for a specific service (like Google Photos), the URL must be strictly validated.
**Prevention:** Use `Uri.TryCreate` with `UriKind.Absolute` to parse the URL. Verify that the scheme is HTTPS and that the host matches an allowlist of expected domains (e.g., `.googleusercontent.com` or `.googleapis.com`) before invoking `HttpClient`.
