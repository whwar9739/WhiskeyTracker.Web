## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.
## 2026-06-23 - [SSRF in Image URL Fetching]
**Vulnerability:** User-provided URLs for fetching images (e.g., Google Photos integration) were requested directly via HttpClient without validation, allowing Server-Side Request Forgery (SSRF) to internal services.
**Learning:** External URLs provided by users must be strictly validated for the HTTPS scheme and trusted domain names. Furthermore, auto-redirects on HttpClient should be disabled to prevent redirection to internal/malicious endpoints after initial validation passes.
**Prevention:** Use `Uri.TryCreate` to enforce `UriSchemeHttps` and a whitelist of trusted domains (e.g., `.googleusercontent.com`). Disable auto-redirects using `new HttpClientHandler { AllowAutoRedirect = false }`.
