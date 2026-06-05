## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-16 - Unvalidated External URLs Leading to SSRF
**Vulnerability:** The Google Photos integration allowed arbitrary URLs supplied by users to be fetched using `HttpClient.GetAsync()` without any validation. This meant an attacker could supply a URL pointing to internal network services or metadata endpoints, potentially leading to Server-Side Request Forgery (SSRF).
**Learning:** Any user-supplied URL that the server will fetch must be strictly validated. Checking only the token is insufficient if the URL itself can be manipulated to point elsewhere.
**Prevention:** Strictly validate that any externally fetched URI uses HTTPS and its host strictly matches a predefined list of trusted domains (e.g., `.googleusercontent.com` or `.googleapis.com`) using `Uri.TryCreate` with `UriKind.Absolute` before invoking `HttpClient.GetAsync()`.
