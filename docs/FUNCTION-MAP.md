# GOAT Tweaks — mapa função → origem

Gerado por `tools/gen_docs.py` a partir de `catalog/catalog.json`. Commits upstream fixados:

- RyTuneX: [https://github.com/rayenghanmi/RyTuneX](https://github.com/rayenghanmi/RyTuneX/tree/957885f53beaacfa7322581cafdb9eba5df5726a) @ `957885f53beaacfa7322581cafdb9eba5df5726a`
- WinSux: [https://github.com/FR33THYFR33THY/WinSux](https://github.com/FR33THYFR33THY/WinSux/tree/91adbfd63b7ed2eb5fcf0821fd53405a3cf63573) @ `91adbfd63b7ed2eb5fcf0821fd53405a3cf63573`
- Ultimate: [https://github.com/FR33THYFR33THY/Ultimate](https://github.com/FR33THYFR33THY/Ultimate/tree/a0a33a57078434201f582b0db5107dcbb502292a) @ `a0a33a57078434201f582b0db5107dcbb502292a`
- winutil: [https://github.com/christitustech/winutil](https://github.com/christitustech/winutil/tree/2f0bdc8fca52e1ebe31a8d98216428061d3f8956) @ `2f0bdc8fca52e1ebe31a8d98216428061d3f8956`

| Origem | Itens |
|---|---|
| Ultimate | 104 |
| WinSux | 1 |
| winutil | 385 |
| RyTuneX | 129 |

Legenda: Admin = exige administrador; Reversão = como desfazer (vazio = sem reversão automática); Flags = áreas do sistema afetadas (análise estática).

## RyTuneX

| ID | Título | Categoria | Tipo | Arquivo de origem | Admin | Risco | Flags | Reversão | Downloads |
|---|---|---|---|---|---|---|---|---|---|
| rytunex:UsbPowerSaving | Disable USB Power Saving | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | power | toggle off (opposite method) |  |
| rytunex:PowerThrottling | Disable Power Throttling | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:MenuShowDelay | Disable Menu Show Delay | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:MouseHoverTime | Disable Mouse Hover Time | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:KeyboardLatency | Optimize Keyboard Latency | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:MouseAcceleration | Disable Mouse Acceleration | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:AutoComplete | Disable Auto Complete | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WindowShake | Disable Window Shake | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:GlobalTimerResolution | Enable Global Timer Resolution | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:HPET | Disable HPET | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | boot | toggle off (opposite method) |  |
| rytunex:DynamicTick | Disable Dynamic Tick | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | boot | toggle off (opposite method) |  |
| rytunex:SysMain | Disable SysMain | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:Prefetch | Disable Prefetch &amp; Superfetch | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:LargeSystemCache | Enable Large System Cache | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:NDU | Disable Network Data Usage Monitor | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low |  | toggle off (opposite method) |  |
| rytunex:PageFileEncryption | Disable Page File Encryption | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low |  | toggle off (opposite method) |  |
| rytunex:GamingMode | Enable Gaming Mode | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:FullscreenOptimizations | Enable Fullscreen Optimizations | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:GpuDriverTweaks | GPU Driver Performance Tweaks | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | fileDeletion, registry | toggle off (opposite method) |  |
| rytunex:SystemProfile | Optimize System Profile | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:OptimizeNTFS | Optimize NTFS Settings | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:PrioritizeForegroundApplications | Optimize Foreground App Performance | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:LowDiskSpaceChecks | Enable Low Disk Space Checks | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:FileExtensionsAndHiddenFiles | Hide File Extensions and Hidden Files | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:LinkResolve | Disable Link Resolve | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:BackgroundApps | Disable Background Apps | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CrashDump | Disable Crash Dump | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:RemoteAssistance | Disable Remote Assistance | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:RemoteRegistry | Disable Remote Registry | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:ServiceHostSplitting | Disable Service Host Splitting | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:TaskTimeouts | Adjust Task Timeouts | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:ServiceTimeouts | Decrease Service Timeouts | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CopyMoveContextMenu | Add Copy/Move Context Menu | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WPBT | Disable WPBT Execution | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:LegacyBootMenu | Enable Legacy Boot Menu | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | boot | toggle off (opposite method) |  |
| rytunex:SystemRestore | Disable System Restore | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:Cortana | Disable Cortana | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:StoreUpdates | Disable Store Updates | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SmartScreen | Disable Smart Screen | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | registry, security | toggle off (opposite method) |  |
| rytunex:Drivers | Exclude Drivers From Windows Updates | OptimizeSystem | toggle | Views/OptimizeSystemPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | registry, updates | toggle off (opposite method) |  |
| rytunex:WindowsTransparency | Disable Windows Transparency | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WindowsDarkMode | Enable Windows Dark Mode | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:VerboseLogon | Enable Verbose Logon | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:HomeGroup | Disable Home Group | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:PrintService | Disable Print Service | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low |  | toggle off (opposite method) |  |
| rytunex:CompatibilityAssistant | Disable Compatibility Assistant | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:Search | Disable Search | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:FaxService | Disable Fax Service | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:InsiderService | Disable Insider Service | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:Hibernation | Disable Hibernation | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | power, registry | toggle off (opposite method) |  |
| rytunex:StickyKeys | Disable Sticky Keys | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WindowsInk | Disable Windows Ink | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SpellingAndTypingFeatures | Disable Spelling and Typing Features | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CloudClipboard | Disable Cloud Clipboard | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:QuickAccessHistory | Disable Quick Access History | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:MyPeople | Disable My People | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CastToDevice | Disable Cast to Device | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:ErrorReporting | Disable Error Reporting | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | registry, security, services | toggle off (opposite method) |  |
| rytunex:GameBar | Disable Game Bar | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:LockScreenNotifications | Disable Lock Screen Notifications | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:ToastNotifications | Disable All Toast Notifications | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:NotificationCenter | Disable Notification Center | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SuggestedActions | Disable Suggested Actions | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:ClassicContextMenu | Enable Classic Context Menu | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:RecommendedSectionStartMenu | Disable Recommended Section In Start Menu | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:TaskbarToLeft | Move Taskbar to The Left | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SnapAssist | Disable Snap Assist | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:Widgets | Disable Widgets | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:Chat | Disable Chat | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:FilesCompactMode | Enable Files Compact Mode | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:EndTask | Enable EndTask | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:Stickers | Disable Stickers | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CoPilotAI | Disable CoPilot AI | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | appx, fileDeletion, registry, services | toggle off (opposite method) |  |
| rytunex:WindowsRecall | Disable Windows Recall | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | appx, features, registry, services | toggle off (opposite method) |  |
| rytunex:WindowsAI | Disable Windows AI | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:EdgeDiscoverBar | Disable Edge Discover Bar | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:VBS | Disable VBS | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | boot, registry, security | toggle off (opposite method) |  |
| rytunex:TakeOwnership | Add "Take Ownership" to Context Menu | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:OpenCmdHere | Add "Open Command Prompt Here" | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CopyFilePath | Add "Copy File Path" to Context Menu | Features | toggle | Views/FeaturesPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:AdvertisingID | Disable Advertising ID | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:BluetoothAdvertising | Disable Bluetooth Advertising | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:NewsAndInterests | Disable News and Interests | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SpotlightFeatures | Disable Spotlight Features | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:TailoredExperiences | Disable Tailored Experiences | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:CloudOptimizedContent | Disable Cloud Optimized Content | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:FeedbackNotifications | Disable Feedback Notifications | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:TelemetryServices | Disable Telemetry Services | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | registry, security, services | toggle off (opposite method) |  |
| rytunex:EdgeTelemetry | Disable Edge Telemetry | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | high | registry, security, services | toggle off (opposite method) |  |
| rytunex:VisualStudioTelemetry | Disable Visual Studio Telemetry | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:NvidiaTelemetry | Disable Nvidia Telemetry | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | medium | registry, services | toggle off (opposite method) |  |
| rytunex:ChromeTelemetry | Disable Chrome Telemetry | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:FirefoxTelemetry | Disable Firefox Telemetry | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:DiagnosticsToast | Disable Diagnostics Toast | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:HandwritingDataSharing | Disable Handwriting Data Sharing | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:TextInputDataCollection | Disable Text Input Data Collection | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:InputPersonalization | Disable Input Personalization | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:OnlineSpeechPrivacy | Disable Online Speech Privacy | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SafeSearchMode | Disable Safe Search Mode | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:ClipboardSync | Disable Clipboard Sync | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:MessageSync | Disable Message Sync | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SettingSync | Disable Setting Sync | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:Cdp | Disable CDP (Customer Data Platform) | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:LocationFeatures | Disable Location Features | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:Biometrics | Disable Biometrics features | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:FindMyDevice | Disable Find My Device | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:VoiceActivation | Disable Voice Activation | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:AutomaticRestartSignOn | Disable Automatic Restart Sign-On | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SMBv1 | Disable SMBv1 | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:SMBv2 | Disable SMBv2 | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WiFiSense | Disable Wi-Fi Sense | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WebSearchResults | Disable Web Results in Search | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:AppLaunchTracking | Disable App Launch Tracking | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:TimelineHistory | Disable Timeline &amp; Activity History | Privacy | toggle | Views/PrivacyPage.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync | sim | low | registry | toggle off (opposite method) |  |
| rytunex:WindowsUpdates | Windows Updates | OptimizeSystem | combo | Views/OptimizeSystemPage.xaml (ComboBox) + Helpers/OptimizeSystemHelper.cs | sim | high | fileDeletion, registry, updates | option Default |  |
| rytunex:tool:FlushDNS | Flush DNS cache | Network | command | Views/NetworkPage.xaml.cs:439 | não | low |  | — |  |
| rytunex:tool:ReleaseRenewIP | Release / renew IP | Network | command | Views/NetworkPage.xaml.cs:464; Views/NetworkPage.xaml.cs:465 | sim | medium |  | — |  |
| rytunex:tool:ResetTCPIP | Reset TCP/IP and Winsock | Network | command | Views/NetworkPage.xaml.cs:492; Views/NetworkPage.xaml.cs:493 | sim | high | network | — |  |
| rytunex:tool:ResetDefaultDNS | Reset DNS to DHCP (adapter) | Network | command | Views/NetworkPage.xaml.cs:409; Views/NetworkPage.xaml.cs:410 | sim | medium | network | — |  |
| rytunex:tool:MemoryDiagRestart | Windows Memory Diagnostic (restart now) | Repair | command | Views/RepairPage.xaml.cs:995 | sim | high | boot, reboot | — |  |
| rytunex:tool:MemoryDiagNextBoot | Windows Memory Diagnostic (next boot) | Repair | command | Views/RepairPage.xaml.cs:1001 | sim | medium | boot | — |  |
| rytunex:tool:DefenderQuickScan | Microsoft Defender quick scan | Security | command | Views/SecurityPage.xaml.cs:540 | sim | low |  | — |  |
| rytunex:tool:DefenderUpdateSignatures | Update Defender signatures | Security | command | Views/SecurityPage.xaml.cs:584 | sim | low |  | — |  |
| rytunex:tool:DISMScan | DISM scan health | Repair | command | Views/RepairPage.xaml.cs:213-216,605-611 | sim | low |  | — |  |
| rytunex:tool:DISMRepair | DISM restore health | Repair | command | Views/RepairPage.xaml.cs:213-216,605-611 | sim | medium |  | — |  |
| rytunex:tool:SFCVerify | SFC verify only | Repair | command | Views/RepairPage.xaml.cs:213-216,605-611 | sim | low |  | — |  |
| rytunex:tool:SFCRepair | SFC scannow | Repair | command | Views/RepairPage.xaml.cs:213-216,605-611 | sim | medium |  | — |  |
| rytunex:tool:CHKDSKScan | CHKDSK scan (read-only) | Repair | command | Views/RepairPage.xaml.cs:213-216,605-611 | sim | low |  | — |  |
| rytunex:tool:CHKDSKSchedule | CHKDSK /f on next boot (system drive) | Repair | command | Views/RepairPage.xaml.cs:213-216,605-611 | sim | medium |  | — |  |

## WinSux

| ID | Título | Categoria | Tipo | Arquivo de origem | Admin | Risco | Flags | Reversão | Downloads |
|---|---|---|---|---|---|---|---|---|---|
| winsux:pipeline | WinSux (full pipeline) | WinSux | pipeline | WinSux/winsux.ps1 | sim | high | appx, bitlocker, boot, drivers, features, fileDeletion, network, power, reboot, registry, security, services, trustedInstaller, updates | — | 31 |

## Ultimate

| ID | Título | Categoria | Tipo | Arquivo de origem | Admin | Risco | Flags | Reversão | Downloads |
|---|---|---|---|---|---|---|---|---|---|
| ultimate:1 Check/1 Bios Check.ps1 | Bios Check | 1 Check | oneshot | 1 Check/1 Bios Check.ps1 | sim | low | reboot, registry | — | 1 |
| ultimate:1 Check/2 Storage Check.ps1 | Storage Check | 1 Check | oneshot | 1 Check/2 Storage Check.ps1 | sim | low |  | — |  |
| ultimate:1 Check/3 Ram Check.ps1 | Ram Check | 1 Check | oneshot | 1 Check/3 Ram Check.ps1 | sim | low |  | — | 1 |
| ultimate:1 Check/4 Gpu Check.ps1 | Gpu Check | 1 Check | oneshot | 1 Check/4 Gpu Check.ps1 | sim | low |  | — | 1 |
| ultimate:1 Check/5 Storage Ram Cpu Test & Bench.ps1 | Storage Ram Cpu Test & Bench | 1 Check | oneshot | 1 Check/5 Storage Ram Cpu Test & Bench.ps1 | sim | low |  | — | 1 |
| ultimate:1 Check/6 Gpu Test & Bench.ps1 | Gpu Test & Bench | 1 Check | oneshot | 1 Check/6 Gpu Test & Bench.ps1 | sim | low |  | — | 1 |
| ultimate:2 Refresh/1 Factory Reset.ps1 | Factory Reset | 2 Refresh | oneshot | 2 Refresh/1 Factory Reset.ps1 | não | high |  | — |  |
| ultimate:2 Refresh/2 Account Local.ps1 | Account Local | 2 Refresh | oneshot | 2 Refresh/2 Account Local.ps1 | não | low |  | — |  |
| ultimate:2 Refresh/3 Reinstall.ps1 | Reinstall | 2 Refresh | menu | 2 Refresh/3 Reinstall.ps1 | sim | high |  | — | 2 |
| ultimate:2 Refresh/4 Autounattend.ps1 | Autounattend | 2 Refresh | oneshot | 2 Refresh/4 Autounattend.ps1 | sim | high | fileDeletion, registry | — | 2 |
| ultimate:2 Refresh/5 Updates Drivers Block.ps1 | Updates Drivers Block | 2 Refresh | oneshot | 2 Refresh/5 Updates Drivers Block.ps1 | sim | high | reboot, registry, updates | — | 1 |
| ultimate:2 Refresh/6 Network Driver.ps1 | Network Driver | 2 Refresh | oneshot | 2 Refresh/6 Network Driver.ps1 | sim | low |  | — | 1 |
| ultimate:2 Refresh/7 To Bios.ps1 | To Bios | 2 Refresh | oneshot | 2 Refresh/7 To Bios.ps1 | sim | low | reboot | — |  |
| ultimate:3 Setup/1 BitLocker.ps1 | BitLocker | 3 Setup | menu | 3 Setup/1 BitLocker.ps1 | sim | high | bitlocker | option 2 (BitLocker: On`) |  |
| ultimate:3 Setup/2 Memory Compression.ps1 | Memory Compression | 3 Setup | menu | 3 Setup/2 Memory Compression.ps1 | sim | low |  | option 2 (Memory Compression: Enable) |  |
| ultimate:3 Setup/3 Convert Home To Pro.ps1 | Convert Home To Pro | 3 Setup | oneshot | 3 Setup/3 Convert Home To Pro.ps1 | sim | high |  | — |  |
| ultimate:3 Setup/4 Keys.ps1 | Keys | 3 Setup | oneshot | 3 Setup/4 Keys.ps1 | não | high |  | — | 1 |
| ultimate:3 Setup/5 Activation.ps1 | Activation | 3 Setup | oneshot | 3 Setup/5 Activation.ps1 | não | high |  | — |  |
| ultimate:3 Setup/6 Date Language Region Time.ps1 | Date Language Region Time | 3 Setup | oneshot | 3 Setup/6 Date Language Region Time.ps1 | não | low |  | — |  |
| ultimate:3 Setup/7 Startup Apps.ps1 | Startup Apps | 3 Setup | oneshot | 3 Setup/7 Startup Apps.ps1 | não | low |  | — |  |
| ultimate:3 Setup/8 Startup Apps.ps1 | Startup Apps | 3 Setup | oneshot | 3 Setup/8 Startup Apps.ps1 | não | low |  | — |  |
| ultimate:3 Setup/9 Background Apps.ps1 | Background Apps | 3 Setup | menu | 3 Setup/9 Background Apps.ps1 | sim | low | registry | option 2 (Background Apps: Default`) |  |
| ultimate:3 Setup/10 Edge Settings.ps1 | Edge Settings | 3 Setup | menu | 3 Setup/10 Edge Settings.ps1 | sim | medium | fileDeletion, registry, services | option 2 (Edge Settings: Default`) | 2 |
| ultimate:3 Setup/11 Store Settings.ps1 | Store Settings | 3 Setup | menu | 3 Setup/11 Store Settings.ps1 | sim | high | registry, updates | option 2 (Store Settings: Default`) |  |
| ultimate:3 Setup/12 Updates Pause.ps1 | Updates Pause | 3 Setup | oneshot | 3 Setup/12 Updates Pause.ps1 | sim | high | registry, updates | — |  |
| ultimate:4 Installers/1 Installers.ps1 | Installers | 4 Installers | menu | 4 Installers/1 Installers.ps1 | sim | medium | fileDeletion, registry, services | — | 30 |
| ultimate:4 Installers/2 MSI Afterburner.ps1 | MSI Afterburner | 4 Installers | oneshot | 4 Installers/2 MSI Afterburner.ps1 | sim | medium | fileDeletion | — | 2 |
| ultimate:5 Graphics/1 Driver Clean.ps1 | Driver Clean | 5 Graphics | menu | 5 Graphics/1 Driver Clean.ps1 | sim | high | boot, drivers, fileDeletion, reboot, registry | — | 2 |
| ultimate:5 Graphics/2 Driver Updated Install.ps1 | Driver Updated Install | 5 Graphics | oneshot | 5 Graphics/2 Driver Updated Install.ps1 | sim | low |  | — | 5 |
| ultimate:5 Graphics/3 Driver Updated Install & Settings.ps1 | Driver Updated Install & Settings | 5 Graphics | oneshot | 5 Graphics/3 Driver Updated Install & Settings.ps1 | sim | medium | fileDeletion, reboot, registry | — | 9 |
| ultimate:5 Graphics/4 Driver Debloat Install & Settings.ps1 | Driver Debloat Install & Settings | 5 Graphics | oneshot | 5 Graphics/4 Driver Debloat Install & Settings.ps1 | sim | high | fileDeletion, reboot, registry, security, services | — | 6 |
| ultimate:5 Graphics/5 Nvidia Settings.ps1 | Nvidia Settings | 5 Graphics | menu | 5 Graphics/5 Nvidia Settings.ps1 | sim | low | registry | option 2 (NVIDIA Settings: Default`) | 2 |
| ultimate:5 Graphics/6 Amd Settings.ps1 | Amd Settings | 5 Graphics | menu | 5 Graphics/6 Amd Settings.ps1 | sim | low | registry | option 2 (AMD Settings: Default`) |  |
| ultimate:5 Graphics/7 Intel Settings.ps1 | Intel Settings | 5 Graphics | menu | 5 Graphics/7 Intel Settings.ps1 | sim | low | registry | option 2 (INTEL Settings: Default`) |  |
| ultimate:5 Graphics/8 Hdcp.ps1 | Hdcp | 5 Graphics | menu | 5 Graphics/8 Hdcp.ps1 | sim | low | registry | option 2 (Default`) |  |
| ultimate:5 Graphics/9 P0 State.ps1 | P0 State | 5 Graphics | menu | 5 Graphics/9 P0 State.ps1 | sim | low | registry | option 2 (Default`) |  |
| ultimate:5 Graphics/10 Msi Mode.ps1 | Msi Mode | 5 Graphics | menu | 5 Graphics/10 Msi Mode.ps1 | sim | low | registry | — |  |
| ultimate:5 Graphics/11 DirectX.ps1 | DirectX | 5 Graphics | oneshot | 5 Graphics/11 DirectX.ps1 | sim | medium | fileDeletion, registry | — | 2 |
| ultimate:5 Graphics/12 C++.ps1 | C++ | 5 Graphics | oneshot | 5 Graphics/12 C++.ps1 | sim | low |  | — | 12 |
| ultimate:5 Graphics/13 Resolution Refresh Rate.ps1 | Resolution Refresh Rate | 5 Graphics | oneshot | 5 Graphics/13 Resolution Refresh Rate.ps1 | não | low |  | — |  |
| ultimate:5 Graphics/14 Hags Windowed.ps1 | Hags Windowed | 5 Graphics | oneshot | 5 Graphics/14 Hags Windowed.ps1 | não | low |  | — |  |
| ultimate:6 Windows/1 Start Menu Taskbar.ps1 | Start Menu Taskbar | 6 Windows | menu | 6 Windows/1 Start Menu Taskbar.ps1 | sim | medium | fileDeletion, registry | option 2 (Start Menu Taskbar: Default`) | 4 |
| ultimate:6 Windows/2 Start Menu Layout.ps1 | Start Menu Layout | 6 Windows | menu | 6 Windows/2 Start Menu Layout.ps1 | sim | low |  | — |  |
| ultimate:6 Windows/3 Start Menu Shortcuts.ps1 | Start Menu Shortcuts | 6 Windows | oneshot | 6 Windows/3 Start Menu Shortcuts.ps1 | sim | low |  | — |  |
| ultimate:6 Windows/4 Context Menu.ps1 | Context Menu | 6 Windows | menu | 6 Windows/4 Context Menu.ps1 | sim | high | registry, security | option 2 (Context Menu: Default`) |  |
| ultimate:6 Windows/5 Theme Black.ps1 | Theme Black | 6 Windows | menu | 6 Windows/5 Theme Black.ps1 | sim | low |  | option 2 (Theme: Default`) |  |
| ultimate:6 Windows/6 Signout Lockscreen Wallpaper Black.ps1 | Signout Lockscreen Wallpaper Black | 6 Windows | menu | 6 Windows/6 Signout Lockscreen Wallpaper Black.ps1 | sim | medium | fileDeletion, registry | option 2 (Signout Lockscreen Wallpaper: Default`) |  |
| ultimate:6 Windows/7 User Account Pictures Black.ps1 | User Account Pictures Black | 6 Windows | menu | 6 Windows/7 User Account Pictures Black.ps1 | sim | low |  | option 2 (Default`) |  |
| ultimate:6 Windows/8 Widgets.ps1 | Widgets | 6 Windows | menu | 6 Windows/8 Widgets.ps1 | sim | low | registry | option 2 (Widgets: Default`) |  |
| ultimate:6 Windows/9 Copilot.ps1 | Copilot | 6 Windows | menu | 6 Windows/9 Copilot.ps1 | sim | medium | appx, registry | option 2 (Copilot: Default`) |  |
| ultimate:6 Windows/10 Gamemode.ps1 | Gamemode | 6 Windows | oneshot | 6 Windows/10 Gamemode.ps1 | não | low |  | — |  |
| ultimate:6 Windows/11 Pointer Precision.ps1 | Pointer Precision | 6 Windows | oneshot | 6 Windows/11 Pointer Precision.ps1 | não | low |  | — |  |
| ultimate:6 Windows/12 Scaling.ps1 | Scaling | 6 Windows | oneshot | 6 Windows/12 Scaling.ps1 | não | low |  | — |  |
| ultimate:6 Windows/13 Bloatware.ps1 | Bloatware | 6 Windows | oneshot | 6 Windows/13 Bloatware.ps1 | sim | high | appx, features, fileDeletion, registry, security, services, updates | — | 2 |
| ultimate:6 Windows/14 Bloatware Legacy Apps Check.ps1 | Bloatware Legacy Apps Check | 6 Windows | oneshot | 6 Windows/14 Bloatware Legacy Apps Check.ps1 | sim | low |  | — |  |
| ultimate:6 Windows/15 Bloatware Legacy Features Check.ps1 | Bloatware Legacy Features Check | 6 Windows | oneshot | 6 Windows/15 Bloatware Legacy Features Check.ps1 | sim | medium | features | — |  |
| ultimate:6 Windows/16 Bloatware UWP Apps Check.ps1 | Bloatware UWP Apps Check | 6 Windows | oneshot | 6 Windows/16 Bloatware UWP Apps Check.ps1 | sim | low |  | — |  |
| ultimate:6 Windows/17 Bloatware UWP Features Check.ps1 | Bloatware UWP Features Check | 6 Windows | oneshot | 6 Windows/17 Bloatware UWP Features Check.ps1 | sim | low |  | — |  |
| ultimate:6 Windows/18 Bloatware TaskMgr Check.ps1 | Bloatware TaskMgr Check | 6 Windows | oneshot | 6 Windows/18 Bloatware TaskMgr Check.ps1 | não | low |  | — |  |
| ultimate:6 Windows/19 Gamebar.ps1 | Gamebar | 6 Windows | menu | 6 Windows/19 Gamebar.ps1 | sim | high | appx, registry, services, trustedInstaller | option 2 (Gamebar Xbox: Default`) | 2 |
| ultimate:6 Windows/20 Edge & WebView.ps1 | Edge & WebView | 6 Windows | menu | 6 Windows/20 Edge & WebView.ps1 | sim | medium | features, fileDeletion, registry, services | option 2 (Edge & WebView: Default`) | 3 |
| ultimate:6 Windows/21 Notepad Settings.ps1 | Notepad Settings | 6 Windows | menu | 6 Windows/21 Notepad Settings.ps1 | sim | medium | fileDeletion, registry | option 2 (Notepad Settings: Default`) |  |
| ultimate:6 Windows/22 Control Panel Settings.ps1 | Control Panel Settings | 6 Windows | menu | 6 Windows/22 Control Panel Settings.ps1 | sim | high | fileDeletion, power, registry, services, trustedInstaller, updates | option 2 (Control Panel Settings: Default`) |  |
| ultimate:6 Windows/23 Sound.ps1 | Sound | 6 Windows | oneshot | 6 Windows/23 Sound.ps1 | não | low |  | — |  |
| ultimate:6 Windows/24 Loudness EQ.ps1 | Loudness EQ | 6 Windows | oneshot | 6 Windows/24 Loudness EQ.ps1 | sim | medium | services | — |  |
| ultimate:6 Windows/25 Device Manager Power Savings & Wake.ps1 | Device Manager Power Savings & Wake | 6 Windows | menu | 6 Windows/25 Device Manager Power Savings & Wake.ps1 | sim | low | registry | option 2 (Device Manager Power Savings & Wake: Default`) |  |
| ultimate:6 Windows/26 Network Adapter Power Savings & Wake.ps1 | Network Adapter Power Savings & Wake | 6 Windows | menu | 6 Windows/26 Network Adapter Power Savings & Wake.ps1 | sim | low | registry | option 2 (Network Adapter Power Savings & Wake: Default`) |  |
| ultimate:6 Windows/27 Network IPv4 Only.ps1 | Network IPv4 Only | 6 Windows | menu | 6 Windows/27 Network IPv4 Only.ps1 | sim | medium | network | option 2 (Network: Default`) |  |
| ultimate:6 Windows/28 Write Cache Buffer Flushing.ps1 | Write Cache Buffer Flushing | 6 Windows | menu | 6 Windows/28 Write Cache Buffer Flushing.ps1 | sim | low | registry | option 2 (Write Cache Buffer Flushing: Default`) |  |
| ultimate:6 Windows/29 Power Plan.ps1 | Power Plan | 6 Windows | menu | 6 Windows/29 Power Plan.ps1 | sim | medium | power, registry | option 2 (Power Plan: Default`) |  |
| ultimate:6 Windows/30 Timer Resolution.ps1 | Timer Resolution | 6 Windows | menu | 6 Windows/30 Timer Resolution.ps1 | sim | medium | fileDeletion, registry, services | option 2 (Timer Resolution: Default`) |  |
| ultimate:6 Windows/31 UAC.ps1 | UAC | 6 Windows | menu | 6 Windows/31 UAC.ps1 | sim | high | registry, security | option 2 (UAC: Default`) |  |
| ultimate:6 Windows/32 Core Isolation.ps1 | Core Isolation | 6 Windows | oneshot | 6 Windows/32 Core Isolation.ps1 | não | high | security | — |  |
| ultimate:6 Windows/33 Defender Optimize.ps1 | Defender Optimize | 6 Windows | menu | 6 Windows/33 Defender Optimize.ps1 | sim | high | boot, reboot, registry, security, services, trustedInstaller | option 2 (Defender: Default`) |  |
| ultimate:6 Windows/34 Autoruns Startup Tasks & Apps Check.ps1 | Autoruns Startup Tasks & Apps Check | 6 Windows | oneshot | 6 Windows/34 Autoruns Startup Tasks & Apps Check.ps1 | sim | high | fileDeletion, registry, services, trustedInstaller | — | 1 |
| ultimate:6 Windows/35 Cleanup.ps1 | Cleanup | 6 Windows | oneshot | 6 Windows/35 Cleanup.ps1 | sim | high | fileDeletion, services, updates | — |  |
| ultimate:6 Windows/36 Restore Point.ps1 | Restore Point | 6 Windows | oneshot | 6 Windows/36 Restore Point.ps1 | sim | low | registry | — |  |
| ultimate:7 Hardware/1 Scaling Higher No Accel.ps1 | Scaling Higher No Accel | 7 Hardware | menu | 7 Hardware/1 Scaling Higher No Accel.ps1 | sim | low | registry | — |  |
| ultimate:7 Hardware/2 Background Polling Rate Cap.ps1 | Background Polling Rate Cap | 7 Hardware | menu | 7 Hardware/2 Background Polling Rate Cap.ps1 | sim | low | registry | option 2 (Default (125hz)`) |  |
| ultimate:7 Hardware/3 Mouse Polling Rate Test.ps1 | Mouse Polling Rate Test | 7 Hardware | oneshot | 7 Hardware/3 Mouse Polling Rate Test.ps1 | sim | low |  | — | 1 |
| ultimate:7 Hardware/4 Controller Overclock.ps1 | Controller Overclock | 7 Hardware | oneshot | 7 Hardware/4 Controller Overclock.ps1 | sim | high | drivers, fileDeletion | — | 1 |
| ultimate:7 Hardware/5 Controller Polling Rate Test.ps1 | Controller Polling Rate Test | 7 Hardware | oneshot | 7 Hardware/5 Controller Polling Rate Test.ps1 | sim | low |  | — | 1 |
| ultimate:7 Hardware/6 Monitor Optimization.ps1 | Monitor Optimization | 7 Hardware | oneshot | 7 Hardware/6 Monitor Optimization.ps1 | sim | low |  | — | 1 |
| ultimate:7 Hardware/7 Network Bufferbloat Test.ps1 | Network Bufferbloat Test | 7 Hardware | oneshot | 7 Hardware/7 Network Bufferbloat Test.ps1 | não | low |  | — | 1 |
| ultimate:7 Hardware/8 PC Build Guide.ps1 | PC Build Guide | 7 Hardware | oneshot | 7 Hardware/8 PC Build Guide.ps1 | não | low |  | — | 1 |
| ultimate:8 Advanced/1 Defender.ps1 | Defender | 8 Advanced | menu | 8 Advanced/1 Defender.ps1 | sim | high | boot, features, reboot, registry, security, services, trustedInstaller | option 2 (Defender: Enable (Default)`) |  |
| ultimate:8 Advanced/2 Firewall.ps1 | Firewall | 8 Advanced | menu | 8 Advanced/2 Firewall.ps1 | sim | high | registry, security | option 2 (Firewall: Enable (Default)`) |  |
| ultimate:8 Advanced/3 Spectre Meltdown.ps1 | Spectre Meltdown | 8 Advanced | menu | 8 Advanced/3 Spectre Meltdown.ps1 | sim | high | registry, security | option 2 (Spectre Meltdown: Enable (Default)`) |  |
| ultimate:8 Advanced/4 Data Execution Prevention.ps1 | Data Execution Prevention | 8 Advanced | menu | 8 Advanced/4 Data Execution Prevention.ps1 | sim | high | boot, security | option 2 (Enable (Default)`) |  |
| ultimate:8 Advanced/5 File Download Security Warning.ps1 | File Download Security Warning | 8 Advanced | menu | 8 Advanced/5 File Download Security Warning.ps1 | sim | low | registry | option 2 (Enable (Default)`) |  |
| ultimate:8 Advanced/6 MMAgent Features.ps1 | MMAgent Features | 8 Advanced | menu | 8 Advanced/6 MMAgent Features.ps1 | sim | low | registry | option 2 (Default) |  |
| ultimate:8 Advanced/7 ReBar Force.ps1 | ReBar Force | 8 Advanced | menu | 8 Advanced/7 ReBar Force.ps1 | sim | low | reboot | option 1 (DEFAULT DRIVER WHITELIST PER GAME (DEFAULT)) | 1 |
| ultimate:8 Advanced/8 Smt Ht.ps1 | Smt Ht | 8 Advanced | menu | 8 Advanced/8 Smt Ht.ps1 | sim | low |  | — |  |
| ultimate:8 Advanced/9 Core 1 Thread 1.ps1 | Core 1 Thread 1 | 8 Advanced | menu | 8 Advanced/9 Core 1 Thread 1.ps1 | sim | low |  | — |  |
| ultimate:8 Advanced/10 Priority.ps1 | Priority | 8 Advanced | menu | 8 Advanced/10 Priority.ps1 | sim | low |  | — |  |
| ultimate:8 Advanced/11 Mpo.ps1 | Mpo | 8 Advanced | menu | 8 Advanced/11 Mpo.ps1 | sim | low | registry | option 1 (On (Default)) |  |
| ultimate:8 Advanced/12 Hardware Legacy Flip.ps1 | Hardware Legacy Flip | 8 Advanced | menu | 8 Advanced/12 Hardware Legacy Flip.ps1 | sim | low | registry | option 1 (Fullscreen Optimizations - Hardware Independent Flip: FSO (Default)) |  |
| ultimate:8 Advanced/13 Hardware Composed Independent Flip.ps1 | Hardware Composed Independent Flip | 8 Advanced | menu | 8 Advanced/13 Hardware Composed Independent Flip.ps1 | sim | low | registry | option 2 (Hardware Independent Flip (Default)`) |  |
| ultimate:8 Advanced/14 Ulps.ps1 | Ulps | 8 Advanced | menu | 8 Advanced/14 Ulps.ps1 | sim | low | registry | option 1 (On (Default)) |  |
| ultimate:8 Advanced/15 Driver Whql Secure Boot Bypass.ps1 | Driver Whql Secure Boot Bypass | 8 Advanced | menu | 8 Advanced/15 Driver Whql Secure Boot Bypass.ps1 | sim | low | registry | option 1 (Off (Default)) |  |
| ultimate:8 Advanced/16 Keyboard Shortcuts.ps1 | Keyboard Shortcuts | 8 Advanced | menu | 8 Advanced/16 Keyboard Shortcuts.ps1 | sim | low | registry | option 1 (Keyboard Shortcuts: Default) |  |
| ultimate:8 Advanced/17 Services.ps1 | Services | 8 Advanced | menu | 8 Advanced/17 Services.ps1 | sim | high | boot, reboot, registry, security, services, trustedInstaller, updates | option 2 (Services: Default`) |  |
| ultimate:8 Advanced/18 Start Search Shell Mobsync.ps1 | Start Search Shell Mobsync | 8 Advanced | menu | 8 Advanced/18 Start Search Shell Mobsync.ps1 | sim | high | registry, security, services | option 2 (Default`) |  |
| ultimate:8 Advanced/19 NVME Faster Driver.ps1 | NVME Faster Driver | 8 Advanced | menu | 8 Advanced/19 NVME Faster Driver.ps1 | sim | low | registry | option 2 (NVME: Default`) |  |

## winutil

| ID | Título | Categoria | Tipo | Arquivo de origem | Admin | Risco | Flags | Reversão | Downloads |
|---|---|---|---|---|---|---|---|---|---|
| winutil:tweak:WPFTweaksActivity | Activity History - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksActivity | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksHiber | Hibernation - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksHiber | sim | medium | power, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksWidget | Widgets - Remove | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksWidget | sim | medium | appx | — | 1 |
| winutil:tweak:WPFTweaksRevertStartMenu | Start Menu Previous Layout - Enable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksRevertStartMenu | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableStoreSearch | Microsoft Store Recommended Search Results - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksDisableStoreSearch | sim | medium |  | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksLocation | Location Tracking - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksLocation | sim | medium | registry, services | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksServices | Services - Set to Manual | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksServices | sim | medium | services | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksSvcHostSplitting | svchost.exe Splitting - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksSvcHostSplitting | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksBraveDebloat | Brave Browser - Debloat | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksBraveDebloat | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableWarningForUnsignedRdp | RDP Unsigned File Warnings - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksDisableWarningForUnsignedRdp | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksEdgeDebloat | Microsoft Edge - Debloat | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksEdgeDebloat | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksConsumerFeatures | ConsumerFeatures - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksConsumerFeatures | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksTelemetry | Telemetry - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksTelemetry | sim | medium | security, registry, services | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDeliveryOptimization | Delivery Optimization - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksDeliveryOptimization | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksRemoveEdge | Microsoft Edge - Remove | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksRemoveEdge | sim | high |  | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableBitLocker | BitLocker - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksDisableBitLocker | sim | medium | bitlocker | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksUTC | Date & Time - Set Time to UTC | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksUTC | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksRemoveOneDrive | Microsoft OneDrive - Remove | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksRemoveOneDrive | sim | high | fileDeletion, services | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksRemoveHomeAndGallery | File Explorer Home and Gallery - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksRemoveHomeAndGallery | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisplay | Visual Effects - Set to Best Performance | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksDisplay | sim | high | fileDeletion, registry, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksReservedStorage | Disable Reserved Storage | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksReservedStorage | sim | high | features | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksRestorePoint | Restore Point - Create | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksRestorePoint | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksEndTaskOnTaskbar | End Task With Right Click - Enable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksEndTaskOnTaskbar | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksStorage | Storage Sense - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksStorage | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksWindowsAI | Windows AI - Disable And Remove | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksWindowsAI | sim | high | appx, features, services, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksWPBT | Windows Platform Binary Table (WPBT) - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksWPBT | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksPreventDeviceMetadataFromNetwork | Prevent Device Companion Apps | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksPreventDeviceMetadataFromNetwork | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksRazerBlock | Razer Software Auto-Install - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksRazerBlock | sim | high | fileDeletion, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksLogiBlock | Logitech Download Assistant Auto-Install - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksLogiBlock | sim | high | fileDeletion | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableNotifications | System Tray Notifications & Calendar - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksDisableNotifications | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksBlockAdobeNet | Adobe URL Block List - Enable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksBlockAdobeNet | sim | high |  | winutil undo (OriginalValue/OriginalType/UndoScript) | 2 |
| winutil:tweak:WPFTweaksRightClickMenu | Right-Click Menu Previous Layout - Enable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksRightClickMenu | sim | high | fileDeletion | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDiskCleanup | Disk Cleanup - Run | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksDiskCleanup | sim | medium | features | — | 1 |
| winutil:tweak:WPFTweaksDeleteTempFiles | Temporary Files - Remove | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksDeleteTempFiles | sim | medium | fileDeletion | — | 1 |
| winutil:tweak:WPFTweaksIPv46 | IPv6 - Set IPv4 as Preferred | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksIPv46 | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksTeredo | Teredo - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksTeredo | sim | high | network, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableIPv6 | IPv6 - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksDisableIPv6 | sim | high | network, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableBGapps | Background Apps - Disable | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFTweaksDisableBGapps | sim | high | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFTweaksDisableExplorerAutoDiscovery | File Explorer Automatic Folder Discovery - Disable | Essential Tweaks | tweak | config/tweaks.json#WPFTweaksDisableExplorerAutoDiscovery | sim | medium | fileDeletion, registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleDetailedBSoD | BSoD Verbose Mode | Customize Preferences | toggle | config/tweaks.json#WPFToggleDetailedBSoD | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleBatteryPercentage | System Tray Battery Percentage | Customize Preferences | toggle | config/tweaks.json#WPFToggleBatteryPercentage | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleDarkMode | Dark Theme for Windows | Customize Preferences | toggle | config/tweaks.json#WPFToggleDarkMode | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleShowExt | File Explorer File Extensions | Customize Preferences | toggle | config/tweaks.json#WPFToggleShowExt | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleHiddenFiles | File Explorer Hidden Files | Customize Preferences | toggle | config/tweaks.json#WPFToggleHiddenFiles | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleVerboseLogon | Logon Verbose Mode | Customize Preferences | toggle | config/tweaks.json#WPFToggleVerboseLogon | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleNewOutlook | Microsoft Outlook New Version | Customize Preferences | toggle | config/tweaks.json#WPFToggleNewOutlook | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleScrollbars | Scrollbars Always Visible | Customize Preferences | toggle | config/tweaks.json#WPFToggleScrollbars | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFMultiplaneOverlay | Multiplane Overlay | Customize Preferences | combo | config/tweaks.json#WPFMultiplaneOverlay | sim | medium | registry | — | 1 |
| winutil:tweak:WPFToggleMouseAcceleration | Mouse Acceleration | Customize Preferences | toggle | config/tweaks.json#WPFToggleMouseAcceleration | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleNumLock | Num Lock on Startup | Customize Preferences | toggle | config/tweaks.json#WPFToggleNumLock | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleWindowSnapping | Window Snapping | Customize Preferences | toggle | config/tweaks.json#WPFToggleWindowSnapping | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleStandbyFix | S0 Sleep Network Connectivity | Customize Preferences | toggle | config/tweaks.json#WPFToggleStandbyFix | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleS3Sleep | S3 Sleep | Customize Preferences | toggle | config/tweaks.json#WPFToggleS3Sleep | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleHideSettingsHome | Settings Home Page | Customize Preferences | toggle | config/tweaks.json#WPFToggleHideSettingsHome | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleBingSearch | Start Menu Bing Search | Customize Preferences | toggle | config/tweaks.json#WPFToggleBingSearch | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleLoginBlur | Logon Screen Acrylic Blur | Customize Preferences | toggle | config/tweaks.json#WPFToggleLoginBlur | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleDisableLockscreen | Lock Screen - Disable | Customize Preferences | toggle | config/tweaks.json#WPFToggleDisableLockscreen | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleStartMenuRecommendations | Start Menu Recommendations | Customize Preferences | toggle | config/tweaks.json#WPFToggleStartMenuRecommendations | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleStickyKeys | Sticky Keys | Customize Preferences | toggle | config/tweaks.json#WPFToggleStickyKeys | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleTaskbarAlignment | Taskbar Centered Icons | Customize Preferences | toggle | config/tweaks.json#WPFToggleTaskbarAlignment | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleTaskbarSearch | Taskbar Search Icon | Customize Preferences | toggle | config/tweaks.json#WPFToggleTaskbarSearch | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleTaskView | Taskbar Task View Icon | Customize Preferences | toggle | config/tweaks.json#WPFToggleTaskView | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleGameMode | Game Mode | Customize Preferences | toggle | config/tweaks.json#WPFToggleGameMode | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFToggleLongPaths | Enable Long Paths | Customize Preferences | toggle | config/tweaks.json#WPFToggleLongPaths | sim | medium | registry | winutil undo (OriginalValue/OriginalType/UndoScript) | 1 |
| winutil:tweak:WPFOOSUbutton | O&O ShutUp10++ - Run | z__Advanced Tweaks - CAUTION | tweak | config/tweaks.json#WPFOOSUbutton | sim | high |  | — | 1 |
| winutil:tweak:WPFchangedns | DNS - Set to: | z__Advanced Tweaks - CAUTION | combo | config/tweaks.json#WPFchangedns | sim | high |  | — | 1 |
| winutil:tweak:WPFAddUltPerf | Ultimate Performance Profile - Enable | Performance Plans - NOT FOR LAPTOPS | tweak | config/tweaks.json#WPFAddUltPerf | sim | medium |  | — | 1 |
| winutil:tweak:WPFRemoveUltPerf | Ultimate Performance Profile - Disable | Performance Plans - NOT FOR LAPTOPS | tweak | config/tweaks.json#WPFRemoveUltPerf | sim | medium |  | — | 1 |
| winutil:feature:WPFFeaturesdotnet | .NET Framework (Versions 2, 3, 4) - Enable | Features | feature | config/feature.json#WPFFeaturesdotnet | sim | medium | features | Disable-WindowsOptionalFeature (manual) | 1 |
| winutil:feature:WPFFixesNTPPool | NTP Server - Enable | Fixes | fix | config/feature.json#WPFFixesNTPPool | sim | medium |  | — | 1 |
| winutil:feature:WPFFeatureshyperv | Hyper-V - Enable | Features | feature | config/feature.json#WPFFeatureshyperv | sim | medium | features | Disable-WindowsOptionalFeature (manual) | 1 |
| winutil:feature:WPFFeatureslegacymedia | Legacy Media Components (WMP, DirectPlay) - Enable | Features | feature | config/feature.json#WPFFeatureslegacymedia | sim | medium | features | Disable-WindowsOptionalFeature (manual) | 1 |
| winutil:feature:WPFFeaturewsl | Windows Subsystem for Linux (WSL) - Enable | Features | feature | config/feature.json#WPFFeaturewsl | sim | medium | features | Disable-WindowsOptionalFeature (manual) | 1 |
| winutil:feature:WPFFeaturenfs | Network File System (NFS) - Enable | Features | feature | config/feature.json#WPFFeaturenfs | sim | medium | registry, features | Disable-WindowsOptionalFeature (manual) | 1 |
| winutil:feature:WPFFeatureRegBackup | Registry Backup (Daily Task 12:30am) - Enable | Features | fix | config/feature.json#WPFFeatureRegBackup | sim | medium | registry | — | 1 |
| winutil:feature:WPFFeatureEnableLegacyRecovery | Legacy F8 Boot Recovery - Enable | Features | fix | config/feature.json#WPFFeatureEnableLegacyRecovery | sim | medium | boot | — | 1 |
| winutil:feature:WPFFeatureDisableLegacyRecovery | Legacy F8 Boot Recovery - Disable | Features | fix | config/feature.json#WPFFeatureDisableLegacyRecovery | sim | medium | boot | — | 1 |
| winutil:feature:WPFFeaturesSandbox | Windows Sandbox - Enable | Features | feature | config/feature.json#WPFFeaturesSandbox | sim | medium | features | Disable-WindowsOptionalFeature (manual) | 1 |
| winutil:feature:WPFFeatureInstall | Install Features | Features | fix | config/feature.json#WPFFeatureInstall | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelAutologin | AutoLogon - Run | Fixes | fix | config/feature.json#WPFPanelAutologin | sim | medium |  | — | 1 |
| winutil:feature:WPFFixesUpdate | Windows Update - Reset | Fixes | fix | config/feature.json#WPFFixesUpdate | sim | medium |  | — | 1 |
| winutil:feature:WPFFixesNetwork | Network - Reset | Fixes | fix | config/feature.json#WPFFixesNetwork | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelDISM | System Corruption Scan - Run | Fixes | fix | config/feature.json#WPFPanelDISM | sim | medium | features | — | 1 |
| winutil:feature:WPFFixesWinget | WinGet - Reinstall | Fixes | fix | config/feature.json#WPFFixesWinget | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelComputer | Computer Management | Legacy Windows Panels | fix | config/feature.json#WPFPanelComputer | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelControl | Control Panel | Legacy Windows Panels | fix | config/feature.json#WPFPanelControl | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelMouse | Mouse Properties | Legacy Windows Panels | fix | config/feature.json#WPFPanelMouse | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelNetwork | Network Connections | Legacy Windows Panels | fix | config/feature.json#WPFPanelNetwork | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelPower | Power Panel | Legacy Windows Panels | fix | config/feature.json#WPFPanelPower | sim | medium | power | — | 1 |
| winutil:feature:WPFPanelPrinter | Printer Panel | Legacy Windows Panels | fix | config/feature.json#WPFPanelPrinter | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelPrograms | Programs and Features | Legacy Windows Panels | fix | config/feature.json#WPFPanelPrograms | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelRegion | Region | Legacy Windows Panels | fix | config/feature.json#WPFPanelRegion | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelSecurity | Security and Maintenance | Legacy Windows Panels | fix | config/feature.json#WPFPanelSecurity | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelSound | Sound Settings | Legacy Windows Panels | fix | config/feature.json#WPFPanelSound | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelSystem | System Properties | Legacy Windows Panels | fix | config/feature.json#WPFPanelSystem | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelTimedate | Time and Date | Legacy Windows Panels | fix | config/feature.json#WPFPanelTimedate | sim | medium |  | — | 1 |
| winutil:feature:WPFPanelFirewall | Windows Defender Firewall | Legacy Windows Panels | fix | config/feature.json#WPFPanelFirewall | sim | medium | security | — | 1 |
| winutil:feature:WPFPanelRestore | Windows Restore | Legacy Windows Panels | fix | config/feature.json#WPFPanelRestore | sim | medium |  | — | 1 |
| winutil:feature:WPFWinUtilInstallPSProfile | CTT PowerShell Profile - Install | Powershell Profile Powershell 7+ Only | fix | config/feature.json#WPFWinUtilInstallPSProfile | sim | medium |  | — | 1 |
| winutil:feature:WPFWinUtilUninstallPSProfile | CTT PowerShell Profile - Remove | Powershell Profile Powershell 7+ Only | fix | config/feature.json#WPFWinUtilUninstallPSProfile | sim | medium |  | — | 1 |
| winutil:feature:WPFWinUtilSSHServer | OpenSSH Server - Enable | Remote Access | fix | config/feature.json#WPFWinUtilSSHServer | sim | medium |  | — | 1 |
| winutil:appx:WPFAppxMicrosoft_WindowsFeedbackHub | Feedback Hub | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsFeedbackHub | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_GetHelp | Get Help | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_GetHelp | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_OutlookForWindows | Outlook for Windows | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_OutlookForWindows | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMSTeams | Microsoft Teams | AppX removal | appx | config/appx.json#WPFAppxMSTeams | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxClipchamp_Clipchamp | Clipchamp | AppX removal | appx | config/appx.json#WPFAppxClipchamp_Clipchamp | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_MicrosoftOfficeHub | Microsoft 365 | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_MicrosoftOfficeHub | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_ZuneMusic | Media Player | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_ZuneMusic | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_BingSearch | Bing Search | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_BingSearch | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoftCorporationII_QuickAssist | Quick Assist | AppX removal | appx | config/appx.json#WPFAppxMicrosoftCorporationII_QuickAssist | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsDevHome | Dev Home | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsDevHome | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsCrossDevice | Mobile Devices | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsCrossDevice | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_Todos | To Do | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_Todos | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_PowerAutomateDesktop | Power Automate | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_PowerAutomateDesktop | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_YourPhone | Phone Link | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_YourPhone | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_MicrosoftStickyNotes | Sticky Notes | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_MicrosoftStickyNotes | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsSoundRecorder | Sound Recorder | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsSoundRecorder | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsAlarms | Clock | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsAlarms | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_Paint | Paint | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_Paint | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsNotepad | Notepad | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsNotepad | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_ScreenSketch | Snipping Tool | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_ScreenSketch | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_Copilot | Copilot | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_Copilot | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsCalculator | Calculator | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsCalculator | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsCamera | Camera | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsCamera | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_WindowsPhotos | Photos | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_WindowsPhotos | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_BingNews | News | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_BingNews | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_BingWeather | Weather | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_BingWeather | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_GamingApp | Xbox App | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_GamingApp | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_XboxGamingOverlay | Xbox Game Bar | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_XboxGamingOverlay | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_XboxIdentityProvider | Xbox Identity Provider | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_XboxIdentityProvider | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_XboxSpeechToTextOverlay | Xbox Speech To Text Overlay | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_XboxSpeechToTextOverlay | sim | medium | appx | — |  |
| winutil:appx:WPFAppxMicrosoft_Xbox_TCUI | Xbox TCUI | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_Xbox_TCUI | sim | medium | appx | — |  |
| winutil:appx:WPFAppxMicrosoft_StartExperiencesApp | Start Experiences App | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_StartExperiencesApp | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:appx:WPFAppxMicrosoft_MicrosoftSolitaireCollection | Solitaire Collection | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_MicrosoftSolitaireCollection | sim | medium | appx | — |  |
| winutil:appx:WPFAppxMicrosoft_ZuneVideo | Movies & TV | AppX removal | appx | config/appx.json#WPFAppxMicrosoft_ZuneVideo | sim | medium | appx | reinstall from Microsoft Store (StoreId) |  |
| winutil:dns:Google | Google | DNS | dns | config/dns.json#Google | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:Cloudflare | Cloudflare | DNS | dns | config/dns.json#Cloudflare | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:Cloudflare_Malware | Cloudflare_Malware | DNS | dns | config/dns.json#Cloudflare_Malware | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:Cloudflare_Malware_Adult | Cloudflare_Malware_Adult | DNS | dns | config/dns.json#Cloudflare_Malware_Adult | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:Open_DNS | Open_DNS | DNS | dns | config/dns.json#Open_DNS | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:Quad9 | Quad9 | DNS | dns | config/dns.json#Quad9 | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:AdGuard_Ads_Trackers | AdGuard_Ads_Trackers | DNS | dns | config/dns.json#AdGuard_Ads_Trackers | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:dns:AdGuard_Ads_Trackers_Malware_Adult | AdGuard_Ads_Trackers_Malware_Adult | DNS | dns | config/dns.json#AdGuard_Ads_Trackers_Malware_Adult | sim | medium | network | DNS 'Default' (DHCP) |  |
| winutil:app:1password | 1Password | Install: Utilities | app | config/applications.json#1password | não | low |  | uninstall via winget/choco |  |
| winutil:app:7zip | 7-Zip | Install: Utilities | app | config/applications.json#7zip | não | low |  | uninstall via winget/choco |  |
| winutil:app:abdownloadmanager | AB Download Manager | Install: Utilities | app | config/applications.json#abdownloadmanager | não | low |  | uninstall via winget/choco |  |
| winutil:app:adobe | Adobe Acrobat Reader | Install: Document | app | config/applications.json#adobe | não | low |  | uninstall via winget/choco |  |
| winutil:app:advancedip | Advanced IP Scanner | Install: Pro Tools | app | config/applications.json#advancedip | não | low |  | uninstall via winget/choco |  |
| winutil:app:aimp | AIMP (Music Player) | Install: Multimedia Tools | app | config/applications.json#aimp | não | low |  | uninstall via winget/choco |  |
| winutil:app:angryipscanner | Angry IP Scanner | Install: Pro Tools | app | config/applications.json#angryipscanner | não | low |  | uninstall via winget/choco |  |
| winutil:app:anydesk | AnyDesk | Install: Utilities | app | config/applications.json#anydesk | não | low |  | uninstall via winget/choco |  |
| winutil:app:audacity | Audacity | Install: Multimedia Tools | app | config/applications.json#audacity | não | low |  | uninstall via winget/choco |  |
| winutil:app:autoruns | Autoruns | Install: Microsoft Tools | app | config/applications.json#autoruns | não | low |  | uninstall via winget/choco |  |
| winutil:app:rdcman | RDCMan | Install: Microsoft Tools | app | config/applications.json#rdcman | não | low |  | uninstall via winget/choco |  |
| winutil:app:autohotkey | AutoHotkey | Install: Utilities | app | config/applications.json#autohotkey | não | low |  | uninstall via winget/choco |  |
| winutil:app:battlenet | Battle.net | Install: Games | app | config/applications.json#battlenet | não | low |  | uninstall via winget/choco |  |
| winutil:app:bitwarden | Bitwarden | Install: Utilities | app | config/applications.json#bitwarden | não | low |  | uninstall via winget/choco |  |
| winutil:app:blender | Blender (3D Graphics) | Install: Multimedia Tools | app | config/applications.json#blender | não | low |  | uninstall via winget/choco |  |
| winutil:app:brave | Brave | Install: Browsers | app | config/applications.json#brave | não | low |  | uninstall via winget/choco |  |
| winutil:app:bruno | Bruno | Install: Development | app | config/applications.json#bruno | não | low |  | uninstall via winget/choco |  |
| winutil:app:bulkcrapuninstaller | Bulk Crap Uninstaller | Install: Utilities | app | config/applications.json#bulkcrapuninstaller | não | low |  | uninstall via winget/choco |  |
| winutil:app:blurautoclicker | BlurAutoClicker | Install: Utilities | app | config/applications.json#blurautoclicker | não | low |  | uninstall via winget/choco |  |
| winutil:app:calibre | Calibre | Install: Multimedia Tools | app | config/applications.json#calibre | não | low |  | uninstall via winget/choco |  |
| winutil:app:cemu | Cemu | Install: Games | app | config/applications.json#cemu | não | low |  | uninstall via winget/choco |  |
| winutil:app:chatgpt | ChatGPT Desktop | Install: Development | app | config/applications.json#chatgpt | não | low |  | uninstall via winget/choco |  |
| winutil:app:chatterino | Chatterino | Install: Communications | app | config/applications.json#chatterino | não | low |  | uninstall via winget/choco |  |
| winutil:app:chrome | Chrome | Install: Browsers | app | config/applications.json#chrome | não | low |  | uninstall via winget/choco |  |
| winutil:app:chromium | Chromium | Install: Browsers | app | config/applications.json#chromium | não | low |  | uninstall via winget/choco |  |
| winutil:app:cinebenchr23 | Cinebench R23 | Install: Pro Tools | app | config/applications.json#cinebenchr23 | não | low |  | uninstall via winget/choco |  |
| winutil:app:claude | Claude Desktop | Install: Development | app | config/applications.json#claude | não | low |  | uninstall via winget/choco |  |
| winutil:app:claude-code | Claude Code | Install: Development | app | config/applications.json#claude-code | não | low |  | uninstall via winget/choco |  |
| winutil:app:cmake | CMake | Install: Development | app | config/applications.json#cmake | não | low |  | uninstall via winget/choco |  |
| winutil:app:codex | Codex | Install: Development | app | config/applications.json#codex | não | low |  | uninstall via winget/choco |  |
| winutil:app:cpuz | CPU-Z | Install: Pro Tools | app | config/applications.json#cpuz | não | low |  | uninstall via winget/choco |  |
| winutil:app:crystaldiskinfo | Crystal Disk Info | Install: Utilities | app | config/applications.json#crystaldiskinfo | não | low |  | uninstall via winget/choco |  |
| winutil:app:crystaldiskmark | Crystal Disk Mark | Install: Utilities | app | config/applications.json#crystaldiskmark | não | low |  | uninstall via winget/choco |  |
| winutil:app:cursor | Cursor | Install: Development | app | config/applications.json#cursor | não | low |  | uninstall via winget/choco |  |
| winutil:app:ddu | Display Driver Uninstaller | Install: Pro Tools | app | config/applications.json#ddu | não | low |  | uninstall via winget/choco |  |
| winutil:app:discord | Discord | Install: Communications | app | config/applications.json#discord | não | low |  | uninstall via winget/choco |  |
| winutil:app:dismtools | DISMTools | Install: Microsoft Tools | app | config/applications.json#dismtools | não | low |  | uninstall via winget/choco |  |
| winutil:app:ntlite | NTLite | Install: Microsoft Tools | app | config/applications.json#ntlite | não | low |  | uninstall via winget/choco |  |
| winutil:app:dorion | Dorion | Install: Communications | app | config/applications.json#dorion | não | low |  | uninstall via winget/choco |  |
| winutil:app:dockerdesktop | Docker Desktop | Install: Development | app | config/applications.json#dockerdesktop | não | low |  | uninstall via winget/choco |  |
| winutil:app:dotnet6 | .NET Desktop Runtime 6 | Install: Microsoft Tools | app | config/applications.json#dotnet6 | não | low |  | uninstall via winget/choco |  |
| winutil:app:dotnet8 | .NET Desktop Runtime 8 | Install: Microsoft Tools | app | config/applications.json#dotnet8 | não | low |  | uninstall via winget/choco |  |
| winutil:app:dotnet9 | .NET Desktop Runtime 9 | Install: Microsoft Tools | app | config/applications.json#dotnet9 | não | low |  | uninstall via winget/choco |  |
| winutil:app:dotnet10 | .NET Desktop Runtime 10 | Install: Microsoft Tools | app | config/applications.json#dotnet10 | não | low |  | uninstall via winget/choco |  |
| winutil:app:dropbox | Dropbox | Install: Utilities | app | config/applications.json#dropbox | não | low |  | uninstall via winget/choco |  |
| winutil:app:eaapp | EA App | Install: Games | app | config/applications.json#eaapp | não | low |  | uninstall via winget/choco |  |
| winutil:app:eartrumpet | EarTrumpet (Audio) | Install: Multimedia Tools | app | config/applications.json#eartrumpet | não | low |  | uninstall via winget/choco |  |
| winutil:app:edge | Edge | Install: Browsers | app | config/applications.json#edge | não | low |  | uninstall via winget/choco |  |
| winutil:app:es-de | EmulationStation Desktop Edition | Install: Games | app | config/applications.json#es-de | não | low |  | uninstall via winget/choco |  |
| winutil:app:enteauth | Ente Auth | Install: Utilities | app | config/applications.json#enteauth | não | low |  | uninstall via winget/choco |  |
| winutil:app:epicgames | Epic Games Launcher | Install: Games | app | config/applications.json#epicgames | não | low |  | uninstall via winget/choco |  |
| winutil:app:files | Files | Install: Utilities | app | config/applications.json#files | não | low |  | uninstall via winget/choco |  |
| winutil:app:fileconverter | File Converter | Install: Multimedia Tools | app | config/applications.json#fileconverter | não | low |  | uninstall via winget/choco |  |
| winutil:app:firefox | Firefox | Install: Browsers | app | config/applications.json#firefox | não | low |  | uninstall via winget/choco |  |
| winutil:app:firefoxesr | Firefox ESR | Install: Browsers | app | config/applications.json#firefoxesr | não | low |  | uninstall via winget/choco |  |
| winutil:app:floorp | Floorp | Install: Browsers | app | config/applications.json#floorp | não | low |  | uninstall via winget/choco |  |
| winutil:app:flux | F.lux | Install: Utilities | app | config/applications.json#flux | não | low |  | uninstall via winget/choco |  |
| winutil:app:foobar | foobar2000 (Music Player) | Install: Multimedia Tools | app | config/applications.json#foobar | não | low |  | uninstall via winget/choco |  |
| winutil:app:fnm | Fast Node Manager | Install: Development | app | config/applications.json#fnm | não | low |  | uninstall via winget/choco |  |
| winutil:app:foxpdfreader | Foxit PDF Reader | Install: Document | app | config/applications.json#foxpdfreader | não | low |  | uninstall via winget/choco |  |
| winutil:app:geforcenow | GeForce NOW | Install: Games | app | config/applications.json#geforcenow | não | low |  | uninstall via winget/choco |  |
| winutil:app:gimp | GIMP (Image Editor) | Install: Multimedia Tools | app | config/applications.json#gimp | não | low |  | uninstall via winget/choco |  |
| winutil:app:git | Git | Install: Development | app | config/applications.json#git | não | low |  | uninstall via winget/choco |  |
| winutil:app:gitextensions | Git Extensions | Install: Development | app | config/applications.json#gitextensions | não | low |  | uninstall via winget/choco |  |
| winutil:app:githubcli | GitHub CLI | Install: Development | app | config/applications.json#githubcli | não | low |  | uninstall via winget/choco |  |
| winutil:app:githubdesktop | GitHub Desktop | Install: Development | app | config/applications.json#githubdesktop | não | low |  | uninstall via winget/choco |  |
| winutil:app:gog | GOG Galaxy | Install: Games | app | config/applications.json#gog | não | low |  | uninstall via winget/choco |  |
| winutil:app:golang | Go | Install: Development | app | config/applications.json#golang | não | low |  | uninstall via winget/choco |  |
| winutil:app:googledrive | Google Drive | Install: Utilities | app | config/applications.json#googledrive | não | low |  | uninstall via winget/choco |  |
| winutil:app:gpuz | GPU-Z | Install: Pro Tools | app | config/applications.json#gpuz | não | low |  | uninstall via winget/choco |  |
| winutil:app:gsudo | gsudo | Install: Pro Tools | app | config/applications.json#gsudo | não | low |  | uninstall via winget/choco |  |
| winutil:app:helium | Helium | Install: Browsers | app | config/applications.json#helium | não | low |  | uninstall via winget/choco |  |
| winutil:app:hugo | Hugo | Install: Development | app | config/applications.json#hugo | não | low |  | uninstall via winget/choco |  |
| winutil:app:handbrake | HandBrake | Install: Multimedia Tools | app | config/applications.json#handbrake | não | low |  | uninstall via winget/choco |  |
| winutil:app:heroiclauncher | Heroic Games Launcher | Install: Games | app | config/applications.json#heroiclauncher | não | low |  | uninstall via winget/choco |  |
| winutil:app:hwinfo | HWiNFO | Install: Pro Tools | app | config/applications.json#hwinfo | não | low |  | uninstall via winget/choco |  |
| winutil:app:hwmonitor | HWMonitor | Install: Pro Tools | app | config/applications.json#hwmonitor | não | low |  | uninstall via winget/choco |  |
| winutil:app:imhex | ImHex | Install: Pro Tools | app | config/applications.json#imhex | não | low |  | uninstall via winget/choco |  |
| winutil:app:imageglass | ImageGlass (Image Viewer) | Install: Multimedia Tools | app | config/applications.json#imageglass | não | low |  | uninstall via winget/choco |  |
| winutil:app:internetdownloadmanager | Internet Download Manager | Install: Utilities | app | config/applications.json#internetdownloadmanager | não | low |  | uninstall via winget/choco |  |
| winutil:app:irfanview | IrfanView | Install: Multimedia Tools | app | config/applications.json#irfanview | não | low |  | uninstall via winget/choco |  |
| winutil:app:itch | Itch.io | Install: Games | app | config/applications.json#itch | não | low |  | uninstall via winget/choco |  |
| winutil:app:itunes | iTunes | Install: Multimedia Tools | app | config/applications.json#itunes | não | low |  | uninstall via winget/choco |  |
| winutil:app:java8 | Amazon Corretto 8 (LTS) | Install: Development | app | config/applications.json#java8 | não | low |  | uninstall via winget/choco |  |
| winutil:app:java21 | Amazon Corretto 21 (LTS) | Install: Development | app | config/applications.json#java21 | não | low |  | uninstall via winget/choco |  |
| winutil:app:java25 | Amazon Corretto 25 (LTS) | Install: Development | app | config/applications.json#java25 | não | low |  | uninstall via winget/choco |  |
| winutil:app:jellyfinmediaplayer | Jellyfin Media Player | Install: Selfhosted Tools | app | config/applications.json#jellyfinmediaplayer | não | low |  | uninstall via winget/choco |  |
| winutil:app:jellyfinserver | Jellyfin Server | Install: Selfhosted Tools | app | config/applications.json#jellyfinserver | não | low |  | uninstall via winget/choco |  |
| winutil:app:jetbrains | Jetbrains Toolbox | Install: Development | app | config/applications.json#jetbrains | não | low |  | uninstall via winget/choco |  |
| winutil:app:jpegview | JPEG View | Install: Utilities | app | config/applications.json#jpegview | não | low |  | uninstall via winget/choco |  |
| winutil:app:joplin | Joplin | Install: Document | app | config/applications.json#joplin | não | low |  | uninstall via winget/choco |  |
| winutil:app:keepassxc | KeePassXC | Install: Utilities | app | config/applications.json#keepassxc | não | low |  | uninstall via winget/choco |  |
| winutil:app:klite | K-Lite Codec Standard | Install: Multimedia Tools | app | config/applications.json#klite | não | low |  | uninstall via winget/choco |  |
| winutil:app:kodi | Kodi Media Center | Install: Selfhosted Tools | app | config/applications.json#kodi | não | low |  | uninstall via winget/choco |  |
| winutil:app:lazygit | Lazygit | Install: Development | app | config/applications.json#lazygit | não | low |  | uninstall via winget/choco |  |
| winutil:app:libreoffice | LibreOffice | Install: Document | app | config/applications.json#libreoffice | não | low |  | uninstall via winget/choco |  |
| winutil:app:librewolf | LibreWolf | Install: Browsers | app | config/applications.json#librewolf | não | low |  | uninstall via winget/choco |  |
| winutil:app:localsend | LocalSend | Install: Selfhosted Tools | app | config/applications.json#localsend | não | low |  | uninstall via winget/choco |  |
| winutil:app:mpc-qt | mpc-qt | Install: Multimedia Tools | app | config/applications.json#mpc-qt | não | low |  | uninstall via winget/choco |  |
| winutil:app:mpv | mpv | Install: Multimedia Tools | app | config/applications.json#mpv | não | low |  | uninstall via winget/choco |  |
| winutil:app:matrix | Element | Install: Communications | app | config/applications.json#matrix | não | low |  | uninstall via winget/choco |  |
| winutil:app:minitoolpartitionwizard | MiniTool Partition Wizard | Install: Utilities | app | config/applications.json#minitoolpartitionwizard | não | low |  | uninstall via winget/choco |  |
| winutil:app:modrinth | Modrinth App | Install: Games | app | config/applications.json#modrinth | não | low |  | uninstall via winget/choco |  |
| winutil:app:moonlight | Moonlight/GameStream Client | Install: Selfhosted Tools | app | config/applications.json#moonlight | não | low |  | uninstall via winget/choco |  |
| winutil:app:mpchc | Media Player Classic - Home Cinema | Install: Multimedia Tools | app | config/applications.json#mpchc | não | low |  | uninstall via winget/choco |  |
| winutil:app:msedgeredirect | MSEdgeRedirect | Install: Utilities | app | config/applications.json#msedgeredirect | não | low |  | uninstall via winget/choco |  |
| winutil:app:msiafterburner | MSI Afterburner | Install: Pro Tools | app | config/applications.json#msiafterburner | não | low |  | uninstall via winget/choco |  |
| winutil:app:mullvadvpn | Mullvad VPN | Install: Pro Tools | app | config/applications.json#mullvadvpn | não | low |  | uninstall via winget/choco |  |
| winutil:app:mullvadbrowser | Mullvad Browser | Install: Browsers | app | config/applications.json#mullvadbrowser | não | low |  | uninstall via winget/choco |  |
| winutil:app:nomacs | nomacs | Install: Multimedia Tools | app | config/applications.json#nomacs | não | low |  | uninstall via winget/choco |  |
| winutil:app:nanazip | NanaZip | Install: Utilities | app | config/applications.json#nanazip | não | low |  | uninstall via winget/choco |  |
| winutil:app:netbird | NetBird | Install: Selfhosted Tools | app | config/applications.json#netbird | não | low |  | uninstall via winget/choco |  |
| winutil:app:tailscale | Tailscale | Install: Pro Tools | app | config/applications.json#tailscale | não | low |  | uninstall via winget/choco |  |
| winutil:app:naps2 | NAPS2 (Scanner) | Install: Document | app | config/applications.json#naps2 | não | low |  | uninstall via winget/choco |  |
| winutil:app:neovim | Neovim | Install: Development | app | config/applications.json#neovim | não | low |  | uninstall via winget/choco |  |
| winutil:app:nextclouddesktop | Nextcloud Desktop | Install: Selfhosted Tools | app | config/applications.json#nextclouddesktop | não | low |  | uninstall via winget/choco |  |
| winutil:app:nmap | Nmap | Install: Pro Tools | app | config/applications.json#nmap | não | low |  | uninstall via winget/choco |  |
| winutil:app:nodejs | NodeJS | Install: Development | app | config/applications.json#nodejs | não | low |  | uninstall via winget/choco |  |
| winutil:app:nodejslts | NodeJS LTS | Install: Development | app | config/applications.json#nodejslts | não | low |  | uninstall via winget/choco |  |
| winutil:app:pnpm | pnpm | Install: Development | app | config/applications.json#pnpm | não | low |  | uninstall via winget/choco |  |
| winutil:app:notepadplus | Notepad++ | Install: Development | app | config/applications.json#notepadplus | não | low |  | uninstall via winget/choco |  |
| winutil:app:nuget | NuGet | Install: Microsoft Tools | app | config/applications.json#nuget | não | low |  | uninstall via winget/choco |  |
| winutil:app:nvclean | NVCleanstall | Install: Utilities | app | config/applications.json#nvclean | não | low |  | uninstall via winget/choco |  |
| winutil:app:obs | OBS Studio | Install: Multimedia Tools | app | config/applications.json#obs | não | low |  | uninstall via winget/choco |  |
| winutil:app:obsidian | Obsidian | Install: Document | app | config/applications.json#obsidian | não | low |  | uninstall via winget/choco |  |
| winutil:app:okular | Okular | Install: Document | app | config/applications.json#okular | não | low |  | uninstall via winget/choco |  |
| winutil:app:onedrive | OneDrive | Install: Microsoft Tools | app | config/applications.json#onedrive | não | low |  | uninstall via winget/choco |  |
| winutil:app:onlyoffice | ONLYOFFICE Desktop | Install: Document | app | config/applications.json#onlyoffice | não | low |  | uninstall via winget/choco |  |
| winutil:app:OPAutoClicker | OPAutoClicker | Install: Utilities | app | config/applications.json#OPAutoClicker | não | low |  | uninstall via winget/choco |  |
| winutil:app:opencode | OpenCode | Install: Development | app | config/applications.json#opencode | não | low |  | uninstall via winget/choco |  |
| winutil:app:openrgb | OpenRGB | Install: Utilities | app | config/applications.json#openrgb | não | low |  | uninstall via winget/choco |  |
| winutil:app:OpenVPN | OpenVPN Connect | Install: Pro Tools | app | config/applications.json#OpenVPN | não | low |  | uninstall via winget/choco |  |
| winutil:app:OVirtualBox | Oracle VirtualBox | Install: Utilities | app | config/applications.json#OVirtualBox | não | low |  | uninstall via winget/choco |  |
| winutil:app:policyplus | Policy Plus | Install: Utilities | app | config/applications.json#policyplus | não | low |  | uninstall via winget/choco |  |
| winutil:app:processexplorer | Process Explorer | Install: Microsoft Tools | app | config/applications.json#processexplorer | não | low |  | uninstall via winget/choco |  |
| winutil:app:Paintdotnet | Paint.NET | Install: Multimedia Tools | app | config/applications.json#Paintdotnet | não | low |  | uninstall via winget/choco |  |
| winutil:app:parsec | Parsec | Install: Utilities | app | config/applications.json#parsec | não | low |  | uninstall via winget/choco |  |
| winutil:app:peazip | PeaZip | Install: Utilities | app | config/applications.json#peazip | não | low |  | uninstall via winget/choco |  |
| winutil:app:pdf-xchange | PDF-XChange Editor | Install: Document | app | config/applications.json#pdf-xchange | não | low |  | uninstall via winget/choco |  |
| winutil:app:pdf24creator | PDF24 Creator | Install: Document | app | config/applications.json#pdf24creator | não | low |  | uninstall via winget/choco |  |
| winutil:app:pdfgear | PDFgear | Install: Document | app | config/applications.json#pdfgear | não | low |  | uninstall via winget/choco |  |
| winutil:app:pdfsam | PDFsam Basic | Install: Document | app | config/applications.json#pdfsam | não | low |  | uninstall via winget/choco |  |
| winutil:app:playnite | Playnite | Install: Games | app | config/applications.json#playnite | não | low |  | uninstall via winget/choco |  |
| winutil:app:plex | Plex Media Server | Install: Selfhosted Tools | app | config/applications.json#plex | não | low |  | uninstall via winget/choco |  |
| winutil:app:plexdesktop | Plex Desktop | Install: Selfhosted Tools | app | config/applications.json#plexdesktop | não | low |  | uninstall via winget/choco |  |
| winutil:app:posh | Oh My Posh (Prompt) | Install: Development | app | config/applications.json#posh | não | low |  | uninstall via winget/choco |  |
| winutil:app:postman | Postman | Install: Development | app | config/applications.json#postman | não | low |  | uninstall via winget/choco |  |
| winutil:app:powershell | PowerShell | Install: Microsoft Tools | app | config/applications.json#powershell | não | low |  | uninstall via winget/choco |  |
| winutil:app:powertoys | PowerToys | Install: Microsoft Tools | app | config/applications.json#powertoys | não | low |  | uninstall via winget/choco |  |
| winutil:app:prismlauncher | Prism Launcher | Install: Games | app | config/applications.json#prismlauncher | não | low |  | uninstall via winget/choco |  |
| winutil:app:processlasso | Process Lasso | Install: Utilities | app | config/applications.json#processlasso | não | low |  | uninstall via winget/choco |  |
| winutil:app:protonauth | Proton Authenticator | Install: Utilities | app | config/applications.json#protonauth | não | low |  | uninstall via winget/choco |  |
| winutil:app:protonmail | Proton Mail | Install: Communications | app | config/applications.json#protonmail | não | low |  | uninstall via winget/choco |  |
| winutil:app:protondrive | Proton Drive | Install: Utilities | app | config/applications.json#protondrive | não | low |  | uninstall via winget/choco |  |
| winutil:app:protonpass | Proton Pass | Install: Utilities | app | config/applications.json#protonpass | não | low |  | uninstall via winget/choco |  |
| winutil:app:protonvpn | Proton VPN | Install: Pro Tools | app | config/applications.json#protonvpn | não | low |  | uninstall via winget/choco |  |
| winutil:app:processmonitor | Process Monitor | Install: Microsoft Tools | app | config/applications.json#processmonitor | não | low |  | uninstall via winget/choco |  |
| winutil:app:pstools | PsTools | Install: Microsoft Tools | app | config/applications.json#pstools | não | low |  | uninstall via winget/choco |  |
| winutil:app:putty | PuTTY | Install: Pro Tools | app | config/applications.json#putty | não | low |  | uninstall via winget/choco |  |
| winutil:app:python3 | Python3 | Install: Development | app | config/applications.json#python3 | não | low |  | uninstall via winget/choco |  |
| winutil:app:qbittorrent | qBittorrent | Install: Utilities | app | config/applications.json#qbittorrent | não | low |  | uninstall via winget/choco |  |
| winutil:app:qownnotes | QOwnNotes | Install: Document | app | config/applications.json#qownnotes | não | low |  | uninstall via winget/choco |  |
| winutil:app:qtox | QTox | Install: Communications | app | config/applications.json#qtox | não | low |  | uninstall via winget/choco |  |
| winutil:app:revo | Revo Uninstaller | Install: Utilities | app | config/applications.json#revo | não | low |  | uninstall via winget/choco |  |
| winutil:app:WiseProgramUninstaller | Wise Program Uninstaller (WiseCleaner) | Install: Utilities | app | config/applications.json#WiseProgramUninstaller | não | low |  | uninstall via winget/choco |  |
| winutil:app:rufus | Rufus Imager | Install: Pro Tools | app | config/applications.json#rufus | não | low |  | uninstall via winget/choco |  |
| winutil:app:rustlang | Rust | Install: Development | app | config/applications.json#rustlang | não | low |  | uninstall via winget/choco |  |
| winutil:app:sdio | Snappy Driver Installer Origin | Install: Utilities | app | config/applications.json#sdio | não | low |  | uninstall via winget/choco |  |
| winutil:app:sharex | ShareX (Screenshots) | Install: Multimedia Tools | app | config/applications.json#sharex | não | low |  | uninstall via winget/choco |  |
| winutil:app:nilesoftShell | Nilesoft Shell | Install: Utilities | app | config/applications.json#nilesoftShell | não | low |  | uninstall via winget/choco |  |
| winutil:app:systeminformer | System Informer | Install: Pro Tools | app | config/applications.json#systeminformer | não | low |  | uninstall via winget/choco |  |
| winutil:app:signal | Signal | Install: Communications | app | config/applications.json#signal | não | low |  | uninstall via winget/choco |  |
| winutil:app:signalrgb | SignalRGB | Install: Utilities | app | config/applications.json#signalrgb | não | low |  | uninstall via winget/choco |  |
| winutil:app:simplenote | Simplenote | Install: Document | app | config/applications.json#simplenote | não | low |  | uninstall via winget/choco |  |
| winutil:app:simplewall | Simplewall | Install: Pro Tools | app | config/applications.json#simplewall | não | low |  | uninstall via winget/choco |  |
| winutil:app:slack | Slack | Install: Communications | app | config/applications.json#slack | não | low |  | uninstall via winget/choco |  |
| winutil:app:startallback | StartAllBack | Install: Utilities | app | config/applications.json#startallback | não | low |  | uninstall via winget/choco |  |
| winutil:app:starship | Starship (Shell Prompt) | Install: Development | app | config/applications.json#starship | não | low |  | uninstall via winget/choco |  |
| winutil:app:steam | Steam | Install: Games | app | config/applications.json#steam | não | low |  | uninstall via winget/choco |  |
| winutil:app:roblox | Roblox | Install: Games | app | config/applications.json#roblox | não | low |  | uninstall via winget/choco |  |
| winutil:app:sublimetext | Sublime Text | Install: Development | app | config/applications.json#sublimetext | não | low |  | uninstall via winget/choco |  |
| winutil:app:sumatra | Sumatra PDF | Install: Document | app | config/applications.json#sumatra | não | low |  | uninstall via winget/choco |  |
| winutil:app:sunshine | Sunshine/GameStream Server | Install: Selfhosted Tools | app | config/applications.json#sunshine | não | low |  | uninstall via winget/choco |  |
| winutil:app:synctrayzor | SyncTrayzor | Install: Selfhosted Tools | app | config/applications.json#synctrayzor | não | low |  | uninstall via winget/choco |  |
| winutil:app:syncthing | Syncthing (CLI / Web UI) | Install: Selfhosted Tools | app | config/applications.json#syncthing | não | low |  | uninstall via winget/choco |  |
| winutil:app:tcpview | TCPView | Install: Microsoft Tools | app | config/applications.json#tcpview | não | low |  | uninstall via winget/choco |  |
| winutil:app:teams | Teams | Install: Communications | app | config/applications.json#teams | não | low |  | uninstall via winget/choco |  |
| winutil:app:teamviewer | TeamViewer | Install: Utilities | app | config/applications.json#teamviewer | não | low |  | uninstall via winget/choco |  |
| winutil:app:teamspeak3 | TeamSpeak 3 | Install: Communications | app | config/applications.json#teamspeak3 | não | low |  | uninstall via winget/choco |  |
| winutil:app:teamspeak6 | TeamSpeak 6 | Install: Communications | app | config/applications.json#teamspeak6 | não | low |  | uninstall via winget/choco |  |
| winutil:app:telegram | Telegram | Install: Communications | app | config/applications.json#telegram | não | low |  | uninstall via winget/choco |  |
| winutil:app:terminal | Windows Terminal | Install: Microsoft Tools | app | config/applications.json#terminal | não | low |  | uninstall via winget/choco |  |
| winutil:app:thunderbird | Thunderbird | Install: Communications | app | config/applications.json#thunderbird | não | low |  | uninstall via winget/choco |  |
| winutil:app:betterbird | Betterbird | Install: Communications | app | config/applications.json#betterbird | não | low |  | uninstall via winget/choco |  |
| winutil:app:tor | Tor Browser | Install: Browsers | app | config/applications.json#tor | não | low |  | uninstall via winget/choco |  |
| winutil:app:totalcommander | Total Commander | Install: Utilities | app | config/applications.json#totalcommander | não | low |  | uninstall via winget/choco |  |
| winutil:app:treesize | TreeSize Free | Install: Utilities | app | config/applications.json#treesize | não | low |  | uninstall via winget/choco |  |
| winutil:app:ttaskbar | TranslucentTB | Install: Utilities | app | config/applications.json#ttaskbar | não | low |  | uninstall via winget/choco |  |
| winutil:app:ubisoft | Ubisoft Connect | Install: Games | app | config/applications.json#ubisoft | não | low |  | uninstall via winget/choco |  |
| winutil:app:ungoogled | Ungoogled Chromium | Install: Browsers | app | config/applications.json#ungoogled | não | low |  | uninstall via winget/choco |  |
| winutil:app:unity | Unity Game Engine | Install: Development | app | config/applications.json#unity | não | low |  | uninstall via winget/choco |  |
| winutil:app:vagrant | Vagrant | Install: Development | app | config/applications.json#vagrant | não | low |  | uninstall via winget/choco |  |
| winutil:app:everything | Everything | Install: Utilities | app | config/applications.json#everything | não | low |  | uninstall via winget/choco |  |
| winutil:app:vc2015_32 | Visual C++ 2015-2022 32-bit | Install: Microsoft Tools | app | config/applications.json#vc2015_32 | não | low |  | uninstall via winget/choco |  |
| winutil:app:vc2015_64 | Visual C++ 2015-2022 64-bit | Install: Microsoft Tools | app | config/applications.json#vc2015_64 | não | low |  | uninstall via winget/choco |  |
| winutil:app:ventoy | Ventoy | Install: Pro Tools | app | config/applications.json#ventoy | não | low |  | uninstall via winget/choco |  |
| winutil:app:vesktop | Vesktop | Install: Communications | app | config/applications.json#vesktop | não | low |  | uninstall via winget/choco |  |
| winutil:app:viber | Viber | Install: Communications | app | config/applications.json#viber | não | low |  | uninstall via winget/choco |  |
| winutil:app:visualstudio2022 | Visual Studio 2022 | Install: Development | app | config/applications.json#visualstudio2022 | não | low |  | uninstall via winget/choco |  |
| winutil:app:visualstudio2026 | Visual Studio 2026 | Install: Development | app | config/applications.json#visualstudio2026 | não | low |  | uninstall via winget/choco |  |
| winutil:app:vivaldi | Vivaldi | Install: Browsers | app | config/applications.json#vivaldi | não | low |  | uninstall via winget/choco |  |
| winutil:app:vlc | VLC (Video Player) | Install: Multimedia Tools | app | config/applications.json#vlc | não | low |  | uninstall via winget/choco |  |
| winutil:app:vrdesktopstreamer | Virtual Desktop Streamer | Install: Games | app | config/applications.json#vrdesktopstreamer | não | low |  | uninstall via winget/choco |  |
| winutil:app:vscode | VS Code | Install: Development | app | config/applications.json#vscode | não | low |  | uninstall via winget/choco |  |
| winutil:app:vscodium | VS Codium | Install: Development | app | config/applications.json#vscodium | não | low |  | uninstall via winget/choco |  |
| winutil:app:waterfox | Waterfox | Install: Browsers | app | config/applications.json#waterfox | não | low |  | uninstall via winget/choco |  |
| winutil:app:whatsapp | WhatsApp Desktop | Install: Communications | app | config/applications.json#whatsapp | não | low |  | uninstall via winget/choco |  |
| winutil:app:wingetui | UniGetUI | Install: Utilities | app | config/applications.json#wingetui | não | low |  | uninstall via winget/choco |  |
| winutil:app:winrar | WinRAR | Install: Utilities | app | config/applications.json#winrar | não | low |  | uninstall via winget/choco |  |
| winutil:app:winscp | WinSCP | Install: Pro Tools | app | config/applications.json#winscp | não | low |  | uninstall via winget/choco |  |
| winutil:app:wireguard | WireGuard | Install: Pro Tools | app | config/applications.json#wireguard | não | low |  | uninstall via winget/choco |  |
| winutil:app:wireshark | Wireshark | Install: Pro Tools | app | config/applications.json#wireshark | não | low |  | uninstall via winget/choco |  |
| winutil:app:wiztree | WizTree | Install: Utilities | app | config/applications.json#wiztree | não | low |  | uninstall via winget/choco |  |
| winutil:app:xeheditor | HxD Hex Editor | Install: Utilities | app | config/applications.json#xeheditor | não | low |  | uninstall via winget/choco |  |
| winutil:app:xournal | Xournal++ | Install: Document | app | config/applications.json#xournal | não | low |  | uninstall via winget/choco |  |
| winutil:app:yarn | Yarn | Install: Development | app | config/applications.json#yarn | não | low |  | uninstall via winget/choco |  |
| winutil:app:zoom | Zoom | Install: Communications | app | config/applications.json#zoom | não | low |  | uninstall via winget/choco |  |
| winutil:app:uv | uv | Install: Development | app | config/applications.json#uv | não | low |  | uninstall via winget/choco |  |
| winutil:app:tightvnc | TightVNC | Install: Utilities | app | config/applications.json#tightvnc | não | low |  | uninstall via winget/choco |  |
| winutil:app:glazewm | GlazeWM | Install: Utilities | app | config/applications.json#glazewm | não | low |  | uninstall via winget/choco |  |
| winutil:app:Overwolf | CurseForge | Install: Games | app | config/applications.json#Overwolf | não | low |  | uninstall via winget/choco |  |
| winutil:app:OFGB | OFGB (Oh Frick Go Back) | Install: Utilities | app | config/applications.json#OFGB | não | low |  | uninstall via winget/choco |  |
| winutil:app:ZenBrowser | Zen Browser | Install: Browsers | app | config/applications.json#ZenBrowser | não | low |  | uninstall via winget/choco |  |
| winutil:app:Zed | Zed | Install: Development | app | config/applications.json#Zed | não | low |  | uninstall via winget/choco |  |
| winutil:app:zotero | Zotero | Install: Document | app | config/applications.json#zotero | não | low |  | uninstall via winget/choco |  |
| winutil:app:deskflow | Deskflow | Install: Utilities | app | config/applications.json#deskflow | não | low |  | uninstall via winget/choco |  |
| winutil:app:Ruby | Ruby | Install: Development | app | config/applications.json#Ruby | não | low |  | uninstall via winget/choco |  |
| winutil:app:Lua | Lua | Install: Development | app | config/applications.json#Lua | não | low |  | uninstall via winget/choco |  |
| winutil:app:CloudflareWARP | Cloudflare WARP | Install: Utilities | app | config/applications.json#CloudflareWARP | não | low |  | uninstall via winget/choco |  |
| winutil:updates:Invoke-WPFUpdatesdefault | Updates - Default | Updates | function | functions/public/Invoke-WPFUpdatesdefault.ps1 | sim | high | fileDeletion, registry, services, updates | — |  |
| winutil:updates:Invoke-WPFUpdatessecurity | Updates - Security (Recommended) | Updates | function | functions/public/Invoke-WPFUpdatessecurity.ps1 | sim | high | fileDeletion, registry, services, updates | winutil:updates:Invoke-WPFUpdatesdefault | 1 |
| winutil:updates:Invoke-WPFUpdatesdisable | Updates - Disable All | Updates | function | functions/public/Invoke-WPFUpdatesdisable.ps1 | sim | high | fileDeletion, registry, services, updates | winutil:updates:Invoke-WPFUpdatesdefault |  |
