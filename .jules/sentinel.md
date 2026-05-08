## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-08 - SSRF Vulnerability via Untrusted Image URL
**Vulnerability:** A Server-Side Request Forgery (SSRF) vulnerability existed where user-provided Google Photo URLs (`GooglePhotoUrl`) were fetched via `HttpClient.GetAsync()` without validation. This could allow attackers to scan internal networks or access sensitive metadata services from the server.
**Learning:** Even expected features like pulling an image from an external service must rigorously validate the requested URI's scheme and host to prevent SSRF attacks.
**Prevention:** Always parse untrusted URLs using `Uri.TryCreate` and strictly validate that the URI uses HTTPS and the host matches an expected allowlist (e.g., `.googleusercontent.com` or `.googleapis.com`) before invoking any HTTP requests.
