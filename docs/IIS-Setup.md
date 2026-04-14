# Snitz.NetCore Forum — Complete Installation & Setup Guide

## 1. Overview

This guide covers everything needed to get Snitz.NetCore running: from choosing a
hosting environment and configuring `appsettings.json`, through publishing and IIS
setup, to the first-run wizard, upgrading from a legacy installation, and automating
future deployments.

---
## 2. Choosing a hosting environment

Before diving into the technical steps, choose the right hosting environment for your
forum. Each option has trade-offs in control, scalability, cost, and maintenance.

| Option | Best for | Notes |
|---|---|---|
| **On-premises / Windows Server + IIS** | Full control, existing infrastructure | Covered in detail in this guide. |
| **Cloud (Azure, AWS, GCP)** | Scalability, managed services | Publish output is identical; cloud-specific app service or container hosting replaces the IIS steps. |
| **Shared hosting** | Low traffic, budget deployments | Provider must support .NET 8 and allow a custom `web.config`. Transfer files via FTP/SFTP (e.g. FileZilla, WinSCP). |

> For **shared hosting**, upload the publish output via FTP/SFTP instead of copying
> locally, then use your host's control panel to map the folder to a hostname.

---

## 3. Requirements

- Windows Server 2019/2022 or Windows 10/11 with the IIS role installed
  (_Web Server_, _Management Tools_, _WebSockets_, _Static Content_,
  _HTTP Redirection_, _Request Filtering_, _IIS Management Console_).
- Latest **.NET 8 Hosting Bundle** (installs the runtime plus ASP.NET Core Module V2).
  Reboot after installation.
- One of the supported database engines reachable from the IIS host:
  SQL Server / SQL Express, MS SQL LocalDB, MySQL / MariaDB, or SQLite.
- Domain/hostname and TLS certificate (recommended for production).

---

## 4. Download the release ZIP

Each release is distributed as a pre-built ZIP archive — no SDK or build tools are
required on the server.

