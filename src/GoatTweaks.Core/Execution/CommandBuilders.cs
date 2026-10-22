using System.Text;
using System.Text.RegularExpressions;
using GoatTweaks.Core.Catalog;

namespace GoatTweaks.Core.Execution;

public static partial class InputValidator
{
    [GeneratedRegex("^[A-Za-z]$", RegexOptions.CultureInvariant)]
    private static partial Regex DriveLetterPattern();

    [GeneratedRegex(@"^[\p{L}\p{N} ._()\-]{1,128}$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    public static bool IsDriveLetter(string? value) =>
        value is not null && DriveLetterPattern().IsMatch(value);

    public static bool IsSafeExeInput(string? value) =>
        !string.IsNullOrWhiteSpace(value) && NamePattern().IsMatch(value) && value.Any(char.IsLetterOrDigit);

    public static bool IsSafeNetworkInterface(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 256 &&
        !value.Any(character => char.IsControl(character) ||
            character is '"' or '\'' or '\u0060' or '$' or ';' or '&' or '|' or '<' or '>' or '^' or '%' or '!');

    public static void ValidateFreeInput(string prompt, string? value)
    {
        if (prompt.Contains("drive", StringComparison.OrdinalIgnoreCase) ||
            prompt.Contains("usb", StringComparison.OrdinalIgnoreCase) ||
            prompt.Equals("letter", StringComparison.OrdinalIgnoreCase))
        {
            if (!IsDriveLetter(value))
                throw new ArgumentException("Informe apenas a letra da unidade, por exemplo C.", nameof(value));
            return;
        }
        if (prompt.Equals("nic", StringComparison.OrdinalIgnoreCase))
        {
            if (!IsSafeNetworkInterface(value))
                throw new ArgumentException("Selecione uma interface de rede com nome válido.", nameof(value));
            return;
        }
        if (!IsSafeExeInput(value))
            throw new ArgumentException("Use de 1 a 128 caracteres: letras, números, espaços, ponto, hífen, sublinhado ou parênteses. Não informe comandos nem caminhos.", nameof(value));
    }
}

public static class UltimateInputBuilder
{
    public static string Build(CatalogItem item, string? selectedKey, IReadOnlyList<string>? freeInputs)
    {
        var lines = new List<string>();
        if (selectedKey is not null)
        {
            if (!item.Options.Any(option => option.Key.Equals(selectedKey, StringComparison.Ordinal)))
                throw new ArgumentException("The selected key is not an option for this item.", nameof(selectedKey));
            lines.Add(selectedKey);
        }

        var answers = freeInputs ?? [];
        if (answers.Count != item.FreeInputs.Count)
            throw new ArgumentException("The number of free-input answers does not match the catalog.", nameof(freeInputs));
        for (var index = 0; index < answers.Count; index++)
        {
            InputValidator.ValidateFreeInput(item.FreeInputs[index], answers[index]);
            lines.Add(answers[index]);
        }

        if (item.Looping)
        {
            if (string.IsNullOrEmpty(item.ExitKey))
                throw new InvalidDataException($"Looping item {item.Id} has no exit key.");
            lines.Add(item.ExitKey);
        }
        return lines.Count == 0 ? "" : string.Join('\n', lines) + "\n";
    }
}

public static class RyTuneXCommandBuilder
{
    public static string Substitute(string command, string? nic, string systemDirectory)
    {
        var driveRoot = Path.GetPathRoot(systemDirectory);
        if (string.IsNullOrEmpty(driveRoot) && systemDirectory.Length >= 3 &&
            char.IsAsciiLetter(systemDirectory[0]) && systemDirectory[1] == ':' &&
            systemDirectory[2] is '\\' or '/')
            driveRoot = systemDirectory[..3];
        var result = command.Replace("{DriveRoot}", driveRoot ?? "", StringComparison.Ordinal);
        if (result.Contains("{nic}", StringComparison.Ordinal))
        {
            if (!InputValidator.IsSafeNetworkInterface(nic))
                throw new ArgumentException("A valid network interface must be selected.", nameof(nic));
            result = result.Replace("{nic}", nic, StringComparison.Ordinal);
        }
        return result;
    }

    public static IReadOnlyList<string> Build(CatalogItem item, string? nic, string systemDirectory) =>
        item.Commands.Select(command => Substitute(command, nic, systemDirectory)).ToList();
}
