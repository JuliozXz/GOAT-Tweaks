using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using GoatTweaks.Core.Intelligence;
using GoatTweaks.Hardware;

namespace GoatTweaks.Windows;

/// <summary>PDH native samples. Counters use English names and arrays, independent of the Windows display language.</summary>
public sealed class WindowsPerformanceSampler : IDisposable
{
    private IntPtr _query;
    private readonly Dictionary<string, IntPtr> _counters = [];
    private readonly SystemUsage _fallback = new();
    public List<string> Unavailable { get; } = [];
    public WindowsPerformanceSampler()
    {
        if (PdhOpenQueryW(null, UIntPtr.Zero, out _query) != 0) { Unavailable.Add("Contadores de desempenho nativos indisponíveis."); return; }
        Add("cpu", @"\Processor Information(_Total)\% Processor Time");
        Add("threads", @"\Processor Information(*)\% Processor Time");
        Add("gpu", @"\GPU Engine(*)\Utilization Percentage");
        Add("paging", @"\Memory\Pages Input/sec");
        Add("disk", @"\PhysicalDisk(_Total)\Disk Bytes/sec");
        Add("latency", @"\PhysicalDisk(_Total)\Avg. Disk sec/Transfer");
        PdhCollectQueryData(_query);
        _fallback.Read();
    }
    private void Add(string key, string path)
    {
        if (PdhAddEnglishCounterW(_query, path, UIntPtr.Zero, out var counter) == 0) _counters[key] = counter;
        else Unavailable.Add("Contador não disponível: " + path);
    }
    private double? Scalar(string key)
    {
        if (!_counters.TryGetValue(key, out var counter) || PdhGetFormattedCounterValue(counter, 0x200, out _, out var value) != 0 || value.Status > 1 || !double.IsFinite(value.Number)) return null;
        return value.Number;
    }
    private Dictionary<string,double> Array(string key)
    {
        var result = new Dictionary<string,double>(StringComparer.Ordinal);
        if (!_counters.TryGetValue(key, out var counter)) return result;
        uint bytes = 0, count = 0;
        PdhGetFormattedCounterArrayW(counter, 0x200, ref bytes, ref count, IntPtr.Zero);
        if (bytes == 0 || bytes > 16 * 1024 * 1024) return result;
        var buffer = Marshal.AllocHGlobal((int)bytes);
        try
        {
            if (PdhGetFormattedCounterArrayW(counter, 0x200, ref bytes, ref count, buffer) != 0) return result;
            var size = Marshal.SizeOf<CounterItem>();
            for (var i = 0; i < count; i++)
            {
                var item = Marshal.PtrToStructure<CounterItem>(IntPtr.Add(buffer, i * size));
                if (item.Value.Status <= 1 && double.IsFinite(item.Value.Number) && Marshal.PtrToStringUni(item.Name) is { } name)
                    result[name] = item.Value.Number;
            }
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return result;
    }
    public MetricSample Read()
    {
        if (_query != IntPtr.Zero) PdhCollectQueryData(_query);
        var fallback = _fallback.Read();
        var threads = Array("threads").Where(x => !x.Key.Contains("_Total", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key, x => Math.Clamp(x.Value,0,100));
        var gpu = Array("gpu").Select(p => new { p.Value, Match = Regex.Match(p.Key, @"luid_.*?_phys_\d+_eng_\d+_engtype_3D", RegexOptions.IgnoreCase) })
            .Where(p => p.Match.Success).GroupBy(p => p.Match.Value, StringComparer.OrdinalIgnoreCase).Select(g => g.Sum(p=>p.Value)).ToArray();
        return new MetricSample
        {
            At = DateTimeOffset.UtcNow, CpuPercent = Scalar("cpu") is { } cpu ? Math.Clamp(cpu,0,100) : fallback.Cpu,
            MemoryPercent = fallback.Memory?.Percent, AvailableMemoryMb = fallback.Memory is { } memory ? (memory.Total - memory.Used) / 1048576d : null,
            CpuThreads = threads, Gpu3dPercent = gpu.Length > 0 ? Math.Clamp(gpu.Max(),0,100) : null,
            PagesInputPerSecond = Scalar("paging"), DiskBytesPerSecond = Scalar("disk"), DiskLatencyMs = Scalar("latency") * 1000
        };
    }
    public void Dispose() { if (_query != IntPtr.Zero) { PdhCloseQuery(_query); _query = IntPtr.Zero; } }
    [StructLayout(LayoutKind.Explicit, Size=16)] private struct CounterValue { [FieldOffset(0)] public uint Status; [FieldOffset(8)] public double Number; }
    [StructLayout(LayoutKind.Sequential)] private struct CounterItem { public IntPtr Name; public CounterValue Value; }
    [DllImport("pdh.dll",CharSet=CharSet.Unicode)] private static extern uint PdhOpenQueryW(string? source, UIntPtr user, out IntPtr query);
    [DllImport("pdh.dll",CharSet=CharSet.Unicode)] private static extern uint PdhAddEnglishCounterW(IntPtr query,string path,UIntPtr user,out IntPtr counter);
    [DllImport("pdh.dll")] private static extern uint PdhCollectQueryData(IntPtr query);
    [DllImport("pdh.dll")] private static extern uint PdhGetFormattedCounterValue(IntPtr counter,uint format,out uint type,out CounterValue value);
    [DllImport("pdh.dll",CharSet=CharSet.Unicode)] private static extern uint PdhGetFormattedCounterArrayW(IntPtr counter,uint format,ref uint bytes,ref uint count,IntPtr buffer);
    [DllImport("pdh.dll")] private static extern uint PdhCloseQuery(IntPtr query);
}
