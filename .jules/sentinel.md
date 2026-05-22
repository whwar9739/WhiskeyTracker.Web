## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-05-22 - Server-Side Request Forgery (SSRF) in Google Photos Integration
**Vulnerability:** The application was passing a user-provided URL (`GooglePhotoUrl`) directly into `HttpClient.GetAsync()` without validation in `Create.cshtml.cs` and `Edit.cshtml.cs`. This could allow an attacker to make the server perform unauthorized HTTP requests to arbitrary internal or external domains.
**Learning:** Directly using user input in backend HTTP requests is a high-risk SSRF vector, especially when downloading resources or images.
**Prevention:** Always parse and strictly validate user-provided URLs using `Uri.TryCreate`, enforce `https` schemes, and restrict allowed domains to a trusted whitelist (e.g., `*.googleusercontent.com`, `*.googleapis.com`) before making outgoing requests.
