# Entra ID setup

Local accounts work without any of this. Follow these steps to let Entra ID users sign in and to manage them from the
journal system.

Values for this installation (not secrets):

| Setting | Value |
| --- | --- |
| Tenant id (`AzureAd:TenantId`) | `d603805b-1934-4b76-9f94-6587640f80bf` |
| Application (client) id (`AzureAd:ClientId`) | `53367cbc-d34c-4a13-af71-83ede93d6b24` |
| Application ID URI (`AzureAd:Audience`) | `api://53367cbc-d34c-4a13-af71-83ede93d6b24` |

## 1. App registration (Azure portal, Entra ID, App registrations, "Dotnetstore JournalSystem API")

1. **Expose an API**: set the Application ID URI to `api://53367cbc-d34c-4a13-af71-83ede93d6b24`, then add a scope
   `access_as_user` ("Access the journal system as the signed-in user", admins and users can consent).
2. **API permissions**: add Microsoft Graph, *Delegated*, `User.ReadWrite.All`, then **Grant admin consent**.
   The permission is delegated: Graph calls run as the signed-in admin, so that admin also needs a directory role that
   may manage users (for example *User Administrator*).
3. **Manifest**: make sure `requestedAccessTokenVersion` is `2` (under `api`), so tokens use the v2.0 issuer.
4. **Certificates & secrets**: create a client secret (or upload a certificate). Copy the secret **value**
   immediately; it is shown only once.
5. If your web UI is a separate app registration, add this API's `access_as_user` scope to its permissions and pre-authorize
   it under *Expose an API*.

## 2. Secrets (never commit these)

```
dotnet user-secrets set "AzureAd:ClientCredentials:0:SourceType" "ClientSecret" --project src/Dotnetstore.JournalSystem.Host
dotnet user-secrets set "AzureAd:ClientCredentials:0:ClientSecret" "<secret value>" --project src/Dotnetstore.JournalSystem.Host
```

In production use a certificate or a managed identity plus a secret store (Key Vault) instead of a plain secret.

## 3. First Entra administrator

The bootstrap administrator can be linked to an Entra account, because Entra users are matched on their object id.
Find your **Object ID** under Entra ID, Users, your user. Do not use the app registration's object id.

```
dotnet user-secrets set "Bootstrap:Admin:Username" "<your email>" --project src/Dotnetstore.JournalSystem.Host
dotnet user-secrets set "Bootstrap:Admin:EntraObjectId" "<your user object id>" --project src/Dotnetstore.JournalSystem.Host
```

The account is created at startup when `Database:MigrateOnStartup` is on, a system own company has been seeded,
and no user with that user name exists yet. It is safe to leave configured: later startups skip it.

## 4. Turn it on

```
dotnet user-secrets set "Authentication:Entra:Enabled" "true" --project src/Dotnetstore.JournalSystem.Host
```

If a setting is missing the host refuses to start and names it.

## 5. Check it works

1. Acquire a token for scope `api://53367cbc-d34c-4a13-af71-83ede93d6b24/access_as_user` as the administrator.
2. `GET /api/organization/users` with `Authorization: Bearer <token>` returns 200.
3. A signed-in Entra user that is not in the journal system's user list gets 403 on that endpoint.
4. Creating a user with `authProvider: 1` (Entra) creates the account in Entra and returns a one-time
   `temporaryPassword`; the user must change it at first sign-in. The user name must be in a domain verified in the tenant.

## 6. "Sign in with Microsoft" in the web UI

The web UI (`FrontEnd.WebUi`) signs people in with OpenID Connect (authorization code flow with PKCE) against the same app
registration, and asks for the `access_as_user` scope so the access token it gets is one the API accepts. The token is kept in
the session cookie, and the session ends when the token expires (about an hour).

In the Azure portal, under the app registration:

1. **Authentication**, **Add a platform**, **Web**: add the redirect URI `https://localhost:7203/signin-oidc`
   (and the same path on any other address the UI is served from). Register the plain `http://localhost:5135/signin-oidc`
   too only if you run the UI over http.
2. The client secret from section 1 is reused. Give it to the web UI as well:

```
dotnet user-secrets set "AzureAd:ClientCredentials:0:ClientSecret" "<secret value>" --project src/Dotnetstore.JournalSystem.FrontEnd.WebUi
dotnet user-secrets set "Authentication:Entra:Enabled" "true" --project src/Dotnetstore.JournalSystem.FrontEnd.WebUi
```

Tenant id and client id are in the web UI's `appsettings.json`. With the switch on and a setting missing, the UI refuses to
start and names it. With the switch off, the button is hidden and the page `/Account/ExternalLogin` returns 404.

### Linking an existing local administrator to your Microsoft account

A person has one account. Give the bootstrap administrator **both** a password and the Entra object id (section 3) and the
account works both ways:

```
dotnet user-secrets set "Bootstrap:Admin:Password" "<password>" --project src/Dotnetstore.JournalSystem.Host
dotnet user-secrets set "Bootstrap:Admin:EntraObjectId" "<your user object id>" --project src/Dotnetstore.JournalSystem.Host
```

- If the user does not exist yet it is created as a local account that is also linked to Entra.
- If it already exists and has no link, startup links it (it never changes the password and never replaces an existing link,
  and it refuses an object id that belongs to another user).
- The link is made on the object id only. Nothing is matched on e-mail or user name.

Without a password, the bootstrap administrator is an Entra-only account that cannot sign in with a password.

### What is and is not covered by automated tests

The browser tests (Playwright) run the whole sign-in against a small fake OpenID provider that the test fixture starts: the
redirect, the code exchange with PKCE, id-token validation, the cookie, the dashboard, a cancelled sign-in, sign-out and
open-redirect protection. Real Entra ID, consent and MFA are not exercised and must be tried by hand once.

Known limits: signing out clears the UI session only (the Microsoft session stays), there is no silent token refresh, and
a Microsoft user who is not in the journal system's user list can reach the dashboard shell but gets no data from the API.

## How it fits together

- Tokens whose issuer is the local issuer are validated as local tokens; every other bearer token is validated by Entra.
- Roles come from the journal system's own `Role` table, never from the Entra token.
- Deleting an Entra user in the journal system deletes it in Entra first, then removes the local record.
