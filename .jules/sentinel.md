## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-18 - SSRF Vulnerability in Google Photos Integration
**Vulnerability:** The application fetches image resources based on user-provided URLs in `Create.cshtml.cs` and `Edit.cshtml.cs` using `HttpClient.GetAsync()`. The URL was not validated, allowing potential Server-Side Request Forgery (SSRF) attacks where a user could provide internal network URLs or malicious external URLs to be fetched by the server.
**Learning:** Never trust user-provided URLs when making outbound HTTP requests from the server.
**Prevention:** Always use `Uri.TryCreate` with `UriKind.Absolute` to strictly validate the user-provided URL. Ensure the scheme is `https` and restrict the host to specific trusted domains (e.g., `.googleusercontent.com` or `.googleapis.com`) before invoking `HttpClient.GetAsync()`.
