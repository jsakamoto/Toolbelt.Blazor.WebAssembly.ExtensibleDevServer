---
name: follow-upstream-release
description: Use when the user asks to update this repository to a new release of the underlying Microsoft.AspNetCore.Components.WebAssembly.DevServer package, or to bump this package's own version to match — including vague prompts like "the upstream released a new version, follow up", "the stock DevServer released a new version, catch up", or "bump to the latest DevServer version".
---

# Following a new Microsoft.AspNetCore.Components.WebAssembly.DevServer release

This package (`Toolbelt.Blazor.WebAssembly.ExtensibleDevServer`) wraps the stock
`Microsoft.AspNetCore.Components.WebAssembly.DevServer` package and launches it as a child process.
Historically, this repository's own package version has been kept identical to the stock DevServer
version it wraps (e.g. stock `10.0.12` → this package also `10.0.12`), even for a pure version bump
with no other change. Follow that convention unless the user says otherwise.

## Steps

1. Check which versions actually exist before touching anything:
   ```
   curl -s https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.components.webassembly.devserver/index.json
   ```
2. Update the version in these files:
   - `VersionInfo.props` — the `<Version>` property (this repo's own package version).
   - `ExtensibleDevServer/ExtensibleDevServer.csproj` — the `Microsoft.AspNetCore.Components.WebAssembly.DevServer` `PackageReference`.
   - `SampleApp/SampleApp.csproj` — three places: the `Microsoft.AspNetCore.Components.WebAssembly`
     reference, the commented-out `Microsoft.AspNetCore.Components.WebAssembly.DevServer` reference
     (kept only as a documented example, still worth keeping in sync), and this repo's own
     `Toolbelt.Blazor.WebAssembly.ExtensibleDevServer` reference (the sample app dogfoods its own
     package).
3. Add a new entry at the top of `RELEASE-NOTES.txt` (see the `release-notes` skill for the house
   style), e.g.:
   ```
   v.X.Y.Z
   - Improve: Updated the underlying Microsoft.AspNetCore.Components.WebAssembly.DevServer dependency to version X.Y.Z.
   ```
4. Raise the E2E test container's SDK image to the latest .NET 10 SDK, every time, without waiting
   for a failure. `ExtensibleDevServer.Test/ProjectTemplateE2ETests.cs` and
   `ExtensibleDevServer.Test/SampleAppE2ETests.cs` each pin an exact
   `mcr.microsoft.com/dotnet/sdk:X.Y.Z` tag. Find the newest published one:
   ```
   curl -s https://mcr.microsoft.com/v2/dotnet/sdk/tags/list | tr ',' '\n' | tr -d '"[] ' | grep -E '^10\.0\.[0-9]+$' | sort -V | tail -1
   ```
   - Set both files to that tag, then `docker pull mcr.microsoft.com/dotnet/sdk:<tag>`.
   - The tag must never be older than the SDK that builds the package locally (`dotnet --version`);
     otherwise the container fails at runtime with "You must install or update .NET to run this
     application."
   - This is a separate axis from the DevServer package version; don't assume bumping one bumps the
     other.
5. Update the NuGet packages referenced by `ExtensibleDevServer.Test/ExtensibleDevServer.Test.csproj`
   to the latest stable versions available at that moment (see the `dotnet-test-projects` skill):
   ```
   dotnet list ./ExtensibleDevServer.Test/ExtensibleDevServer.Test.csproj package --outdated
   ```
   - Bump every `PackageReference` it reports, including `NUnit`, `NUnit3TestAdapter`,
     `Microsoft.NET.Test.Sdk`, `Testcontainers`, `ChainingAssertion-NUnit.Bin`, `XProcess`, and the
     `Toolbelt.*` packages. If the command cannot reach a package, check the nuget.org flat-container
     index JSON instead.
   - Do not touch the stock DevServer reference here; step 2 owns it.
   - If a bump needs a test code change, or a package has no stable release for the new setup, report
     it rather than silently swapping in a replacement.
6. Build and test:
   ```
   dotnet pack -c Release
   dotnet test --project ./ExtensibleDevServer.Test/ExtensibleDevServer.Test.csproj --output detailed
   ```
   Confirm every test passes before considering the bump done.

## After this repository is done

The `Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.UserSecretsExtension` and
`...ImportMapExtension` repositories (siblings under `NET10/Extensions/`) don't literally depend on
the stock DevServer, but their own package versions are conventionally realigned to match this
repository's new version too, purely so users aren't confused by mismatched version numbers across
the package family. Each of those repositories has its own `follow-upstream-release` skill for that
half of the work — run this repository's bump and tests first, then theirs, in that order.

If an extension's test needs an `ExtensibleDevServer` version that isn't published on nuget.org yet,
they read it from a manually-populated `_local-packages` folder instead — see each extension's
`AGENTS.md` for that mechanism. After packing this repository (step 6 above), copy the resulting
`.nupkg` from this repo's `_dist/` folder into the other repo's `_local-packages/` folder.

## Gotchas

- Files in this repository are stored as LF line endings in git; a normal checkout converts them to
  CRLF locally (`core.autocrlf=true`). Editing with a POSIX tool (`sed`, a bash heredoc) leaves LF
  and git will warn "LF will be replaced by CRLF" on the next `git add` — harmless, but if you want
  the working tree byte-identical to what a fresh checkout would produce, re-normalize the edited
  file's line endings to CRLF before committing.
- Don't hand-edit generated `*.deps.json` files if you find one; check whether an MSBuild target
  (search for `GenerateDepsJson`) regenerates it from `$(Version)` on build — if so, just build once
  and let it rewrite the checked-in copy.
