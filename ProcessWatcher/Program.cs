using System.Diagnostics;

var driverProcess = GetProcessById(args.ElementAtOrDefault(0));
var serverProcess = GetProcessById(args.ElementAtOrDefault(1));

driverProcess?.WaitForExit();
if (serverProcess?.HasExited != true) serverProcess?.Kill();

static Process? GetProcessById(string? idString)
{
    if (!int.TryParse(idString, out int id)) return null;
    try { return Process.GetProcessById(id); }
    catch (ArgumentException) { return null; }
}
