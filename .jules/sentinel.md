## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-07-01 - SSRF Prevention in Google Photos Integration
**Vulnerability:** Server-Side Request Forgery (SSRF) was possible via the `GooglePhotoUrl` parameter during Whiskey creation and editing. The application used an unsanitized user-provided URL in an `HttpClient.GetAsync()` call and allowed automatic redirects, enabling attackers to make arbitrary requests to internal network resources or malicious external endpoints on behalf of the server.
**Learning:** External integrations relying on user-provided URLs require strict validation and constrained HTTP client configuration to prevent SSRF, even if the primary intent is simply downloading an image.
**Prevention:**
1. Validate the URL structure (absolute URI, HTTPS scheme).
2. Enforce an allowlist of trusted hosts (e.g., `*.googleusercontent.com`, `*.googleapis.com`).
3. Disable automatic redirects on the `HttpClient` (`AllowAutoRedirect = false`) to prevent attackers from using an allowed host that redirects to a prohibited internal resource.
