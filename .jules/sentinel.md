## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.
## 2024-05-05 - Missing URI Validation in Image Fetching (SSRF)
**Vulnerability:** The application accepted user-provided URLs for fetching images (`GooglePhotoUrl`) without validating that the URI uses HTTPS or points to a trusted domain, potentially allowing Server-Side Request Forgery (SSRF) attacks.
**Learning:** Any endpoint that fetches resources from a user-provided URL must strictly validate the URI scheme and host before making the HTTP request to prevent the server from accessing internal or malicious external resources.
**Prevention:** Implement strict `Uri.TryCreate` validation to ensure the URL uses `Uri.UriSchemeHttps` and the `Uri.Host` ends with a trusted domain (e.g., `.googleusercontent.com` or `.googleapis.com`) before invoking `HttpClient.GetAsync()`.
