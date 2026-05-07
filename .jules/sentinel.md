## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2026-05-07 - Server-Side Request Forgery (SSRF) via Google Photo Fetching
**Vulnerability:** The application was fetching Google photo images based on user-provided `GooglePhotoUrl` without validating the host or scheme. This could allow an attacker to make the server issue HTTP GET requests to internal networks or unapproved external URLs, resulting in a Server-Side Request Forgery (SSRF) vulnerability.
**Learning:** Whenever an application makes outbound HTTP requests based on user input, it's critical to strictly validate the destination URL.
**Prevention:** Use `Uri.TryCreate` to ensure the URL is valid, verify the scheme is strictly `https`, and check that the host matches a specific, trusted allowlist (e.g., `.googleusercontent.com` and `.googleapis.com`) before making the request.
