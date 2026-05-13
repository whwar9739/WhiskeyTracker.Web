## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-24 - SSRF in Google Photos Integration
**Vulnerability:** Server-Side Request Forgery (SSRF) was possible because `HttpClient.GetAsync()` in `Create.cshtml.cs` and `Edit.cshtml.cs` fetched resources from user-provided URLs (`GooglePhotoUrl`) without validating the host or scheme. An attacker could potentially supply internal network addresses or malicious external URLs.
**Learning:** Whenever fetching remote resources based on user input, strictly validate the URL scheme (e.g., HTTPS) and allowlist the target hosts. The codebase memory specifically instructs to trust `.googleusercontent.com` and `.googleapis.com` for this integration.
**Prevention:** Always parse the URL using `Uri.TryCreate()` with `UriKind.Absolute` and check `uriResult.Scheme` and `uriResult.Host` before making outbound HTTP requests.
