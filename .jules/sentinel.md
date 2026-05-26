## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-26 - Server-Side Request Forgery (SSRF) in Image Fetching
**Vulnerability:** The application was vulnerable to Server-Side Request Forgery (SSRF) in `Create.cshtml.cs` and `Edit.cshtml.cs` when fetching images from `GooglePhotoUrl`. The provided URL was directly passed to `HttpClient.GetAsync()` without validation.
**Learning:** Never trust user-provided URLs. Unvalidated URLs passed to backend HTTP clients can allow attackers to perform requests to internal networks or unauthenticated internal APIs.
**Prevention:** Always validate URLs against an allowlist of trusted domains and enforce the HTTPS scheme before performing HTTP requests.
