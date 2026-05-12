## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-12 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application accepted a user-provided URL (`GooglePhotoUrl`) and immediately issued a server-side `HttpClient.GetAsync()` request to it in both `Create.cshtml.cs` and `Edit.cshtml.cs` without validating the host or scheme. This could allow an attacker to make the server fetch arbitrary internal network resources or external malicious payloads (Server-Side Request Forgery).
**Learning:** Even when a feature is intended to integrate with a specific third-party service (like Google Photos API), user-controlled URLs must strictly enforce domain allowlisting.
**Prevention:** Always validate that external URIs strictly use HTTPS and point only to trusted, expected host domains (e.g., `.googleusercontent.com`, `.googleapis.com`) before making outbound HTTP requests from the server.
