# SnitzCore.BackOffice — Admin Controllers & Views Reference

All controllers in the `SnitzCore.BackOffice` namespace live under
`SnitzCore.Admin\Controllers\`. Unless noted, every controller requires the
`Administrator` or `SuperAdmin` role.

---

## AdminController — `/Admin`

**Authorization:** `[Authorize(Roles="SuperAdmin,Administrator")]`

### Index view (`/Admin` → `Index.cshtml`)

The main admin dashboard. Posts to `SnitzConfig/SaveSettings`.

> **SuperAdmin only** — the entire form is disabled for the `Administrator` role.

#### Forum Details section

| Config Key | Label | Notes |
|---|---|---|
| `INTFIRSTINSTALL` | Toggle Startup page | Shows/hides the first-run welcome page. |
| `_STRFORUMTITLE` | Forum Title | Saved to `appsettings.json → SnitzForums:strForumTitle`. |
| `_STRCOPYRIGHT` | Forum Copyright | Saved to `appsettings.json → SnitzForums:strCopyright`. |
| `_STRFORUMDESC` | Forum Description | Saved to `appsettings.json → SnitzForums:strForumDescription`. Textarea. |
| `STRHOMEURL` | Home URL | URL of the main website linked from the forum header. |
| `_STRFORUMURL` | Forum URL | Display-only. Saved to `appsettings.json → SnitzForums:strForumUrl`. |
| `STRADMINUSER` | Main Admin account | Display-only (legacy; replaced by the `SuperAdmin` role). |

#### Options section

| Config Key | Label | Notes |
|---|---|---|
| `STREMAIL` | Email mode | `0` disabled, `1` enabled. |
| `STRUSERNAMEFILTER` | Username filter | Enable the username disallow list. |
| `STRSETCOOKIETOFORUM` | Set cookie to Forum | Scopes the auth cookie to the forum path. |
| `INTPAGETIMER` | Show Page Timer | Displays page render time in the footer. |
| `STRPROHIBITNEWMEMBERS` | Prohibit new Members | Prevents new registrations. |
| `STRREQUIREREG` | Require registration | Requires login to read content. |
| `STRRESTRICTREG` | Restrict registration | Restricts who can register. |
| `INTREQUIRECONSENT` | Require Cookie consent | Shows the cookie consent banner. |
| `INTTHEMECHANGE` | Allow Theme Change | Lets members switch the UI theme. |
| `STRDEFAULTTHEME` | Default Theme | Select list: CarpyTheme, SnitzTheme, DarkTheme, OrangeTheme, VellumTheme. |
| `INTUSETIMEAGO` | Use friendly times | Shows relative times (e.g. "3 hours ago"). |
| `STRDATETYPE` | Default Date Format | Select list of date format patterns (US/UK short, medium, long). |
| `INTLEADERBOARD` | Show Leaderboard | Displays a member leaderboard widget. |
| `INTDONATIONS` | Show Donation link | Shows the PayPal donation button. |

---

### Forum view (`/Admin/Forum` → `Forum.cshtml`)

Tabbed UI for forum-level settings, moderation, spam protection, bad-words, and announcements.

#### General tab — posts to `SnitzConfig/SaveFeatures`

| Config Key | Label | Notes |
|---|---|---|
| `STRIPLOGGING` | IP Logging | Logs poster IP addresses. |
| `STRIPLOOKUP` | Third-party IP lookup | *(Enabled when IP logging is on.)* URL template with `[IP]` placeholder. |
| `INTFULLTEXT` | Use Fulltext search | Enables SQL full-text search instead of `LIKE`. |
| `STRSUBSCRIPTION` | Highest level of subscription | Select: None, Board, Category, Forum, Topic. |
| `INTPROTECTCONTENT` | Protected Content | Hides content from guests. |
| `INTALLOWHIDEONLINE` | Allow members to hide from Online | Creates the `HiddenMembers` Identity role if enabled. |
| `STREXCLUDEBOTS` | Bots to exclude from counter | Comma-separated bot user-agent strings. |
| `STRPAGESIZE` | Default page size | Default items per page across listings. |
| `STRACCOUNTPAGESIZES` | Members page size | Page size for the member list. |

#### Groups tab

Loaded via AJAX from `/Admin/ForumGroups`. Manage category/forum group assignments.

#### Moderators tab — posts to `SnitzConfig/SaveFeatures`

| Config Key | Label |
|---|---|
| `STRMODERATION` | Allow Topic Moderation |
| `STRSHOWMODERATORS` | Show Moderators on forum listing |
| `STRMOVETOPICMODE` | Restrict Topic move to Admins/SuperAdmins only |
| `STRMOVENOTIFY` | Email author when moving topics |

Also contains the **Manage Moderators** form: select a forum from the drop-down,
then use Ctrl+click to select/deselect moderators. Posts to `Admin/UpdateForumModerators`.

#### Anti-Spam tab — posts to `SnitzConfig/SaveCaptcha`

| Config Key | Label | Notes |
|---|---|---|
| `STRUSECAPTCHA` | Use Captcha for Registration | Enables the built-in image-maths Captcha on the register page. |
| `INTEMAILCAPTCHA` | Use Captcha when Emailing | Adds Captcha to the contact/email forms. |
| `STRCAPTCHAOPERATORS` | Captcha operators | Multi-select: which arithmetic operators (`+`, `-`, `×`) to use in the generated image. |

A live Captcha preview is shown beneath the form.

#### Badwords tab

- **Add form** (posts to `SnitzConfig/SaveBadword`): enter a word and its replacement string.
- **Edit/delete list** (posts to `SnitzConfig/UpdateBadwords`): inline editing of all
  existing bad-word entries; click the trash icon to flag a row for deletion.

#### Archives tab

Loaded via AJAX — manage topic archive settings.

#### Announcement tab — posts to `SnitzConfig/SaveFeatures`

| Config Key | Label | Notes |
|---|---|---|
| `STRSHOWANNOUNCE` | Show Announcement | Displays the announcement banner across all pages. |
| `STRANNOUNCETITLE` | Title | Optional custom title; leave blank for the default. |
| `STRANNOUNCE` | Message | Announcement body text (textarea). |

---

### Members view (`/Admin/Members` → `Members.cshtml`)

Tabbed UI for member profile field visibility, roles, ranking, subscriptions, and more.

#### Profile tab — posts to `SnitzConfig/SaveMemberSettings`

Each field has a companion **Required** toggle. All are on/off switches unless noted.

| Config Key | Label |
|---|---|
| `STRFULLNAME` / `STRREQFULLNAME` | Full Name (show / required) |
| `STRAGE` / `STRREQAGE` | Age (show / required) |
| `STRAGEDOB` / `STRREQAGEDOB` | Date of Birth (show / required) |
| `STRMINAGE` | Minimum Age |
| `STRSEX` / `STRREQSEX` | Gender (show / required) |
| `STRMARSTATUS` / `STRREQMARSTATUS` | Marital Status (show / required) |
| `STRCITY` / `STRREQCITY` | City (show / required) |
| `STRSTATE` / `STRREQSTATE` | State (show / required) |
| `STRCOUNTRY` / `STRREQCOUNTRY` | Country (show / required) |
| `STRPICTURE` / `STRREQPICTURE` | Avatar (show / required) |
| `STROCCUPATION` / `STRREQOCCUPATION` | Occupation (show / required) |
| `STRHOMEPAGE` / `STRREQHOMEPAGE` | Homepage (show / required) |
| `STRBIO` / `STRREQBIO` | Bio (show / required) |
| `STRAIM` / `STRREQAIM` | AIM (show / required) |
| `STRICQ` / `STRREQICQ` | ICQ (show / required) |
| `STRMSN` / `STRREQMSN` | Skype/MSN (show / required) |
| `STRYAHOO` / `STRREQYAHOO` | Yahoo! (show / required) |
| `STRFAVLINKS` / `STRREQFAVLINKS` | Favourite Links (show / required) |
| `STRHOBBIES` / `STRREQHOBBIES` | Hobbies (show / required) |
| `STRLNEWS` / `STRREQLNEWS` | Latest News (show / required) |
| `STRQUOTE` / `STRREQQUOTE` | Quote (show / required) |
| `STRRECENTTOPICS` / `STRREQRECENTTOPICS` | Recent Topics (show / required) |

#### Roles tab

Loaded via AJAX from `/Admin/ManageRoles`. Allows:
- View all Identity roles and the members assigned to them.
- Assign a member to a role (username autocomplete via `/Admin/AutoCompleteUsername`).
- Create a new custom role.
- Remove a member from a role.
- Delete a role.

#### Ranking tab

Loaded via AJAX from `/SnitzConfig/RankingConfig`. Allows:
- Set the rank display type (text title, image, or both).
- Edit each rank tier's title, post threshold, image path, and image repeat count.

#### Subscriptions tab

Loaded via AJAX from `/Admin/ManageSubscriptions`. Filter options:

| Filter | Scope |
|---|---|
| 0 | All subscriptions |
| 1 | Board-level subscriptions |
| 2 | Category subscriptions |
| 3 | Forum subscriptions |
| 4 | Topic subscriptions |
| 5 | Archived topic subscriptions |

Delete individual rows or bulk-delete the selected filter group.

#### Username Filters tab

- **Add form** (posts to `SnitzConfig/SaveUsername`): enter a name pattern to disallow.
- **Edit list** (posts to `SnitzConfig/UpdateUsernameFilter`): inline editing; click
  the delete button to remove a filter entry.

#### Pending Members tab

Loaded via AJAX from `/Admin/PendingMembers` — list and approve/reject members
awaiting activation.

#### Avatars tab

Loaded via AJAX from `/Admin/ManageAvatars` — grid of all member avatars. Clicking the
delete button clears the database record and removes the image from
`wwwroot/Content/Avatar`.

#### Create Member tab

Loaded via AJAX from `/Admin/CreateUser` — create a new member account directly
without the standard registration flow.

---

### Feature Configuration view (`/Admin/Features` → `FeatureConfig.cshtml`)

Tabbed UI, each tab posts to `SnitzConfig/SaveFeatures`.

#### Forum Settings tab

| Config Key | Label | Notes |
|---|---|---|
| `STRPRIVATEFORUMS` | Private Forums | Enable private forum support. |
| `STRARCHIVESTATE` | Archive functions | Enable/disable the topic archive feature. |
| `STRSHOWSTATISTICS` | Show detailed statistics | Toggle detailed board statistics. |
| `STRJUMPLASTPOST` | Show Jump To Last post link | Jump-to-last-post link on listings. |
| `INTFORUMRSS` | Show RSS Link | Toggle the RSS feed link. |
| `STRSHOWPAGING` | Show quick paging | Enable quick paging controls. |
| `STRPAGENUMBERSIZE` | Page numbers per row | *(Enabled when paging is on.)* Default `10`. |
| `STRFORUMPAGESIZES` | Page sizes | Comma-separated options for forum listings (default `10,25,50,100`). |
| `STRSEARCHPAGESIZES` | Search results page size | Page size for search results (default `10`). |

#### Topic tab

| Config Key | Label | Notes |
|---|---|---|
| `INTTOPICDISPLAY` | Show Topic on all pages | Repeat the topic post on every reply page. |
| `STRSHOWQUICKREPLY` | Show Quick Reply | Inline quick-reply box. |
| `STRSTICKYTOPIC` | Allow Sticky Topics | Enable pinned/sticky topics. |
| `STRHOTTOPIC` | Hot Topics | Enable hot-topic indicator. |
| `INTHOTTOPICNUM` | Hot Topic Count | *(Enabled when hot topics is on.)* Post count threshold. |
| `STREDITEDBYDATE` | Show edited by on Date | Show "last edited by" beneath a post. |
| `STRSHOWTOPICNAV` | Show Prev / Next Topic | Previous/next topic navigation links. |
| `STRSHOWSENDTOFRIEND` | Show send Topic to a friend | Send-to-friend link. |
| `STRSHOWPRINTERFRIENDLY` | Show printer friendly link | Printer-friendly page link. |
| `STRSOCIALSHARE` | Social Media links | Social sharing buttons on topics. |
| `STRTOPICSORT` | Topic sort direction | Default reply sort (`asc` / `des`). |
| `STRTOPICPAGESIZES` | Topic Replies page size | Comma-separated options (default `10,25,50,100`). |
| `STRACTIVEPAGESIZES` | Active Topics page size | Options for the Active Topics list (default `10,20,50`). |
| `INTRECENTCOUNT` | Recent Topics list | Widget count (default `5`). |

#### Posting Options tab

| Config Key | Label | Notes |
|---|---|---|
| `STRFLOODCHECK` | Enable flood Control | Prevent rapid repeated posting. |
| `STRFLOODCHECKTIME` | Flood lockout (seconds) | *(Enabled when flood control is on.)* Default `30`. |
| `STRBADWORDFILTER` | Filter out bad words | Apply the bad-words list to post content. |
| `STRALLOWFORUMCODE` | Allow BB code tags | Enable BBCode formatting. |
| `STRIMGINPOSTS` | Allow images in posts | Allow `[img]` tags. |
| `STRICONS` | Allow smiley icons | Enable smiley/emoji icons. |
| `STRALLOWUPLOADS` | Allow File Attachments | Enable file attachments on posts. |
| `INTMAXFILESIZE` | Max File size (MB) | *(Enabled when uploads are on.)* Default `5`. |
| `STRFILETYPES` | Allowed Extensions | *(Enabled when uploads are on.)* Default `.txt,.pdf,.zip,.png,.jpg`. |
| `STRSIGNATURES` | Allow Signatures | Enable member post signatures. |
| `STRDSIGNATURES` | Dynamic Signatures | Enable dynamically rendered signatures. |

#### Private Messages tab

| Config Key | Label | Notes |
|---|---|---|
| `STRPMSTATUS` | Enable Private Messages | Master toggle for the PM system. |
| `STRPMATTACH` | Allow attachments | File attachments in PMs. |
| `STRPMSIG` | Allow signatures | Include signatures in PMs. |
| `STRPMSOUND` | Play notification sound | Sound on new PM arrival. |
| `INTMAXPMRECIPIENTS` | Max PM recipients | Default `50`. |
| `STRPMLIMIT` | Message Limit | Max PM storage per member. Default `500`. |

#### Plugins / Mods tab

Rendered as an accordion. Each panel is a View Component and only shown when the
corresponding database table exists.

| Plugin | Config Key | Condition |
|---|---|---|
| Ad Rotator | — | Always shown; managed via `BannerController`. |
| Polls | — | Always shown; rendered by the `Polls` view component. |
| Bookmarks | `STRBOOKMARK` | Only shown when `FORUM_BOOKMARKS` table exists. |
| Topic Rating | `INTTOPICRATING` | Only shown when `TOPIC_RATINGS` table exists. |
| Image Album | — | Rendered by the `ImageAlbum` view component. |
| Events / Calendar | — | Rendered by the `Events` view component. |
| Post Thanks | — | Rendered by the `PostThanks` view component. |

---

### Page Templates view (`/Admin/ListViews` → `ListViews.cshtml`)

A two-panel view for editing Razor view templates directly in the browser:

- **Left panel** — a tree listing of all `.cshtml` files under the `Views/` folder,
  grouped by subfolder.
- **Right panel** — a CodeMirror editor (C# mode) that loads the selected file via
  `GET /Admin/LoadFile?id={encodedPath}`.
- **Save Changes** button — confirms with a warning dialog then posts the edited content
  to `POST /Admin/SaveTemplate`.

> ⚠️ Changes are written directly to disk and are permanent. There is no version
> control within this UI — rely on Git for rollback.

---

## SnitzConfigController — `/SnitzConfig`

**Authorization:** `[Authorize(Roles="Administrator")]`

| Method | HTTP | What it saves |
|---|---|---|
| `Index` | GET | Renders the main config shell view. |
| `SaveSettings` | POST | `strForumTitle`, `strCopyright`, `strForumDescription`, `strForumUrl` → `appsettings.json`. All other form keys → `FORUM_CONFIG` via `SaveForm`. |
| `SaveFeatures` | POST | Forum, topic, posting, PM, and plugin feature toggles → `FORUM_CONFIG` via `SaveForm`. |
| `SaveCaptcha` | POST | `STRUSECAPTCHA`, `INTEMAILCAPTCHA`, `STRCAPTCHAOPERATORS` → `FORUM_CONFIG` via `SaveForm`. |
| `SaveMemberSettings` | POST | Member profile field visibility flags → `FORUM_CONFIG` via `SaveForm`. |
| `SaveBadword` | POST | Inserts a new row into `FORUM_BADWORDS`. |
| `UpdateBadwords` | POST | Batch updates or deletes existing `FORUM_BADWORDS` rows. |
| `UpdateUsernameFilter` | POST | Batch updates or deletes `FORUM_MEMBERNAMEFILT` rows. |
| `SaveUsername` | POST | Inserts a new row into `FORUM_MEMBERNAMEFILT`. |
| `RankingConfig` (GET) | GET | Returns `ManageRanking` partial view with current rank tiers. |
| `RankingConfig` (POST) | POST | Updates rank display type and all tier entries in `FORUM_MEMBERRANKING`. |

> **`SaveForm` helper** — iterates every form key not starting with `_` or `X-`,
> upserts the matching `FORUM_CONFIG` row inside a transaction, and cache-busts
> the key via `ISnitzConfig.RemoveFromCache`.

---

## EmailConfig view (`/Admin/EmailConfig` → `EmailConfig.cshtml`)

Posts to `Admin/EmailConfigUpdate`.

#### SMTP Settings (saved to `appsettings.json → MailSettings`)

| Field | Notes |
|---|---|
| SMTP Server | Hostname or IP of the mail server. |
| Port | Default `25`; use `587` for StartTLS. |
| From address | The `From` header on all outgoing email. |
| SSL/TLS mode | Select: `None`, `Auto`, `StartTls`, `SslOnConnect`. |
| Require login | When enabled, Username and Password fields are shown. |
| Username / Password | SMTP authentication credentials. |

#### Email behaviour (saved to `FORUM_CONFIG`)

| Config Key | Label |
|---|---|
| `STREMAIL` | Email notification mode (`0` off, `1` on). |
| `STRFILTEREMAILADDRESSES` | Use StopForumSpam spam filter on email addresses. |
| `STRCONTACTEMAIL` | Contact/reply-to address (defaults to `From`). |
| `STRUNIQUEEMAIL` | Require unique email per member. |
| `STREMAILVAL` | Validate email address format on registration. |
| `STRLOGONFORMAIL` | Require login before using the contact form. |
| `INTMAXPOSTSTOEMAIL` | Maximum replies included in subscription emails. |
| `STRNOMAXPOSTSTOEMAIL` | Override — always send all replies regardless of count. |

Banned email domains are shown in a read-only table sourced from `FORUM_SPAMFILTER`.

---

## BannerController — `/Banner`

**Authorization:** None declared — restrict at IIS or middleware level in production.

| Method | What it does |
|---|---|
| `Index` | Lists all `"Admin"`-tagged banner adverts. |
| `Save` | Persists an updated `Advert` record. |
| `Delete` | Removes a banner record. ⚠️ Physical image file is **not** deleted automatically. |

---

## LogViewController — `/LogView`

**Authorization:** `[Authorize(Roles="Administrator")]`

Log files are JSON files in the `Logs/` folder at the content root.

| Method | What it does |
|---|---|
| `Index` | Lists all `.json` log files, ordered by name descending. Selecting a file deserialises it and passes entries to the view. Stores the selection in session for DataTables AJAX. |
| `DeleteLog` | Deletes the specified log file from disk (`200 OK` / `204 No Content`). |
| `CustomServerSideSearchAction` | DataTables server-side endpoint — returns paged, date-sorted log entries as JSON. |

---

## FileManagerController — `/FileManager`

**Authorization:** None declared — restrict via middleware or IIS rules in production.

| Method | What it does |
|---|---|
| `SnitzFileApi` | Generic file manager API (browse, upload, rename, delete) delegated to `IFileManagerCommandsProcessor`. |
| `Index` | Renders the file manager UI. |
| `Releases` | Lists all files in `wwwroot/releases` joined with their `FORUM_FILECOUNT` records (title, version, download count, archived flag). |
| `Download` | Increments `LinkHits` counter then streams the file as `application/zip`. |
| `UploadRelease` | Accepts multipart upload (max ~200 MB), saves to `wwwroot/releases`, inserts a `FORUM_FILECOUNT` record. |
| `EditRelease` (GET) | Returns the `_EditForm` partial pre-populated with the release record. |
| `EditRelease` (POST) | Updates title, version, link order, and archived flag for a release record. |