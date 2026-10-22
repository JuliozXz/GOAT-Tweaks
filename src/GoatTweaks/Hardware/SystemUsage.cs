using System.Runtime.InteropServices;

namespace GoatTweaks.Hardware;

public sealed record MemoryUsage(ulong Total, ulong Used)
{
    public double Percent => Total == 0 ? 0 : Math.Clamp(100d * Used / Total, 0, 100);
}
public sealed record UsageSample(long Tick, double? Cpu, MemoryUsage? Memory);

/// <summary>Two native snapshots per tick; no counters, services, subprocesses or network requests.</summary>
public sealed class SystemUsage
{
    private ulong? _lastIdle;
    private ulong _lastTotal;
    public bool MultipleProcessorGroups { get; } = GetActiveProcessorGroupCount() > 1;

    public UsageSample Read()
    {
        double? cpu = null;
        if (!MultipleProcessorGroups && GetSystemTimes(out var idle, out var kernel, out var user))
        {
            var total = kernel.Value + user.Value;
            if (_lastIdle.HasValue && total > _lastTotal && idle.Value >= _lastIdle.Value)
            {
                var deltaTotal = total - _lastTotal;
                var deltaIdle = idle.Value - _lastIdle.Value;
                cpu = Math.Clamp(100d * (1d - Math.Min(deltaIdle, deltaTotal) / (double)deltaTotal), 0, 100);
            }
            _lastIdle = idle.Value;
            _lastTotal = total;
        }
        return new UsageSample(Environment.TickCount64, cpu, ReadMemory());
    }

    public static MemoryUsage? ReadMemory()
    {
        var state = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return GlobalMemoryStatusEx(ref state) && state.TotalPhysical > 0
            ? new MemoryUsage(state.TotalPhysical, state.TotalPhysical - Math.Min(state.TotalPhysical, state.AvailablePhysical)) : null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeFileTime
    {
        public uint Low;
        public uint High;
        public readonly ulong Value => ((ulong)High << 32) | Low;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint Load;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out NativeFileTime idle, out NativeFileTime kernel, out NativeFileTime user);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus state);
    [DllImport("kernel32.dll")]
    private static extern ushort GetActiveProcessorGroupCount();
}
