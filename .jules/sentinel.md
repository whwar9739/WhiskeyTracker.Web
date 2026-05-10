## 2024-04-16 - Missing Admin Authorization on Master Whiskey Data
**Vulnerability:** Any authenticated user could create, edit, or delete entries in the master `Whiskey` global directory because `Create.cshtml.cs`, `Edit.cshtml.cs`, and `Delete.cshtml.cs` under `Pages/Whiskies` lacked authorization attributes. Only the `/Admin` folder was protected by convention.
**Learning:** Razor Pages convention-based folder authorization (`AuthorizeFolder("/Admin")`) does not automatically protect administrative-level entities that reside outside the designated admin folder.
**Prevention:** Always explicitly annotate page models with `[Authorize(Roles = "Admin")]` for global entity modification pages, regardless of folder structure.

## 2025-05-24 - [SSRF Mitigation in Image Uploads]
**Vulnerability:** A critical Server-Side Request Forgery (SSRF) risk existed because user-controlled `GooglePhotoUrl` inputs were passed directly to `HttpClient.GetAsync()` without domain validation in `Create.cshtml.cs` and `Edit.cshtml.cs`.
**Learning:** External integrations fetching resources via URLs supplied by users must have strictly validated endpoints.
**Prevention:** Apply whitelist-based URI validation, enforcing HTTPS and trusted domains, prior to invoking outbound HTTP calls.