1. Go to the [Releases page](https://github.com/huwred/Snitz.NetCore/releases) on GitHub.
2. Under the latest release, download **SnitzForum.zip** (or the versioned equivalent,
   e.g. `SnitzForum_8.0.1.15.zip`).
3. Save the ZIP to a convenient location on the server (e.g. `C:\Installs\SnitzForum.zip`).

The ZIP contains the complete publish output — `web.config`, all assemblies,
`wwwroot`, `App_Data`, `appsettings.json`, and `App_Offline.htm` are all included.

---

## 5. Extract and place files on the server

1. Create `C:\inetpub\SnitzForum`.
2. Extract the contents of `SnitzForum.zip` directly into `C:\inetpub\SnitzForum`
   so that `web.config` sits at the root of that folder.
   - **Windows Explorer:** right-click the ZIP → _Extract All_ → set the destination
     to `C:\inetpub\SnitzForum`.
   - **PowerShell:**
     ```powershell
     Expand-Archive -Path 'C:\Installs\SnitzForum.zip' -DestinationPath 'C:\inetpub\SnitzForum' -Force
     ```
   - **Shared hosting / FTP:** upload the ZIP via FTP/SFTP (FileZilla, WinSCP) and
     use your host's file manager to extract it, or extract locally first and upload
     the extracted files.
3. Grant **Modify** permissions on the folder to `IIS_IUSRS` (needed for logging,
   file uploads, `appsettings.json` writes by the setup wizard, and runtime caches).
4. If you are using **SQLite or LocalDB**, also create `C:\inetpub\SnitzForum\App_Data`
   and grant **Modify** to `IIS_IUSRS` — the setup wizard creates the `.db` / `.mdf`
   file there automatically. (The `App_Data` folder may already exist in the ZIP.)
5. **Leave `App_Offline.htm` in place** until IIS is configured and you are ready to
   run the wizard or upgrade.

---

## 6. Configure `appsettings.json` before deploying

A file called `App_Offline.htm` is shipped in the root folder of the published output.
This prevents the application from serving requests until you are ready. Edit
`appsettings.json` **before** removing that file.

### Connection strings

If you already know your connection details, add them now. Otherwise leave
`SnitzConnection` empty and the setup wizard will populate it on first run.

> **Upgrade note:** If your legacy database uses a non-standard table prefix (e.g.
> `SNF_` instead of `FORUM_`), set `forumTablePrefix` and `memberTablePrefix`
> **before** running the upgrade, otherwise EF Core will target the wrong tables.

### Full annotated `appsettings.json`

```json
{
  "ConnectionStrings": {
    "SnitzConnection": "Server=.\\SQL2019;Database=SnitzMVC;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true",
    "HangfireConnection": "Server=.\\SQL2019;Database=SnitzMVC;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true",
    "DBProvider": "mssql"
  },
  "SnitzForums": {
    "forumTablePrefix": "FORUM_",
    "memberTablePrefix": "FORUM_",
    "strForumUrl": "https://yourservername/",
    "strForumDescription": "Short description about your Forums",
    "strForumTitle": "Name of your Forums",
    "strCopyright": "{0} H Reddick",
    "strUniqueId": "SnitzCore00",
    "excludeBots": "googlebot,bing,duckduck,yahoo,spider,bot,facebook,netEstate,Not-A.Brand,barkrowler",
    "SupportedLanguages": [ "en", "no", "fa" ],
    "Log4NetCleanupTime": 3,
    "Log4NetCleanupOlderThan": 7,
    "VirtualPath": "",
    "LandingPage": "",
    "strVersion": ""
  },
  "MailSettings": {
    "From": "name@youremail.com",
    "UserName": "",
    "Password": "",
    "RequireLogin": false,
    "SmtpServer": "mailserver address",
    "Port": 25,
    "SecureSocketOptions": "StartTls"
  },
  "IdentityOptions": {
    "User": {
      "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
      "RequireUniqueEmail": false
    },
    "Password": {
      "RequiredLength": 10,
      "RequireDigit": true,
      "RequiredUniqueChars": 4,
      "RequireLowercase": true,
      "RequireNonAlphanumeric": true,
      "RequireUppercase": true
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Error",
      "Hangfire": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Key settings explained:**

| Setting | Description |
|---|---|
| `DBProvider` | `mssql`, `mysql`, or `sqlite` |
| `forumTablePrefix` / `memberTablePrefix` | Must match the prefix used in your database. Default is `FORUM_`. |
| `strForumUrl` | Public base URL including trailing slash. Must start with `https://`. |
| `AllowedUserNameCharacters` | Includes a space to permit spaces in usernames. Remove the space if not required. |
| `Log4NetCleanupTime` | Hour (24h) at which the daily log cleanup job runs. |
| `Log4NetCleanupOlderThan` | Log files older than this many days are deleted. |
| `VirtualPath` | Set automatically by the application — leave empty. |
| `SecureSocketOptions` | `None`, `Auto`, `SslOnConnect`, `StartTls`, or `StartTlsWhenAvailable`. |

> **Security:** Store passwords and API keys in environment variables or the
> .NET Secret Manager rather than plain `appsettings.json` for production deployments.

---

## 7. Configure IIS

### 7.1 Application Pool

| Setting | Value |
|---|---|
| Name | `SnitzForumPool` |
| .NET CLR Version | **No Managed Code** |
| Pipeline Mode | Integrated |
| Start Automatically | Yes |

### 7.2 Website

1. **Add Website** -> Site name `SnitzForum`, physical path `C:\inetpub\SnitzForum`,
   app pool `SnitzForumPool`.
2. Bind to `https` with your TLS certificate; optionally keep `http` for a redirect.
3. In **Configuration Editor** (`system.webServer/aspNetCore`) confirm `hostingModel`
   is `InProcess` (the default for .NET 8 publish output).

### 7.3 Environment variable

In the site's **Configuration > Application Settings** add:

| Name | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

This tells the app to load `appsettings.Production.json` over the development defaults.

### 7.4 Optional tuning

- Enable **Static Content Compression** and HTTP/2 for faster delivery of `wwwroot` assets.
- Configure HTTP-to-HTTPS redirects with the IIS URL Rewrite module.
- Keep IIS in-memory session enabled (default) — the Captcha view component writes the
  answer into `HttpContext.Session` and requires it to be available on postback.
- For Integrated Security, run the app pool under a domain account and grant that account
  `db_owner` (or appropriate) access to the database.

---

## 8. Starting the forum

Once IIS is configured, rename `App_Offline.htm` to `App_Offline.htm.old` (or delete
it) and open your forum URL in a browser.

- **Fresh install** — no `SnitzConnection` in `appsettings.json` -> the setup wizard
  starts automatically (see section 9).
- **Upgrade from a previous version** — `SnitzConnection` is already present -> the
  upgrade page is displayed (see section 10).

---

## 9. First-run setup wizard (fresh install)

When no valid `SnitzConnection` exists in `appsettings.json`, the application
automatically redirects every request to `/Setup`. The wizard is a single page with
three fieldsets.

### Step 1 — Database Configuration

Choose your database provider and supply the connection details, then click
**Test Connection** to verify before continuing.

| Provider | Server | Credentials | Notes |
|---|---|---|---|
| **MS SQL Server / SQL Express** | Required | Integrated Security (default) or Username + Password | _Use Encryption_ and _Trust Server Certificate_ available. For production, use a CA-issued certificate and leave _Trust Server Certificate_ unchecked. |
| **MS SQL LocalDB** | Not required | Always Integrated Security | A `.mdf` file is attached from `App_Data`. |
| **MySQL / MariaDB** | Required | Username + Password always required | Encryption options are not applicable. |
| **SQLite** | Not required | Not required | A `.db` file is created in `App_Data`. |

A green **:white_check_mark: Connection successful!** message confirms connectivity. A red **:x:**
displays the driver error so you can correct the settings before proceeding.

> **Security note:** For production SQL Server, uncheck _Trust Server Certificate_
> and use a certificate issued by a trusted CA.

### Step 2 — Forum Setup

| Field | Description |
|---|---|
| **Forum URL** | The public base URL (e.g. `https://forums.yourdomain.com`). Must start with `https://`. Auto-populated from the current request including any virtual path / path base. |
| **Landing Page** | **All Forums** — classic view of all categories and forums (default). **Welcome page** — displays a customisable static page as the home page. |

The wizard writes the chosen values directly into `appsettings.json` under
`SnitzForums.strForumUrl` and `SnitzForums.LandingPage`.

### Step 3 — Forum Administrator

This section only appears when no administrator account (membership level 3) exists in
the database. It is skipped automatically on subsequent wizard visits once an admin exists.

| Field | Requirement |
|---|---|
| **Username** | Login name for the forum administrator. |
| **Email** | Valid email address. |
| **Password** | Must meet the policy in `IdentityOptions.Password` (default: min 10 chars, 1 digit, 1 uppercase, 1 non-alphanumeric, 4 unique characters). |

### What happens when you click Continue

1. Connection strings and forum settings are written back to `appsettings.json`.
2. EF Core migrations run (`Database.MigrateAsync`) to create or upgrade the schema.
3. The administrator account is seeded if one does not already exist.
4. The app restarts and redirects to the forum home page.

> **Note:** If `IIS_IUSRS` does not have **Modify** rights on the site root, the wizard
> will fail to write `appsettings.json`. See section 6.

---

## 10. Upgrading from a previous version of Snitz Forums

> If you are doing a **fresh install**, skip this section.

### 10.1 Pre-upgrade checklist

- [ ] Back up the database.
- [ ] Set the correct table prefixes in `appsettings.json` if your legacy database
      uses a non-standard prefix (see section 4).
- [ ] Verify SMTP settings — the migration process sends confirmation emails to members.
- [ ] Ensure `IIS_IUSRS` has **Modify** rights on the site root (the upgrade writes
      back to `appsettings.json`).

### 10.2 Running the upgrade

1. With `App_Offline.htm` renamed to `.old`, open your forum URL in a browser.
2. The **Upgrade** page is displayed. Review the options (they vary depending on what
   is already in `appsettings.json`), then click **Continue**.
3. EF Core migrations run. When complete a **Restart Application** button appears.
4. Click **Restart Application**. The forum applies the remaining upgrades and
   redirects to the home page.

Read all messages displayed during the process — they help resolve potential issues.

### 10.3 Post-upgrade: migrate the administrator account

Log in with your existing admin credentials immediately after the upgrade to trigger
account migration:

1. Open the login form, enter your username and password, and click **Sign In**.
2. **Password valid and meets current policy** -> account migrated, signed in immediately.
3. **Password valid but does not meet current policy** -> redirected to
   `/Account/MigratePassword` to set a new password.
4. **Password cannot be verified** -> a locked Identity account is created, a
   confirmation email is sent, and you are redirected to `/Account/ConfirmMigrateEmail`.
   Click the link in the email to unlock the account and set a new password.

> **Important:** After migrating your account, navigate to **Admin -> Manage Roles** and
> add your admin account to the **SuperUser** role to restore full administrative access.

---

## 11. Member account migration (bulk upgrade)

When upgrading, existing member accounts are **not pre-migrated in bulk**. Each account
is migrated automatically and transparently the first time that member logs in.

### How it works

The login process checks whether an ASP.NET Core Identity record already exists for the
username. If none is found, the `MigrateMember` flow runs:

1. **Password validation** — the supplied password is tested against the legacy format:
   - **Classic Snitz password** — SHA-256 hex hash stored in `FORUM_MEMBERS.M_PASSWORD`.
   - **ASP.NET Membership password** — PBKDF2/SHA-1 hash stored in the legacy membership
     table, verified by `CustomPasswordHasher`.

2. **New Identity user created** — a `ForumUser` record is inserted with the member's
   existing username, email, and membership date. Roles are mapped from the member's
   level (1 = ForumMember, 2 = Moderator, 3 = Administrator).

3. **One of three outcomes:**

| Outcome | What happens |
|---|---|
| **Password valid and meets current policy** | Identity user created, user signed in immediately. Migration is invisible. |
| **Password valid but does not meet current policy** | Identity user created, user redirected to `/Account/MigratePassword` before sign-in. |
| **Password cannot be verified** | Locked Identity account created, confirmation email sent, user redirected to `/Account/ConfirmMigrateEmail`. Clicking the email link unlocks the account and prompts a new password. |

### What administrators should tell users

- **Most users** log in as normal and notice nothing different.
- Users whose passwords **do not meet the new policy** are prompted to choose a new
  password immediately after their first login.
- Users who have **forgotten their password** or whose password cannot be verified
  should use the **Forgot Password** link, or follow the migration confirmation email.
- Ensure **SMTP settings** (`MailSettings` in `appsettings.json`) are correct before
  going live so that migration confirmation emails are delivered.

### Verifying migration health

- Monitor the application log (`%ProgramFiles%\dotnet\logs` or Windows Event Viewer)
  for `Error finding member record` entries, which indicate a member whose legacy record
  could not be located.
- Any member whose `MEMBER_ID` does not exist in `FORUM_MEMBERS` or the legacy
  membership table will see a _'No member to migrate'_ error and must contact the
  administrator to have their account reset manually.

---

## 12. Database connectivity

- Ensure the IIS host can reach SQL Server on port **1433** (or your configured port).
- For SQL authentication, credentials are stored in the connection string written by the
  wizard into `appsettings.json`.
- For Integrated Security, run the app pool under a domain account and grant that
  account `db_owner` (or appropriate) access to the database.

---

## 13. Smoke testing & troubleshooting

1. Browse `https://forums.yourdomain.com` from the server to warm up the worker process.
2. Log in with the administrator account.
3. If the worker process fails to start, check:
   - **Windows Event Viewer -> Windows Logs -> Application**
   - `%ProgramFiles%\dotnet\logs` for ASP.NET Core Module errors.
   - Folder permissions (`IIS_IUSRS` Modify rights on site root and `App_Data`).
   - `web.config` (generated during publish) for a missing or incorrect `aspNetCore`
     handler.
4. Verify Razor Pages endpoints (e.g. `/`, `/Account/Login`) load without HTTP 500s.
5. Confirm session-based features (Captcha on registration, moderation flows) work
   end-to-end.

### Common errors and fixes

| Symptom | Likely cause | Fix |
|---|---|---|
| HTTP 500.19 on startup | Malformed `web.config` | Re-publish; check the `aspNetCore` handler entry. |
| HTTP 500.30 in-process startup failure | Missing .NET 8 Hosting Bundle | Install the bundle and reboot. |
| Wizard fails to save settings | `IIS_IUSRS` lacks Modify on site root | Grant Modify on `C:\inetpub\SnitzForum`. |
| Emails not delivered during migration | Wrong SMTP settings | Update `MailSettings` in `appsettings.json`. |
| Admin loses access after upgrade | Not added to SuperUser role | **Admin -> Manage Roles -> add to SuperUser**. |
| SQLite / LocalDB file not created | `App_Data` missing or wrong permissions | Create folder and grant `IIS_IUSRS` Modify. |
| Forum shows wrong table / empty data | Wrong table prefix in `appsettings.json` | Set `forumTablePrefix` / `memberTablePrefix` before upgrade. |

---

## 14. Updating to a newer release

Once the wizard has run once, `appsettings.json` contains all connection strings and
forum settings. Subsequent updates only require extracting the new ZIP over the
existing site — the wizard is skipped automatically because `SnitzConnection` is
already present.

### Recommended update procedure

1. **Download** the new `SnitzForum.zip` from the
   [Releases page](https://github.com/huwred/Snitz.NetCore/releases).
2. **Put the site offline** — copy `App_Offline.htm.old` back to `App_Offline.htm`
   (or create a new one) so users see a maintenance page during the update.
3. **Back up** the existing `appsettings.json` and `App_Data` folder.
4. **Extract** the new ZIP over `C:\inetpub\SnitzForum`, overwriting all files:
   ```powershell
   Expand-Archive -Path 'C:\Installs\SnitzForum_new.zip' -DestinationPath 'C:\inetpub\SnitzForum' -Force
   ```
5. **Restore** your backed-up `appsettings.json` if the extract has overwritten it
   with the default template.
6. **Bring the site back online** — rename `App_Offline.htm` to `App_Offline.htm.old`.
7. Browse to the forum. If schema changes are included, the upgrade page appears
   automatically (see section 10).

### Additional tips

- Use **PowerShell Desired State Configuration (DSC)** for repeatable IIS provisioning
  across multiple servers.
- Pair IIS **Application Initialization** with a scheduled ping to keep the worker
  process warm and avoid cold-start delays.
