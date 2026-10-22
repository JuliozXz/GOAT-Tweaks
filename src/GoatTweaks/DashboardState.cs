using System.ComponentModel;
using System.Runtime.CompilerServices;
using GoatTweaks.Hardware;

namespace GoatTweaks;

public sealed class DashboardState : INotifyPropertyChanged
{
    private HardwareSnapshot _hardware = new();
    private string _hardwareStatus = "Identificando o computador em segundo plano…";
    private bool _isDetecting;
    private string _cpuPercent = "—";
    private string _ramPercent = "—";
    private string _memoryUsage = "Aguardando leitura do Windows";
    private string _queueSummary = "Sua fila está vazia";
    private string _queueButton = "Revisar fila · 00";
    private string _chartValue = "—";
    private string _chartDetail = "Coletando as primeiras amostras…";
    public HardwareSnapshot Hardware { get => _hardware; set => Set(ref _hardware, value); }
    public string HardwareStatus { get => _hardwareStatus; set => Set(ref _hardwareStatus, value); }
    public bool IsDetecting { get => _isDetecting; set => Set(ref _isDetecting, value); }
    public string CpuPercent { get => _cpuPercent; set => Set(ref _cpuPercent, value); }
    public string RamPercent { get => _ramPercent; set => Set(ref _ramPercent, value); }
    public string MemoryUsage { get => _memoryUsage; set => Set(ref _memoryUsage, value); }
    public string QueueSummary { get => _queueSummary; set => Set(ref _queueSummary, value); }
    public string QueueButton { get => _queueButton; set => Set(ref _queueButton, value); }
    public string ChartValue { get => _chartValue; set => Set(ref _chartValue, value); }
    public string ChartDetail { get => _chartDetail; set => Set(ref _chartDetail, value); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
