using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RyTuneX.Helpers;

internal static partial class OptimizationOptions
{
internal static async Task<int> StartInCmd(string command)
    {
        try
        {
            var cmdPath = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysNative", "cmd.exe")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmdPath,
                    Arguments = $"/C {command}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true
                }
            };

            process.Start();

            var stdErrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync().ConfigureAwait(false);

            var errorOutput = await stdErrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                _ = LogHelper.LogError($"Command failed (exit {process.ExitCode})\n{errorOutput}");
            }

            return process.ExitCode;
        }
        catch (Exception ex)
        {
            _ = LogHelper.LogError($"Error running command: {ex}");
            throw;
        }
    }

internal static async Task ExecuteToggleActionAsync(string? tag, bool isOn, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(tag)) return;

        _ = LogHelper.Log($"Executing optimization: {tag} = {(isOn ? "ON" : "OFF")}");

        // Provide a cancellable wrapper around each action and centralize switch
        switch (tag)
        {
            case "RecommendedSectionStartMenu":
                if (isOn) await OptimizeSystemHelper.DisableRecommendedSectionStartMenu().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableRecommendedSectionStartMenu().ConfigureAwait(false);
                break;

            case "LegacyBootMenu":
                if (isOn) await OptimizeSystemHelper.EnableLegacyBootMenu().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableLegacyBootMenu().ConfigureAwait(false);
                break;

            case "OptimizeNTFS":
                if (isOn) await OptimizeSystemHelper.EnableOptimizeNTFS().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableOptimizeNTFS().ConfigureAwait(false);
                break;

            case "PrioritizeForegroundApplications":
                if (isOn) await OptimizeSystemHelper.EnablePrioritizeForegroundApplications().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisablePrioritizeForegroundApplications().ConfigureAwait(false);
                break;

            case "WPBT":
                if (isOn) await OptimizeSystemHelper.DisableWPBT().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWPBT().ConfigureAwait(false);
                break;

            case "ServiceHostSplitting":
                if (isOn) await OptimizeSystemHelper.DisableServiceHostSplitting().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableServiceHostSplitting().ConfigureAwait(false);
                break;

            case "MenuShowDelay":
                if (isOn) await OptimizeSystemHelper.DisableMenuShowDelay().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableMenuShowDelay().ConfigureAwait(false);
                break;

            case "MouseHoverTime":
                if (isOn) await OptimizeSystemHelper.DisableMouseHoverTime().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableMouseHoverTime().ConfigureAwait(false);
                break;

            case "KeyboardLatency":
                if (isOn) await OptimizeSystemHelper.EnableKeyboardLatencyOptimization().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableKeyboardLatencyOptimization().ConfigureAwait(false);
                break;

            case "MouseAcceleration":
                if (isOn) await OptimizeSystemHelper.DisableMouseAcceleration().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableMouseAcceleration().ConfigureAwait(false);
                break;

            case "BackgroundApps":
                if (isOn) await OptimizeSystemHelper.DisableBackgroundApps().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableBackgroundApps().ConfigureAwait(false);
                break;

            case "AutoComplete":
                if (isOn) await OptimizeSystemHelper.DisableAutoComplete().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableAutoComplete().ConfigureAwait(false);
                break;

            case "CrashDump":
                if (isOn) await OptimizeSystemHelper.EnableCrashDump().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableCrashDump().ConfigureAwait(false);
                break;

            case "RemoteAssistance":
                if (isOn) await OptimizeSystemHelper.DisableRemoteAssistance().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableRemoteAssistance().ConfigureAwait(false);
                break;

            case "WindowShake":
                if (isOn) await OptimizeSystemHelper.DisableWindowShake().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWindowShake().ConfigureAwait(false);
                break;

            case "CopyMoveContextMenu":
                if (isOn) await OptimizeSystemHelper.AddCopyMoveContextMenu().ConfigureAwait(false);
                else await OptimizeSystemHelper.RemoveCopyMoveContextMenu().ConfigureAwait(false);
                break;

            case "TaskTimeouts":
                if (isOn) await OptimizeSystemHelper.AdjustTaskTimeouts().ConfigureAwait(false);
                else await OptimizeSystemHelper.IncreaseTaskTimeouts().ConfigureAwait(false);
                break;

            case "LowDiskSpaceChecks":
                if (isOn) await OptimizeSystemHelper.DisableLowDiskSpaceChecks().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableLowDiskSpaceChecks().ConfigureAwait(false);
                break;

            case "LinkResolve":
                if (isOn) await OptimizeSystemHelper.DisableLinkResolve().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableLinkResolve().ConfigureAwait(false);
                break;

            case "ServiceTimeouts":
                if (isOn) await OptimizeSystemHelper.DecreaseServiceTimeouts().ConfigureAwait(false);
                else await OptimizeSystemHelper.RevertServiceTimeouts().ConfigureAwait(false);
                break;

            case "RemoteRegistry":
                if (isOn) await OptimizeSystemHelper.DisableRemoteRegistry().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableRemoteRegistry().ConfigureAwait(false);
                break;

            case "FileExtensionsAndHiddenFiles":
                if (isOn) await OptimizeSystemHelper.ShowFileExtensionsAndHiddenFiles().ConfigureAwait(false);
                else await OptimizeSystemHelper.HideFileExtensionsAndHiddenFiles().ConfigureAwait(false);
                break;

            case "SystemProfile":
                if (isOn) await OptimizeSystemHelper.OptimizeSystemProfile().ConfigureAwait(false);
                else await OptimizeSystemHelper.RevertSystemProfile().ConfigureAwait(false);
                break;

            case "TelemetryServices":
                if (isOn) await OptimizeSystemHelper.DisableTelemetryServices().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableTelemetryServices().ConfigureAwait(false);
                break;

            case "HomeGroup":
                if (isOn) await OptimizeSystemHelper.DisableHomeGroup().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableHomeGroup().ConfigureAwait(false);
                break;

            case "PrintService":
                if (isOn) await OptimizeSystemHelper.DisablePrintService().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnablePrintService().ConfigureAwait(false);
                break;

            case "SysMain":
                if (isOn) await OptimizeSystemHelper.DisableSysMain().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSysMain().ConfigureAwait(false);
                break;

            case "CompatibilityAssistant":
                if (isOn) await OptimizeSystemHelper.DisableCompatibilityAssistant().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableCompatibilityAssistant().ConfigureAwait(false);
                break;

            case "SystemRestore":
                if (isOn) await OptimizeSystemHelper.DisableSystemRestore().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSystemRestore().ConfigureAwait(false);
                break;

            case "WindowsTransparency":
                if (isOn) await OptimizeSystemHelper.DisableWindowsTransparency().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWindowsTransparency().ConfigureAwait(false);
                break;

            case "WindowsDarkMode":
                if (isOn) await OptimizeSystemHelper.EnableWindowsDarkMode().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableWindowsDarkMode().ConfigureAwait(false);
                break;

            case "VerboseLogon":
                if (isOn) await OptimizeSystemHelper.EnableVerboseLogon().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableVerboseLogon().ConfigureAwait(false);
                break;

            case "ClassicContextMenu":
                if (isOn) await OptimizeSystemHelper.EnableClassicContextMenu().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableClassicContextMenu().ConfigureAwait(false);
                break;

            case "Search":
                if (isOn) await OptimizeSystemHelper.DisableSearch().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSearch().ConfigureAwait(false);
                break;

            case "Biometrics":
                if (isOn) await OptimizeSystemHelper.DisableBiometrics().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableBiometrics().ConfigureAwait(false);
                break;

            case "SMBv1":
                if (isOn) await OptimizeSystemHelper.DisableSMBAsync("1").ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSMBAsync("1").ConfigureAwait(false);
                break;

            case "SMBv2":
                if (isOn) await OptimizeSystemHelper.DisableSMBAsync("2").ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSMBAsync("2").ConfigureAwait(false);
                break;

            case "ErrorReporting":
                if (isOn) await OptimizeSystemHelper.DisableErrorReporting().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableErrorReporting().ConfigureAwait(false);
                break;

            case "Cortana":
                if (isOn) await OptimizeSystemHelper.DisableCortana().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableCortana().ConfigureAwait(false);
                break;

            case "GamingMode":
                if (isOn) await OptimizeSystemHelper.EnableGamingMode().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableGamingMode().ConfigureAwait(false);
                break;

            case "FullscreenOptimizations":
                if (isOn) await OptimizeSystemHelper.EnableFullscreenOptimizations().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableFullscreenOptimizations().ConfigureAwait(false);
                break;

            case "UsbPowerSaving":
                if (isOn) await OptimizeSystemHelper.DisableUsbPowerSaving().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableUsbPowerSaving().ConfigureAwait(false);
                break;

            case "PowerThrottling":
                if (isOn) await OptimizeSystemHelper.DisablePowerThrottling().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnablePowerThrottling().ConfigureAwait(false);
                break;

            case "GpuDriverTweaks":
                if (isOn) await OptimizeSystemHelper.ApplyGpuDriverTweaks().ConfigureAwait(false);
                else await OptimizeSystemHelper.RevertGpuDriverTweaks().ConfigureAwait(false);
                break;

            case "StoreUpdates":
                if (isOn) await OptimizeSystemHelper.DisableStoreUpdates().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableStoreUpdates().ConfigureAwait(false);
                break;

            case "OneDrive":
                if (isOn) await OptimizeSystemHelper.DisableOneDrive().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableOneDrive().ConfigureAwait(false);
                break;

            case "NewsAndInterests":
                if (isOn) await OptimizeSystemHelper.DisableNewsAndInterests().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableNewsAndInterests().ConfigureAwait(false);
                break;

            case "SpotlightFeatures":
                if (isOn) await OptimizeSystemHelper.DisableSpotlightFeatures().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSpotlightFeatures().ConfigureAwait(false);
                break;

            case "TailoredExperiences":
                if (isOn) await OptimizeSystemHelper.DisableTailoredExperiences().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableTailoredExperiences().ConfigureAwait(false);
                break;

            case "CloudOptimizedContent":
                if (isOn) await OptimizeSystemHelper.DisableCloudOptimizedContent().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableCloudOptimizedContent().ConfigureAwait(false);
                break;

            case "FeedbackNotifications":
                if (isOn) await OptimizeSystemHelper.DisableFeedbackNotifications().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableFeedbackNotifications().ConfigureAwait(false);
                break;

            case "AdvertisingID":
                if (isOn) await OptimizeSystemHelper.DisableAdvertisingID().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableAdvertisingID().ConfigureAwait(false);
                break;

            case "BluetoothAdvertising":
                if (isOn) await OptimizeSystemHelper.DisableBluetoothAdvertising().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableBluetoothAdvertising().ConfigureAwait(false);
                break;

            case "AutomaticRestartSignOn":
                if (isOn) await OptimizeSystemHelper.DisableAutomaticRestartSignOn().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableAutomaticRestartSignOn().ConfigureAwait(false);
                break;

            case "HandwritingDataSharing":
                if (isOn) await OptimizeSystemHelper.DisableHandwritingDataSharing().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableHandwritingDataSharing().ConfigureAwait(false);
                break;

            case "TextInputDataCollection":
                if (isOn) await OptimizeSystemHelper.DisableTextInputDataCollection().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableTextInputDataCollection().ConfigureAwait(false);
                break;

            case "InputPersonalization":
                if (isOn) await OptimizeSystemHelper.DisableInputPersonalization().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableInputPersonalization().ConfigureAwait(false);
                break;

            case "SafeSearchMode":
                if (isOn) await OptimizeSystemHelper.DisableSafeSearchMode().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSafeSearchMode().ConfigureAwait(false);
                break;

            case "ClipboardSync":
                if (isOn) await OptimizeSystemHelper.DisableClipboardSync().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableClipboardSync().ConfigureAwait(false);
                break;

            case "MessageSync":
                if (isOn) await OptimizeSystemHelper.DisableMessageSync().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableMessageSync().ConfigureAwait(false);
                break;

            case "SettingSync":
                if (isOn) await OptimizeSystemHelper.DisableSettingSync().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSettingSync().ConfigureAwait(false);
                break;

            case "VoiceActivation":
                if (isOn) await OptimizeSystemHelper.DisableVoiceActivation().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableVoiceActivation().ConfigureAwait(false);
                break;

            case "FindMyDevice":
                if (isOn) await OptimizeSystemHelper.DisableFindMyDevice().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableFindMyDevice().ConfigureAwait(false);
                break;

            case "Cdp":
                if (isOn) await OptimizeSystemHelper.DisableCdp().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableCdp().ConfigureAwait(false);
                break;

            case "DiagnosticsToast":
                if (isOn) await OptimizeSystemHelper.DisableDiagnosticsToast().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableDiagnosticsToast().ConfigureAwait(false);
                break;

            case "OnlineSpeechPrivacy":
                if (isOn) await OptimizeSystemHelper.DisableOnlineSpeechPrivacy().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableOnlineSpeechPrivacy().ConfigureAwait(false);
                break;

            case "LocationAccess":
                if (isOn) await OptimizeSystemHelper.DisableLocationFeatures().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableLocationFeatures().ConfigureAwait(false);
                break;

            case "LocationFeatures":
                if (isOn) await OptimizeSystemHelper.DisableLocationFeatures().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableLocationFeatures().ConfigureAwait(false);
                break;

            case "GameBar":
                if (isOn) await OptimizeSystemHelper.DisableGameBar().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableGameBar().ConfigureAwait(false);
                break;

            case "QuickAccessHistory":
                if (isOn) await OptimizeSystemHelper.DisableQuickAccessHistory().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableQuickAccessHistory().ConfigureAwait(false);
                break;

            case "MyPeople":
                if (isOn) await OptimizeSystemHelper.DisableMyPeople().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableMyPeople().ConfigureAwait(false);
                break;

            case "Drivers":
                if (isOn) await OptimizeSystemHelper.ExcludeDrivers().ConfigureAwait(false);
                else await OptimizeSystemHelper.IncludeDrivers().ConfigureAwait(false);
                break;

            case "WindowsInk":
                if (isOn) await OptimizeSystemHelper.DisableWindowsInk().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWindowsInk().ConfigureAwait(false);
                break;

            case "SpellingAndTypingFeatures":
                if (isOn) await OptimizeSystemHelper.DisableSpellingAndTypingFeatures().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSpellingAndTypingFeatures().ConfigureAwait(false);
                break;

            case "FaxService":
                if (isOn) await OptimizeSystemHelper.DisableFaxService().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableFaxService().ConfigureAwait(false);
                break;

            case "InsiderService":
                if (isOn) await OptimizeSystemHelper.DisableInsiderService().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableInsiderService().ConfigureAwait(false);
                break;

            case "SmartScreen":
                if (isOn) await OptimizeSystemHelper.DisableSmartScreen().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSmartScreen().ConfigureAwait(false);
                break;

            case "CloudClipboard":
                if (isOn) await OptimizeSystemHelper.DisableCloudClipboard().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableCloudClipboard().ConfigureAwait(false);
                break;

            case "StickyKeys":
                if (isOn) await OptimizeSystemHelper.DisableStickyKeys().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableStickyKeys().ConfigureAwait(false);
                break;

            case "CastToDevice":
                if (isOn) await OptimizeSystemHelper.RemoveCastToDevice().ConfigureAwait(false);
                else await OptimizeSystemHelper.AddCastToDevice().ConfigureAwait(false);
                break;

            case "VBS":
                if (isOn) await OptimizeSystemHelper.DisableVBS().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableVBS().ConfigureAwait(false);
                break;

            case "TaskbarToLeft":
                if (isOn) await OptimizeSystemHelper.AlignTaskbarToLeft().ConfigureAwait(false);
                else await OptimizeSystemHelper.AlignTaskbarToCenter().ConfigureAwait(false);
                break;

            case "SnapAssist":
                if (isOn) await OptimizeSystemHelper.DisableSnapAssist().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSnapAssist().ConfigureAwait(false);
                break;

            case "Widgets":
                if (isOn) await OptimizeSystemHelper.DisableWidgets().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWidgets().ConfigureAwait(false);
                break;

            case "Chat":
                if (isOn) await OptimizeSystemHelper.DisableChat().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableChat().ConfigureAwait(false);
                break;

            case "FilesCompactMode":
                if (isOn) await OptimizeSystemHelper.EnableFilesCompactMode().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableFilesCompactMode().ConfigureAwait(false);
                break;

            case "Stickers":
                if (isOn) await OptimizeSystemHelper.DisableStickers().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableStickers().ConfigureAwait(false);
                break;

            case "EdgeDiscoverBar":
                if (isOn) await OptimizeSystemHelper.DisableEdgeDiscoverBar().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableEdgeDiscoverBar().ConfigureAwait(false);
                break;

            case "EdgeTelemetry":
                if (isOn) await OptimizeSystemHelper.DisableEdgeTelemetry().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableEdgeTelemetry().ConfigureAwait(false);
                break;

            case "CoPilotAI":
                if (isOn) await OptimizeSystemHelper.DisableCoPilotAI().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableCoPilotAI().ConfigureAwait(false);
                break;

            case "WindowsRecall":
                if (isOn) await OptimizeSystemHelper.DisableWindowsRecall().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWindowsRecall().ConfigureAwait(false);
                break;

            case "VisualStudioTelemetry":
                if (isOn) await OptimizeSystemHelper.DisableVisualStudioTelemetry().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableVisualStudioTelemetry().ConfigureAwait(false);
                break;

            case "NvidiaTelemetry":
                if (isOn) await OptimizeSystemHelper.DisableNvidiaTelemetry().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableNvidiaTelemetry().ConfigureAwait(false);
                break;

            case "ChromeTelemetry":
                if (isOn) await OptimizeSystemHelper.DisableChromeTelemetry().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableChromeTelemetry().ConfigureAwait(false);
                break;

            case "FirefoxTelemetry":
                if (isOn) await OptimizeSystemHelper.DisableFirefoxTelemetry().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableFirefoxTelemetry().ConfigureAwait(false);
                break;

            case "Hibernation":
                if (isOn) await OptimizeSystemHelper.DisableHibernation().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableHibernation().ConfigureAwait(false);
                break;

            case "EndTask":
                if (isOn) await OptimizeSystemHelper.EnableEndTask().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableEndTask().ConfigureAwait(false);
                break;

            case "WindowsAI":
                if (isOn) await OptimizeSystemHelper.DisableWindowsAI().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWindowsAI().ConfigureAwait(false);
                break;

            case "GlobalTimerResolution":
                if (isOn) await OptimizeSystemHelper.EnableGlobalTimerResolution().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableGlobalTimerResolution().ConfigureAwait(false);
                break;

            case "HPET":
                if (isOn) await OptimizeSystemHelper.DisableHPET().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableHPET().ConfigureAwait(false);
                break;

            case "DynamicTick":
                if (isOn) await OptimizeSystemHelper.DisableDynamicTick().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableDynamicTick().ConfigureAwait(false);
                break;

            case "Prefetch":
                if (isOn) await OptimizeSystemHelper.DisablePrefetch().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnablePrefetch().ConfigureAwait(false);
                break;

            case "LargeSystemCache":
                if (isOn) await OptimizeSystemHelper.EnableLargeSystemCache().ConfigureAwait(false);
                else await OptimizeSystemHelper.DisableLargeSystemCache().ConfigureAwait(false);
                break;

            case "NDU":
                if (isOn) await OptimizeSystemHelper.DisableNDU().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableNDU().ConfigureAwait(false);
                break;

            case "PageFileEncryption":
                if (isOn) await OptimizeSystemHelper.DisablePageFileEncryption().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnablePageFileEncryption().ConfigureAwait(false);
                break;

            case "LockScreenNotifications":
                if (isOn) await OptimizeSystemHelper.DisableLockScreenNotifications().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableLockScreenNotifications().ConfigureAwait(false);
                break;

            case "ToastNotifications":
                if (isOn) await OptimizeSystemHelper.DisableToastNotifications().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableToastNotifications().ConfigureAwait(false);
                break;

            case "NotificationCenter":
                if (isOn) await OptimizeSystemHelper.DisableNotificationCenter().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableNotificationCenter().ConfigureAwait(false);
                break;

            case "SuggestedActions":
                if (isOn) await OptimizeSystemHelper.DisableSuggestedActions().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableSuggestedActions().ConfigureAwait(false);
                break;

            case "WiFiSense":
                if (isOn) await OptimizeSystemHelper.DisableWiFiSense().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWiFiSense().ConfigureAwait(false);
                break;

            case "WebSearchResults":
                if (isOn) await OptimizeSystemHelper.DisableWebSearchResults().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableWebSearchResults().ConfigureAwait(false);
                break;

            case "AppLaunchTracking":
                if (isOn) await OptimizeSystemHelper.DisableAppLaunchTracking().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableAppLaunchTracking().ConfigureAwait(false);
                break;

            case "TimelineHistory":
                if (isOn) await OptimizeSystemHelper.DisableTimelineHistory().ConfigureAwait(false);
                else await OptimizeSystemHelper.EnableTimelineHistory().ConfigureAwait(false);
                break;

            case "TakeOwnership":
                if (isOn) await OptimizeSystemHelper.AddTakeOwnership().ConfigureAwait(false);
                else await OptimizeSystemHelper.RemoveTakeOwnership().ConfigureAwait(false);
                break;

            case "OpenCmdHere":
                if (isOn) await OptimizeSystemHelper.AddOpenCmdHere().ConfigureAwait(false);
                else await OptimizeSystemHelper.RemoveOpenCmdHere().ConfigureAwait(false);
                break;

            case "CopyFilePath":
                if (isOn) await OptimizeSystemHelper.AddCopyFilePath().ConfigureAwait(false);
                else await OptimizeSystemHelper.RemoveCopyFilePath().ConfigureAwait(false);
                break;

            default:
                _ = LogHelper.Log($"Unhandled toggle tag queued: {tag}");
                break;
        }
    }
}
