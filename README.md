# Plant Companion

A .NET MAUI app for managing plants, crop beds, care reminders, and community messages. It also includes an ML.NET plant-photo identification screen.

## Build

Open `FinalYearProject.sln` in Visual Studio with the .NET MAUI workload installed, or restore and build the Windows target with:

```powershell
dotnet restore FinalYearProject.csproj -p:TargetFrameworks=net8.0-windows10.0.19041.0
dotnet build FinalYearProject.csproj -f net8.0-windows10.0.19041.0 --no-restore -p:TargetFrameworks=net8.0-windows10.0.19041.0
```

The current project targets .NET 8. Plan a framework and MAUI upgrade before .NET 8 support ends.

## Data and integrations

- User, plant, care-task, forum, and crop-bed data is stored locally in SQLite in the app's local data directory.
- Current weather is fetched from Open-Meteo for London; an internet connection is required.
- Plant identification requires the trained `MLModel1.mlnet` artifact in the project. The source repository does not include that artifact, so the scanner will report that identification is unavailable until the model is supplied.
- Admin sign-in is intentionally disabled until server-side authentication is available. Do not add administrator credentials or prediction keys to the MAUI client.

If a prediction key was previously committed to GitHub, revoke it at its provider even after removing it from the source.