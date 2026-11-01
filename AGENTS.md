# Repository Guidelines

## Project Structure & Module Organization

`Backend/` contains the .NET solution (`IIROSA.sln`) and projects under `Backend/src/`: `IIROSA.Domain`, `IIROSA.Application`, `IIROSA.Infrastructure`, and `IIROSA.Api`. Shared framework projects are in `Backend/Framework/`. Angular 18 code is in `Frontend/src/`; organize features in `Frontend/src/app/modules/`, shared UI in `shared/`, and static/localized assets in `Frontend/src/assets/`. `UseCases/` and `Architecture/` hold functional and design references. There is no root-level test project directory; keep automated tests alongside their owning stack if adding them.

## Build, Test, and Development Commands

Run commands from the relevant project directory:

- `cd Backend; dotnet restore IIROSA.sln` restores .NET dependencies.
- `cd Backend; dotnet build IIROSA.sln` builds the solution.
- `cd Backend; dotnet run --project src/IIROSA.Api` starts the API.
- `cd Frontend; npm install` installs Angular dependencies.
- `cd Frontend; npm start` serves the app with its API proxy; `npm run build` creates a production build.
- `cd Frontend; npm test` runs Angular tests; `npm run lint` runs the configured lint script.

Configure local database and service settings outside committed source. Check `Backend/README.md` and `Frontend/README.md` for setup details.

## Coding Style & Naming Conventions

Follow existing .NET conventions: four-space indentation, PascalCase for public types and members, and descriptive async method names ending in `Async`. Keep layers focused and avoid moving persistence concerns into domain or API code. In Angular/TypeScript, use two-space indentation, kebab-case feature filenames, and the established component/service naming patterns. Keep translations in the existing locale files and support both English and Arabic RTL layouts.

## Testing Guidelines

Add tests for business rules and changed UI behavior. Use the existing .NET/Angular test tooling and name tests after the behavior they cover (for example, `CreateFamily_ReturnsValidationError`). Run the relevant stack's test command before submitting; include any manual verification steps when automation does not cover the change.

## Commit & Pull Request Guidelines

Recent commits use terse, inconsistent subjects. Prefer imperative, specific subjects such as `Add family payment validation` or `Fix Arabic charity filters`. A pull request should explain the behavior change, link related work, note configuration or migration impact, and include screenshots for visible UI changes. Record the build and test commands run.

## Security & Configuration

Never commit credentials, JWT keys, production connection strings, or personal data. Use local configuration or environment-specific secret storage, and review changes to authorization, uploads, and audit logging carefully.
