## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.
## 2025-02-28 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** Server-Side Request Forgery (SSRF) allowed the server to fetch images from any URL provided in `GooglePhotoUrl` without validation, posing a risk of internal network scanning or accessing unintended internal services.
**Learning:** External URLs provided via client requests and fetched by `HttpClient` must be strictly validated. The scheme and domain should be constrained, and `HttpClient` auto-redirects must be disabled to prevent attackers from bypassing domain restrictions via redirect responses.
**Prevention:** Implement an allowlist approach for expected external hosts (e.g., `*.googleusercontent.com` and `*.googleapis.com`), enforce HTTPS using `Uri.TryCreate`, and use `new HttpClientHandler { AllowAutoRedirect = false }` when instantiating the `HttpClient`.
