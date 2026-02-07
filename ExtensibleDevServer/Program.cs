using System.Diagnostics;
using System.Text.RegularExpressions;
using CommandLineSwitchParser;

var leftArgs = args.TakeWhile(a => a != "--").ToArray();
var rightArgs = args.SkipWhile(a => a != "--").Skip(1).ToArray();
var options = CommandLineSwitch.Parse<CommandLineOptions>(ref leftArgs);

// Parse response file
var currentSection = "";
var responseFileEntries = new Dictionary<string, List<string>>();
if (File.Exists(options.ResponseFilePath))
{
    foreach (var line in File.ReadAllLines(options.ResponseFilePath))
    {
        var trimmedLine = line.Trim();
        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#")) continue;

        var matchSection = Regex.Match(trimmedLine, @"^\[(?<name>[^\]]*)\]$");
        if (matchSection.Success)
        {
            currentSection = matchSection.Groups["name"].Value;
            if (!responseFileEntries.ContainsKey(currentSection)) responseFileEntries[currentSection] = [];
            continue;
        }

        if (string.IsNullOrEmpty(currentSection)) continue;

        responseFileEntries[currentSection].Add(trimmedLine);
    }
}

var delimitter = OperatingSystem.IsWindows() ? ';' : ':';

var serverStartInfo = new ProcessStartInfo
{
    FileName = "dotnet",
    UseShellExecute = false,
};

// Set environment variables for the development server process
ConfigureEnvironmentVariable(serverStartInfo, responseFileEntries, "HostingStartupAssemblies", "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", ';');
ConfigureEnvironmentVariable(serverStartInfo, responseFileEntries, "AdditionalDeps", "DOTNET_ADDITIONAL_DEPS", delimitter);

responseFileEntries.TryGetValue("EnvironmentVariables", out var envValues);
foreach (var envVar in envValues ?? [])
{
    var separatorIndex = envVar.IndexOf('=');
    if (separatorIndex <= 0) continue;
    var name = envVar[..separatorIndex];
    var value = envVar[(separatorIndex + 1)..];
    serverStartInfo.Environment[name] = value;
}

// Set up the arguments to start the Blazor development server
serverStartInfo.ArgumentList.Add("exec");

serverStartInfo.ArgumentList.Add("--additionalprobingpath");
responseFileEntries.TryGetValue("AdditionalProbingPaths", out var additionalProbingPaths);
serverStartInfo.ArgumentList.Add(string.Join(delimitter, additionalProbingPaths ?? []));

serverStartInfo.ArgumentList.Add(options.DevServerPath ?? throw new InvalidOperationException("DevServerPath is required."));
foreach (var arg in rightArgs) serverStartInfo.ArgumentList.Add(arg);

// Start the authenticated Blazor development server process
var serverProcess = Process.Start(serverStartInfo);
if (serverProcess is null)
{
    Console.Error.WriteLine("Failed to start the development server process.");
    return -1;
}

// Start the process watcher
Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    UseShellExecute = false,
    ArgumentList =
    {
        "exec",
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "process-watcher.dll"),
        Environment.ProcessId.ToString(),
        serverProcess.Id.ToString()
    }
});

// Wait for the development server process to exit and return its exit code
serverProcess.WaitForExit();
return serverProcess.ExitCode;

static void ConfigureEnvironmentVariable(ProcessStartInfo startInfo, Dictionary<string, List<string>> entries, string key, string variableName, char delimitter)
{
    var values = entries.TryGetValue(key, out var v) ? v : [];
    var prevValue = Environment.GetEnvironmentVariable(variableName);
    if (!string.IsNullOrEmpty(prevValue)) values = [.. values.Prepend(prevValue.TrimEnd(delimitter))];
    startInfo.Environment[variableName] = string.Join(delimitter, values);
}