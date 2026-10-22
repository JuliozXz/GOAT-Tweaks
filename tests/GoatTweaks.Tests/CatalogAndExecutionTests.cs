using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Conflicts;
using GoatTweaks.Core.Execution;
using GoatTweaks.Core.Assets;
using GoatTweaks.Runner;
using GoatTweaks.Runner.Logging;
using Xunit;

namespace GoatTweaks.Tests;

public sealed class CatalogAndExecutionTests
{
    [Fact]
    public void Catalog_loads_and_every_item_has_a_section()
    {
        var catalog = CatalogService.Load(Path.Combine(RepositoryRoot(), "catalog", "catalog.json"));
        Assert.Equal(619, catalog.Items.Count);
        Assert.Empty(catalog.Validate());
        Assert.All(catalog.Items, item => Assert.Contains(item.Section, CatalogSections.All));
        Assert.Equal(8, catalog.Items.Count(item => item.ConsoleRequired));
    }

    [Fact]
    public void Ultimate_and_WinSux_sources_match_catalog_hashes()
    {
        var root = RepositoryRoot();
        var catalog = CatalogService.Load(Path.Combine(root, "catalog", "catalog.json"));
        var items = catalog.Items.Where(item => (item.Source is "Ultimate" or "WinSux") && item.Sha256 is not null).ToList();
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.True(CatalogIntegrity.Verify(item, root), item.Id));
    }

    [Fact]
    public void RyTuneX_sources_are_byte_identical_to_upstream()
    {
        var root = RepositoryRoot();
        var copy = Path.Combine(root, "third_party", "RyTuneX");
        var upstream = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "repos", "src", "RyTuneX");
        Assert.True(Directory.Exists(upstream), $"Upstream checkout not found: {upstream}");
        foreach (var copiedFile in Directory.GetFiles(copy, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(copy, copiedFile);
            var original = Path.Combine(upstream, relative);
            Assert.True(File.Exists(original), $"Missing upstream source: {relative}");
            Assert.Equal(File.ReadAllBytes(original), File.ReadAllBytes(copiedFile));
        }
    }

    [Fact]
    public void Ultimate_stdin_orders_menu_inputs_and_exit_key()
    {
        var item = new CatalogItem
        {
            Id = "synthetic:ultimate",
            Kind = "menu",
            Options = [new CatalogOption { Key = "2", Label = "Option 2" }],
            FreeInputs = ["Drive letter"],
            Looping = true,
            ExitKey = "0"
        };
        Assert.Equal("2\nD\n0\n", UltimateInputBuilder.Build(item, "2", ["D"]));
    }

    [Fact]
    public void Ultimate_policy_forces_console_and_defaults_timeout_to_120_minutes()
    {
        Assert.True(UltimateRunPolicy.UseConsoleWindow(consoleRequired: true, requested: false));
        Assert.True(UltimateRunPolicy.UseConsoleWindow(consoleRequired: false, requested: true));
        Assert.False(UltimateRunPolicy.UseConsoleWindow(consoleRequired: false, requested: false));
        Assert.Equal(120, UltimateRunPolicy.TimeoutMinutes(null));
        Assert.Equal(9, UltimateRunPolicy.TimeoutMinutes("9"));
        Assert.Equal(1, UltimateRunPolicy.TimeoutMinutes("0"));
    }

    [Fact]
    public void Validators_reject_bad_drive_exe_and_nic_values()
    {
        Assert.True(InputValidator.IsDriveLetter("C"));
        Assert.False(InputValidator.IsDriveLetter("C:"));
        Assert.False(InputValidator.IsDriveLetter("1"));
        Assert.False(InputValidator.IsSafeExeInput("game.exe\nmalicious"));
        Assert.False(InputValidator.IsSafeNetworkInterface("Wi-Fi & cmd"));
        Assert.False(InputValidator.IsSafeNetworkInterface("NIC\r\n"));
    }

    [Fact]
    public void Batch_json_round_trips()
    {
        var original = new ActionBatch
        {
            Actions = [new ActionRequest { ItemId = "rytunex:tool:FlushDNS", Inputs = ["adapter"], Confirmation = "CONFIRMAR" }]
        };
        var clone = JsonSerializer.Deserialize<ActionBatch>(JsonSerializer.Serialize(original));
        Assert.NotNull(clone);
        Assert.Equal(original.Actions[0].ItemId, clone!.Actions[0].ItemId);
        Assert.Equal(original.Actions[0].Inputs, clone.Actions[0].Inputs);
        Assert.Equal(original.Actions[0].Confirmation, clone.Actions[0].Confirmation);
    }

    [Fact]
    public void Lead_conflicts_have_valid_schema_exclusive_pipeline_and_block_user_choice_pair()
    {
        var path = Path.Combine(RepositoryRoot(), "catalog", "conflicts.lead.json");
        var detector = ConflictDetector.Load(path);
        Assert.Empty(detector.Validate());
        Assert.Contains(detector.Groups, group => group.Decision == "exclusive" && group.Items.Contains("winsux:pipeline"));
        Assert.Empty(ConflictDetector.Load(Path.Combine(RepositoryRoot(), "catalog", "conflicts.json")).Validate());

        var group = detector.Groups.First(candidate =>
            candidate.Decision == "user-choice" && candidate.Items.Count >= 2);
        Assert.Contains(detector.FindUserChoiceConflicts(group.Items.Take(2)),
            violation => violation.Group.Id == group.Id);
    }

    [Fact]
    public void Duplicate_conflicts_are_informational()
    {
        var detector = new ConflictDetector([
            new ConflictGroup { Id = "d", Setting = "s", Items = ["one", "two"], Decision = "duplicate", Note = "info" }
        ]);
        Assert.Empty(detector.FindUserChoiceConflicts(["one", "two"]));
        Assert.Equal(new[] { "two" }, detector.FindDuplicateInfo("one").Single().SelectedItems);
    }

    [Fact]
    public void RyTuneX_command_substitutes_drive_root_but_not_memdiag_and_checks_nic()
    {
        Assert.Equal("echo C:\\ {memdiag}", RyTuneXCommandBuilder.Substitute(
            "echo {DriveRoot} {memdiag}", null, @"C:\Windows\System32"));
        Assert.Equal("netsh interface ipv4 set dnsservers \"Wi-Fi\" dhcp",
            RyTuneXCommandBuilder.Substitute("netsh interface ipv4 set dnsservers \"{nic}\" dhcp",
                "Wi-Fi", @"C:\Windows\System32"));
        Assert.Throws<ArgumentException>(() => RyTuneXCommandBuilder.Substitute("netsh {nic}", "a&b", @"C:\Windows\System32"));
    }

    [Fact]
    public void Function_and_command_catalog_kinds_are_supported()
    {
        var catalog = CatalogService.Load(Path.Combine(RepositoryRoot(), "catalog", "catalog.json"));
        Assert.Contains(catalog.Items, item => item.Kind == "function" && item.Function == "Invoke-WPFUpdatesdefault");
        Assert.Contains(catalog.Items, item => item.Kind == "command" && item.Launcher == "StartInCmd");
    }

    [Fact]
    public void RyTuneX_extraction_matches_original_method_bytes()
    {
        var root = RepositoryRoot();
        var extractionScript = "import json,sys;sys.path.insert(0,'tools');import extract_rytunex as e;s=e.read_source(e.SOURCE);print(json.dumps([e.extract(s,x) for x in e.SIGNATURES]))";
        var extraction = Run("python3", ["-c", extractionScript], root);
        Assert.Equal(0, extraction.ExitCode);
        var spans = JsonSerializer.Deserialize<string[]>(extraction.Stdout)!;
        var generated = File.ReadAllBytes(Path.Combine(root, "src", "GoatTweaks.Runner", "RyTuneX", "OptimizationOptions.Extracted.cs"));
        foreach (var span in spans)
            Assert.True(ContainsBytes(generated, Encoding.UTF8.GetBytes(span)), "Generated method span differs from upstream bytes.");
    }

    [Fact]
    public void Embedded_third_party_assets_materialize_for_single_file_launch()
    {
        var assetRoot = Path.Combine(Path.GetTempPath(), $"goat-assets-{Guid.NewGuid():N}");
        try
        {
            EmbeddedAssetStore.MaterializeThirdParty(assetRoot);
            Assert.True(File.Exists(Path.Combine(assetRoot, "third_party", "WinSux", "WinSux", "winsux.ps1")));
            Assert.True(File.Exists(Path.Combine(assetRoot, "third_party", "winutil", "harness.ps1")));
        }
        finally
        {
            if (Directory.Exists(assetRoot))
                Directory.Delete(assetRoot, recursive: true);
        }
    }

    [Fact]
    public void Every_third_party_file_matches_sha256sums()
    {
        var root = Path.Combine(RepositoryRoot(), "third_party");
        var manifest = Path.Combine(root, "SHA256SUMS");
        var expected = File.ReadAllLines(manifest).Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => (Hash: line[..64], Path: line[66..].TrimStart('*').Replace("./", "", StringComparison.Ordinal)))
            .ToDictionary(pair => pair.Path, pair => pair.Hash, StringComparer.Ordinal);
        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                     .Where(path => !Path.GetRelativePath(root, path).Equals("SHA256SUMS", StringComparison.Ordinal)))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            Assert.True(expected.TryGetValue(relative, out var hash), $"File missing from SHA256SUMS: {relative}");
            Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant());
        }
        Assert.Equal(expected.Count, Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length - 1);
    }

    [Fact]
    public async Task ActionLog_error_collector_captures_async_local_messages()
    {
        var path = Path.Combine(Path.GetTempPath(), $"goat-log-{Guid.NewGuid():N}.jsonl");
        var previousPath = Environment.GetEnvironmentVariable("GOATTWEAKS_ACTION_LOG");
        try
        {
            Environment.SetEnvironmentVariable("GOATTWEAKS_ACTION_LOG", path);
            using var collector = ActionLog.BeginErrorCollection();
            await ActionLog.WriteUpstreamAsync("Error", "first error", "caller", "file.cs");
            await ActionLog.WriteUpstreamAsync("Information", "not an error", "caller", "file.cs");
            Assert.Equal(new[] { "first error" }, collector.Messages);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOATTWEAKS_ACTION_LOG", previousPath);
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task Batch_action_failures_do_not_stop_later_actions()
    {
        var executed = new List<int>();
        var loggedFailures = new List<int>();
        var failures = await ActionBatchIsolation.RunAsync(new[] { 1, 2, 3 }, async action =>
        {
            executed.Add(action);
            await Task.Yield();
            if (action == 2)
                throw new InvalidOperationException("action failed");
            return action == 3 ? 1 : 0;
        }, (action, _) =>
        {
            loggedFailures.Add(action);
            return Task.CompletedTask;
        });

        Assert.Equal(new[] { 1, 2, 3 }, executed);
        Assert.Equal(new[] { 2 }, loggedFailures);
        Assert.Equal(2, failures);
    }

    [Fact]
    public void Launcher_token_and_PowerShell_parsers_are_valid()
    {
        var root = RepositoryRoot();
        var launcher = Path.Combine(root, "launcher", "goat.ps1");
        Assert.Contains("__GOAT_REPO__", File.ReadAllText(launcher));
        var launcherPath = launcher.Replace("'", "''", StringComparison.Ordinal);
        var parsed = Run("pwsh", ["-NoProfile", "-Command",
            $"$t=$null;$e=$null;[System.Management.Automation.Language.Parser]::ParseFile('{launcherPath}',[ref]$t,[ref]$e)|Out-Null;if($e.Count){{$e|% ToString;exit 1}};'Parse errors: 0'"], root);
        Assert.Equal(0, parsed.ExitCode);
        Assert.Contains("Parse errors: 0", parsed.Stdout);

        var harness = Path.Combine(root, "src", "GoatTweaks.Core", "Harness", "winutil-harness.ps1");
        var harnessPath = harness.Replace("'", "''", StringComparison.Ordinal);
        var harnessParse = Run("pwsh", ["-NoProfile", "-Command",
            $"$t=$null;$e=$null;[System.Management.Automation.Language.Parser]::ParseFile('{harnessPath}',[ref]$t,[ref]$e)|Out-Null;if($e.Count){{$e|% ToString;exit 1}};'Parse errors: 0'"], root);
        Assert.Equal(0, harnessParse.ExitCode);
        Assert.Contains("Parse errors: 0", harnessParse.Stdout);
    }

    [Fact]
    public void Materialized_winutil_harness_loads_and_lists_keys()
    {
        var assetRoot = Path.Combine(Path.GetTempPath(), $"goat-harness-{Guid.NewGuid():N}");
        try
        {
            EmbeddedAssetStore.MaterializeThirdParty(assetRoot);
            var harness = Path.Combine(assetRoot, "third_party", "winutil", "harness.ps1");
            var result = Run("pwsh", ["-NoProfile", "-File", harness, "-ListTweaks"], RepositoryRoot());
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Loaded 68 tweak keys", result.Stdout + result.Stderr);
        }
        finally
        {
            if (Directory.Exists(assetRoot))
                Directory.Delete(assetRoot, recursive: true);
        }
    }

    private static string RepositoryRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
            if (File.Exists(Path.Combine(current.FullName, "catalog", "catalog.json")))
                return current.FullName;
        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private static (int ExitCode, string Stdout, string Stderr) Run(string program, IEnumerable<string> args, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(program)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        foreach (var argument in args)
            process.StartInfo.ArgumentList.Add(argument);
        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    private static bool ContainsBytes(byte[] haystack, byte[] needle)
    {
        for (var start = 0; start <= haystack.Length - needle.Length; start++)
            if (haystack.AsSpan(start, needle.Length).SequenceEqual(needle))
                return true;
        return false;
    }
}
