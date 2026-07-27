# Deploying DentLink to SmarterASP.NET (Free Hosting)

SmarterASP.NET was chosen because its free tier includes a **free MSSQL database**,
which matches this project's SQL Server + EF Core setup with no code changes.

I've already prepared everything on my side:
- `DentLink-deploy.zip` — the compiled, ready-to-upload Release build (framework-dependent, targets .NET 8)
- `appsettings.Production.json` (inside the zip) — a connection-string placeholder you'll fill in after creating your database

I can't create the hosting account or enter any passwords for you — you'll need to do the
signup and upload steps yourself. Everything below is written so you can follow it directly.

---

## Step 1 — Create your free account

1. Go to **https://www.smarterasp.net** and click **Free ASP.NET Hosting**.
2. Fill in the signup form yourself (email, password, etc.) and verify your email.
   No credit card is required for the free plan.

## Step 2 — Create a website (hosting space)

1. In the SmarterASP.NET control panel, click **Add Website** (or use the one created automatically at signup).
2. Give it any name, e.g. `dentlink`.
3. Once created, open its **Website Detail** page — note the temporary URL, e.g.
   `dentlink.smarterasp.net`.
4. Under **.NET Version** (or "Application Pool" settings) for the site, select **.NET 8 (or newer LTS available)**.
   This must match the app, which targets `net8.0`.

## Step 3 — Create the free MSSQL database

1. In the control panel, go to **MSSQL Databases** → **Create Database**.
2. Give it a name (e.g. `dentlinkdb`). SmarterASP will generate:
   - A **SQL Server hostname**, something like `SQLxxxx.smarterasp.net`
   - A **database name**, **username**, and **password**
3. **Write these four values down** — you'll need them in Step 5.

## Step 4 — Upload the site files

You have two options; pick whichever is easier for you:

**Option A — File Manager (simplest, no extra software)**
1. In the control panel, open **File Manager**, navigate to the `wwwroot` (or site root) folder.
2. Upload `DentLink-deploy.zip` there, then use the panel's **Extract/Unzip** option to unzip it in place.
3. Delete the zip file afterwards to save space (free plan has a disk quota).

**Option B — FTP (e.g. FileZilla)**
1. Get your FTP host/username/password from the control panel's **FTP Accounts** section.
2. Connect with FileZilla, then upload the **contents** of the unzipped `DentLink-deploy.zip`
   folder directly into the site's root folder (not the zip itself — extract it locally first).

## Step 5 — Point the app at your database

1. Back in **File Manager**, open `appsettings.Production.json` (in the site root) for editing.
2. Replace the placeholder connection string with your real values from Step 3:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=SQLxxxx.smarterasp.net;Database=dentlinkdb;User Id=your_db_username;Password=your_db_password;TrustServerCertificate=True;"
     }
   }
   ```
3. Save the file.

> Why `appsettings.Production.json` and not `appsettings.json`? IIS hosts run with
> `ASPNETCORE_ENVIRONMENT=Production` by default, so ASP.NET Core automatically merges
> `appsettings.Production.json` over `appsettings.json`, overriding just the connection
> string without touching the original file.

## Step 6 — First run (creates & seeds the database automatically)

1. Visit your site's URL (e.g. `http://dentlink.smarterasp.net`).
2. On first request, the app automatically runs `Database.Migrate()` — this creates all
   7 tables in your empty SmarterASP database — and seeds the sample university, company,
   employees, and appointment slots. No manual SQL scripts needed.
3. You should land on the DentLink welcome page. Try logging in with the sample accounts
   from the README (`admin@ku.edu.kw` / `University@123` and `hr@gulftech.com` / `Company@123`).

## Troubleshooting

- **500 error / blank page on first load**: open File Manager → the site's `logs` folder
  (created automatically) and check `stdout_*.log` for the real exception — almost always
  a wrong connection string in Step 5.
- **"Cannot open database" / login failed**: double-check the DB username/password and that
  `TrustServerCertificate=True` is present (SmarterASP's SQL Server uses a cert your app
  won't otherwise trust).
- **Wrong .NET version error**: revisit Step 2 and make sure the site's .NET version matches
  `net8.0`.
- **Static files (CSS/logo) not loading**: confirm the `wwwroot` folder was uploaded/extracted
  fully, not just the root files.

## Re-deploying after future code changes

Whenever you change the code:
```bash
dotnet publish -c Release -o publish
```
then re-upload the contents of the `publish` folder the same way (Step 4). You do **not**
need to redo Steps 1–3 or re-enter the connection string — `appsettings.Production.json`
on the server will keep your saved values as long as you don't overwrite it with the
placeholder version from a fresh publish.
