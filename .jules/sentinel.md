## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - Missing Input Validation on Image Upload Integration
**Vulnerability:** Server-Side Request Forgery (SSRF) allowed external, attacker-controlled URLs to be fetched via `HttpClient.GetAsync(GooglePhotoUrl)` without proper host or scheme validation in `Create.cshtml.cs` and `Edit.cshtml.cs`.
**Learning:** Automatically trusting user-provided URLs in HTTP clients opens the door to fetching local or restricted network resources, or arbitrary external files.
**Prevention:** Strictly validate external URIs with `Uri.TryCreate` specifying `UriKind.Absolute`, requiring HTTPS, and restricting hosts to known trusted domains (e.g., `.googleusercontent.com`, `.googleapis.com`).
