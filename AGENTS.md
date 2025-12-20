# Repository Guidelines

## Project Structure & Module Organization
- `DotVerter/` MAUI app entry point, DI setup (`MauiProgram.cs`), app resources (`Resources/`), and platform-specific code under `Platforms/`.
- `UI/` UI layer with `Views/`, `ViewModels/`, and UI models.
- `Domain/` domain models and interfaces.
- `Data/` DTOs, database entities (`Data/Database`), repositories, and API services.
- `DotVerter.slnx` solution file.

## Build, Test, and Development Commands
- `dotnet build DotVerter.slnx` builds all projects.
- `dotnet run --project DotVerter/DotVerter.csproj -f net10.0-windows10.0.19041.0` runs the Windows target.
- `dotnet build DotVerter/DotVerter.csproj -f net10.0-android` builds the Android target (MAUI workload required).
- `dotnet clean DotVerter.slnx` cleans outputs.

## Coding Style & Naming Conventions
- C# uses 4-space indentation, braces on new lines, `nullable` and `implicit usings` enabled.
- Public types and members use `PascalCase`; locals and parameters use `camelCase`.
- Interfaces use `I` prefixes (`ICurrencyRepository`), view models are `*ViewModel`, and pages are `*Page.xaml`.
- Keep XAML bindings typed with `x:DataType` as in existing views.

## Testing Guidelines
- No automated test projects are present yet.
- If you add tests, create a `*.Tests` project and name test classes `*Tests`.
- Run tests with `dotnet test` once a test project exists.

## Commit & Pull Request Guidelines
- Git history uses short, lowercase summaries without prefixes (e.g., `remove logging`); follow that style.
- PRs should include a concise summary, testing notes, and screenshots for UI changes, plus linked issues when applicable.

## Configuration & Data Notes
- The app creates a local SQLite database `currencies.db` in the MAUI app data directory; do not commit generated data.
- Rates are fetched from `https://www.cbr-xml-daily.ru/`; ensure network access for live runs.
