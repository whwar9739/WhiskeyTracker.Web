## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2026-05-23 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application allowed server-side request forgery (SSRF) by directly passing unvalidated, user-provided URLs (`GooglePhotoUrl`) to `HttpClient.GetAsync` during image uploads in `Create.cshtml.cs` and `Edit.cshtml.cs`.
**Learning:** Blindly trusting URLs sent from the client (e.g., hidden form fields) can allow an attacker to force the server to make requests to internal network resources or malicious external sites.
**Prevention:** When fetching resources from user-provided URLs, strictly validate that the URI uses HTTPS and a trusted host (e.g., `.googleusercontent.com` or `.googleapis.com`) using `Uri.TryCreate` with `UriKind.Absolute` before invoking `HttpClient.GetAsync()`.
