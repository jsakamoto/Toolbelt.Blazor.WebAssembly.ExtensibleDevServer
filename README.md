# Blazor WebAssembly Extensible Dev Server

[![tests](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer/actions/workflows/tests.yml/badge.svg)](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer/actions/workflows/tests.yml) [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer/) [![Discord](https://img.shields.io/discord/798312431893348414?style=flat&logo=discord&logoColor=white&label=Blazor%20Community&labelColor=5865f2&color=gray)](https://discord.com/channels/798312431893348414/1202165955900473375)


An alternative Blazor WebAssembly dev server that can be extended with additional NuGet packages for custom middleware.

## What is this?

In a standalone Blazor WebAssembly project, the development server is provided by the `Microsoft.AspNetCore.Components.WebAssembly.DevServer` NuGet package. This package works well out of the box, but it does not offer any way to customize or extend the dev server's behavior.

**Toolbelt.Blazor.WebAssembly.ExtensibleDevServer** is a drop-in replacement for that default dev server. By itself, simply replacing the default package with this one does not change any behavior. Your project will continue to work exactly as before.

The key difference is that this dev server is designed to be **extensible**. It leverages the ASP.NET Core **Hosting Startup** mechanism, allowing additional NuGet packages to inject custom middleware into the dev server pipeline. This means you can install extension packages alongside this one to add new capabilities to your development experience.

### Examples of possible extensions

- **User Secrets integration**: Merge .NET User Secrets into the `appsettings.json` response served to the client, so secret configuration values are available during development without checking them into source control.
- **CSP hash rewriting**: Automatically update Content Security Policy hash values in `index.html` to match the actual content, eliminating manual hash maintenance during development.

## How to use

### 1. Replace the default dev server package

Remove the default dev server package from your Blazor WebAssembly project and add this one instead.

**Using the .NET CLI:**

```shell
dotnet remove package Microsoft.AspNetCore.Components.WebAssembly.DevServer
dotnet add package Toolbelt.Blazor.WebAssembly.ExtensibleDevServer
```

**Or, edit your project file (`.csproj`) directly:**

Find the following `PackageReference` in your `.csproj` file:

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="..." />
```

and replace it with:

```xml
<PackageReference Include="Toolbelt.Blazor.WebAssembly.ExtensibleDevServer" Version="10.0.3" PrivateAssets="all" />
```

### 2. Install extension packages

Then, add any extension packages you need. For example:

```shell
dotnet add package <ExtensionPackageName>
```

That's it. No additional code or configuration is required. The extensions will be automatically loaded by the hosting startup mechanism when you run your project.

## Creating your own extension

You can create your own extension to add custom middleware to the dev server pipeline.

### 1. Install the project template

First, install the extension project template:

```shell
dotnet new install Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.Extension.ProjectTemplates
```

### 2. Create a new extension project

Create a new extension project using one of the following methods:

**Using Visual Studio or VS Code with C# Dev Kit:**

Select "Blazor WebAssembly Extensible Dev Server Extension" from the new project templates.

**Or, using the .NET CLI:**

```shell
dotnet new blazorwasmdevserverextension -n {YourExtensionName}
```

### 3. Implement your middleware

Open the generated `{YourExtensionName}StartupFilter.cs` file. You'll find a minimal ASP.NET Core middleware implementation that does nothing by default. Modify this to add your desired behavior.

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace YourExtensionName;

public class YourExtensionNameStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        // Return a registration action that adds middleware to the pipeline
        return app =>
        {
            // Example middleware that does nothing but call the next middleware
            app.Use(async (context, nextMiddleware) =>
            {
                // You can add custom logic here before the next middleware is invoked

                await nextMiddleware();

                // You can add custom logic here after the next middleware has completed
            });

            // Call the next startup filter in the chain
            next(app);
        };
    }
}
```

### 4. Build the NuGet package

Package your extension using one of the following methods:

**Using the .NET CLI:**

```shell
dotnet pack -c Release
```

**Or, using Visual Studio:**

Right-click the project in Solution Explorer and select "Pack".

This will generate a `{YourExtensionName}.nupkg` file.

### 5. Use your extension

Install your generated NuGet package into a Blazor WebAssembly standalone project alongside `Toolbelt.Blazor.WebAssembly.ExtensibleDevServer`. When you run the project, your custom middleware will be automatically loaded into the dev server's HTTP request pipeline.

## License and 3rd Party Notices

This project is licensed under the Mozilla Public License v2.0. See the [LICENSE](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer/blob/main/LICENSE) file for details.

This project includes third-party components. See the [THIRD-PARTY-NOTICES](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer/blob/main/THIRD-PARTY-NOTICES.txt) file for details.