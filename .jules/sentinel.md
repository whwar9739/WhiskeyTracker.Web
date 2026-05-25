## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-25 - Prevent SSRF in Google Photos Integration
**Vulnerability:** The application was using user-provided URLs (`GooglePhotoUrl`) directly in `HttpClient.GetAsync()` during whiskey creation and editing. This is a Server-Side Request Forgery (SSRF) vulnerability, allowing an attacker to force the server to make requests to arbitrary internal or external URLs.
**Learning:** External integrations that fetch resources based on user input must strictly validate the URI format, scheme, and host before executing the request.
**Prevention:** Always use `Uri.TryCreate` with `UriKind.Absolute` to validate the URL. Explicitly check that the scheme is HTTPS (`Uri.UriSchemeHttps`) and the host belongs to a trusted list (e.g., `.googleusercontent.com`, `.googleapis.com`).
