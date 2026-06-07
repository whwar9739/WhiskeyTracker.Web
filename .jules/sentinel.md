## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-06-07 - Server-Side Request Forgery (SSRF) via Google Photos Integration
**Vulnerability:** The application allowed arbitrary internal/external HTTP requests through the Google Photos URL parameter (`GooglePhotoUrl`) in `WhiskeyTracker.Web/Pages/Whiskies/Create.cshtml.cs` and `WhiskeyTracker.Web/Pages/Whiskies/Edit.cshtml.cs` because it passed unsanitized user input directly to `HttpClient.GetAsync()`.
**Learning:** Cloud-based integrations that fetch external resources can be abused to port scan internal networks or access internal metadata services (e.g., AWS IMDS 169.254.169.254) if URLs are not strictly validated against a known whitelist.
**Prevention:** When fetching resources from user-provided URLs (like the Google Photos integration), strictly validate that the URI uses HTTPS and a trusted host (e.g., '.googleusercontent.com' or '.googleapis.com') using `Uri.TryCreate` with `UriKind.Absolute` before invoking `HttpClient.GetAsync()`.
