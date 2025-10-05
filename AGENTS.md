# Repository Guidelines

This pet project explores modern .NET, EF Core, Blazor WebAssembly, SignalR, and Kubernetes via Minikube; contributions that align with current best practices and deepen understanding of these stacks are welcome.

## Project Structure & Module Organization
`voting_app.sln` wires together the Blazor client (`VotingApp/`), ASP.NET Core API (`VotingApp.Server/`), worker service (`Worker/`), and shared DTO libraries (`*.Shared`). Entity models and requests live in the shared projects to keep EF Core mappings consistent. Real-time results stream through the `VotingResults.*` trio over SignalR. Infrastructure artifacts, including Minikube-ready manifests and secrets templates, sit in `k8s/`, while the worker seeds sample data from `Worker/DB/`.

## Build, Test, and Development Commands
Run `dotnet restore` once after cloning, then `dotnet build voting_app.sln` to validate the solution. Use `dotnet watch --project VotingApp.Server` for API hot reload and `dotnet watch --project VotingApp` when iterating on Razor UI changes. Bring services up locally with `dotnet run --project VotingApp.Server`, `dotnet run --project VotingResults.Server`, and `dotnet run --project Worker`. No Node.js tooling is required.

## Coding Style & Naming Conventions
Respect `.editorconfig`: two-space indentation for C#, 130-character line limit, and trimmed trailing whitespace. Default to file-scoped namespaces, expression-bodied members, and `var` for local declarations. Name files after their primary type, keep Razor components PascalCase, and store cross-project contracts in the shared libraries. Format updates with `dotnet format` before opening a pull request.

## Testing Guidelines
Author unit or integration tests only when maintainers explicitly request them. If asked, create a sibling project (e.g., `VotingApp.Server.Tests`) with `dotnet new xunit -n VotingApp.Server.Tests`, add it to `voting_app.sln`, and mirror the production namespace layout. Keep Arrange/Act/Assert sections clear and target SignalR hubs, EF Core data access, or worker scheduling logic as directed. Execute suites with `dotnet test` and report coverage gaps in the pull request description.

## Commit & Pull Request Guidelines
Follow the established conventional commits (`feat:`, `fix:`, `refactor:`) with imperative descriptions. Keep commits focused, reference related issues, and avoid mixing refactors with behavioral changes. Pull requests should summarize intent, list manual verification steps, and attach UI screenshots or hub output when behavior changes. Highlight recommendations that modernize the stack—experimentation is part of the project’s purpose.

## Configuration & Deployment Notes
Each service reads configuration from its `appsettings*.json`; store secrets in environment variables or Kubernetes secrets such as `k8s/db-secret.yaml`. Dockerfiles at the project roots support container builds (`docker build -f VotingApp.Server/Dockerfile .`). For Kubernetes trials, use Minikube (`minikube start`, `minikube kubectl -- apply -f k8s/`). When adjusting infrastructure, keep the manifests in sync and document required toggles or migrations in the PR discussion.
