using System.Globalization;
using System.Runtime.InteropServices;
using GoatTweaks.Core.Intelligence;

namespace GoatTweaks.Windows;

/// <summary>Optional read-only WMI shared by an already running LibreHardwareMonitor. No driver is installed or loaded.</summary>
public sealed class SharedSensorReader : IDisposable
{
    private object? _locator;
    private object? _connection;
    public string Status { get; private set; } = "Sensores compartilhados não disponíveis. GOAT não instala drivers de monitoramento.";
    public SharedSensorReader()
    {
        try
        {
            var type = Type.GetTypeFromProgID("WbemScripting.SWbemLocator");
            if (type is null) return;
            _locator = Activator.CreateInstance(type);
            _connection = ((dynamic)_locator!).ConnectServer(".", @"root\LibreHardwareMonitor");
            Status = "LibreHardwareMonitor · WMI local existente";
        }
        catch (Exception ex) when (ex is COMException or InvalidCastException or System.Reflection.TargetInvocationException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        { Dispose(); }
    }
    public MetricSample Enrich(MetricSample sample)
    {
        if (_connection is null) return sample;
        var cpuT = new List<double>(); var gpuT = new List<double>(); var cpuC = new List<double>(); var gpuC = new List<double>();
        object? collection = null;
        try
        {
            collection = ((dynamic)_connection).ExecQuery("SELECT Name,SensorType,Value,Identifier,Parent FROM Sensor", "WQL", 48);
            foreach (object item in (dynamic)collection)
            {
                try
                {
                    dynamic sensor = item;
                    var name = (Convert.ToString(sensor.Name, CultureInfo.InvariantCulture) + " " + Convert.ToString(sensor.Identifier, CultureInfo.InvariantCulture) + " " + Convert.ToString(sensor.Parent, CultureInfo.InvariantCulture)).ToLowerInvariant();
                    var type = Convert.ToString(sensor.SensorType, CultureInfo.InvariantCulture);
                    var value = Convert.ToDouble(sensor.Value, CultureInfo.InvariantCulture);
                    if (!double.IsFinite(value)) continue;
                    var gpu = name.Contains("gpu"); var cpu = name.Contains("cpu") || name.Contains("tctl");
                    if (type == "Temperature" && value is >= -20 and <= 150) { if (gpu) gpuT.Add(value); else if (cpu) cpuT.Add(value); }
                    if (type == "Clock" && value is > 0 and < 20000) { if (gpu && name.Contains("core")) gpuC.Add(value); else if (cpu && name.Contains("core")) cpuC.Add(value); }
                }
                finally { Release(item); }
            }
        }
        catch (Exception ex) when (ex is COMException or InvalidCastException or FormatException or System.Reflection.TargetInvocationException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        { Status = "A fonte de sensores não respondeu corretamente; leituras ausentes não são estimadas."; return sample; }
        finally { Release(collection); }
        return sample with
        {
            CpuTemperatureC = cpuT.Count > 0 ? cpuT.Max() : null, GpuTemperatureC = gpuT.Count > 0 ? gpuT.Max() : null,
            CpuClockMhz = cpuC.Count > 0 ? cpuC.Average() : null, GpuClockMhz = gpuC.Count > 0 ? gpuC.Max() : null, SensorSource = Status
        };
    }
    private static void Release(object? instance) { if (instance is not null && Marshal.IsComObject(instance)) Marshal.FinalReleaseComObject(instance); }
    public void Dispose() { Release(_connection); Release(_locator); _connection = _locator = null; }
}
