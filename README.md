# Plant Companion

A .NET MAUI app for managing plants, care reminders and a gardening community, backed by an ASP.NET Core API.

## Projects

| Path | What it is |
|---|---|
| `FinalYearProject.csproj` | MAUI client (Windows verified; Android/iOS not yet built) |
| `server/PlantApp.Api` | REST API: JWT auth, plants, weather-aware watering schedule, forum with replies/reports, admin moderation, Pl@ntNet proxy |
| `server/PlantApp.Api.Tests` | xUnit integration and unit tests (13 tests) |

## Run the client (Windows)

```powershell
dotnet build FinalYearProject.csproj -p:TargetFrameworks=net8.0-windows10.0.19041.0 -p:Platform=x64
.\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\FinalYearProject.exe
```

The Windows build is self-contained, so no separate Windows App Runtime install is needed. Do not build `FinalYearProject.sln`; it references a project that is not in the repo.

## Run the API

Requires the .NET 10 SDK.

```powershell
cd server\PlantApp.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<random string of 32+ characters>"
dotnet user-secrets set "PlantNet:ApiKey" "<your free key from my.plantnet.org>"
dotnet run
```

In production set `Jwt__Key`, `PlantNet__ApiKey` and `ConnectionStrings__Default` as environment variables. The API refuses to start in production without a signing key. Health check: `GET /health`.

### Endpoints

- `POST /api/auth/register`, `POST /api/auth/login`
- `GET/POST /api/plants`, `POST /api/plants/{id}/water`, `DELETE /api/plants/{id}`
- `GET/POST /api/forum`, `POST /api/forum/{id}/replies`, `POST /api/forum/{id}/report`
- `GET /api/admin/reports`, `GET /api/admin/users`, `POST /api/admin/posts/{id}/hide` (Admin role)
- `POST /api/identify` (multipart `image`; key stays on the server)

## Connect the client to the API

The client defaults to `http://localhost:5000` (`http://10.0.2.2:5000` on the Android emulator). Sign-in, registration, the forum (posts, replies, reports) and plant identification use the API when it is reachable, and fall back to on-device data when it is not. Override the address with the `api_base_url` preference.

## Test

```powershell
dotnet test server\PlantApp.Api.Tests
```

## Status and roadmap

Done: secure API with tests, server-side auth and moderation, key-safe identification proxy, weather-adaptive care scheduling, five-green UI.

Not done yet:
- The plants and care-task screens still use local SQLite data; only sign-in, registration, forum and identification call the API.
- Push notifications, photo growth journal, plant detail/care guides, offline sync.
- Android and iOS builds, store packaging and a Windows installer.
- The API has no admin-creation flow yet (promote a user by setting `Role = "Admin"` in the database).

## Security

If a prediction key was previously committed, revoke it at its provider (Azure); removing it from the code does not remove it from Git history. Never put keys in the MAUI client. See [PRIVACY.md](PRIVACY.md) for the draft privacy policy.
