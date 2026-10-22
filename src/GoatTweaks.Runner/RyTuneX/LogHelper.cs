using System.Runtime.CompilerServices;
using GoatTweaks.Runner.Logging;

namespace RyTuneX.Helpers;

internal static class LogHelper
{
    public static Task Log(
        string message,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "") =>
        ActionLog.WriteUpstreamAsync("Information", message, caller, file);

    public static Task LogError(
        string message,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "") =>
        ActionLog.WriteUpstreamAsync("Error", message, caller, file);
}
