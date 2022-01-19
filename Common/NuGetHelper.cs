using System;
using System.IO;
using Common.Extensions;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common
{
    public static class NuGetHelper
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(NuGetHelper));

        public static string GetNugetPackageVersion(string packageName, string nugetRunCommand, bool preRelease)
        {
            var shellRunner = ShellRunnerFactory.Create();
            ConsoleWriter.WriteProgressWithoutSave("Get package version for " + packageName);

            shellRunner.Run($"{nugetRunCommand} list {packageName} -NonInteractive" + (preRelease ? " -PreRelease" : ""));
            foreach (var line in shellRunner.Output.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var lineTokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (lineTokens.Length == 2 && lineTokens[0].Equals(packageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var msg = $"Got package version: {lineTokens[1]} for {packageName}";
                    Log.LogInformation(msg);
                    ConsoleWriter.WriteInfo(msg);
                    return lineTokens[1];
                }
            }

            var message = $"not found package version for package {packageName}. nuget output: " + shellRunner.Output + shellRunner.Errors;
            Log.LogDebug(message);
            ConsoleWriter.WriteWarning(message);
            return null;
        }

        public static string GetNugetRunCommand()
        {
            var nuGetPath = GetNuGetPath();
            if (nuGetPath == null)
                return null;
            var nugetCommand = $"\"{nuGetPath}\"";
            if (Helper.OsIsUnix())
                nugetCommand = $"mono {nugetCommand}";
            return nugetCommand;
        }

        private static string GetNuGetPath()
        {
            var nuget = Path.Combine(Helper.GetCementInstallDirectory(), "dotnet", "NuGet.exe");
            if (!File.Exists(nuget))
            {
                Log.LogError($"NuGet.exe not found in {nuget}");
                return null;
            }

            Log.LogDebug($"NuGet.exe found in {nuget}");
            return nuget;
        }
    }
}
