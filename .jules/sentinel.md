## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2024-05-30 - Server-Side Request Forgery in Google Photo Integrations
**Vulnerability:** GooglePhotoUrl was fetched directly from a string via HttpClient.GetAsync without validating the URI. This allowed an attacker to input any URL (e.g., local endpoints or internal metadata endpoints) and force the server to fetch its contents, resulting in a Server-Side Request Forgery (SSRF) vulnerability.
**Learning:** External URL fetching must be rigorously validated. Strings should be parsed into Uris and verified for the HTTPS scheme, Absolute kind, and specific trusted hosts before invocation.
**Prevention:** Use Uri.TryCreate with UriKind.Absolute to parse URLs safely, verify that uri.Scheme == Uri.UriSchemeHttps, and check that uri.Host matches a specific, trusted suffix (e.g., EndsWith(".googleusercontent.com")).
