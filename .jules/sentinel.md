## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in Photo Upload via External URL
**Vulnerability:** The application fetches image bytes from a user-provided `GooglePhotoUrl` without validating the host or scheme, allowing Server-Side Request Forgery (SSRF) and potential local network port scanning or accessing internal meta-data services.
**Learning:** Automatically trusting and blindly retrieving content from user-supplied URLs using `HttpClient` is dangerous as it allows attackers to forge requests from the server's context.
**Prevention:** Implement strict URI validation (`Uri.TryCreate` with `UriKind.Absolute`), enforce the `HTTPS` scheme, strictly whitelist trusted hostnames (e.g., `*.googleusercontent.com`, `*.googleapis.com`), and explicitly disable automatic redirects using `new HttpClientHandler { AllowAutoRedirect = false }` when fetching resources from external URLs.
