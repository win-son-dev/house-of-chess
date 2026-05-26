using System.Globalization;
using System.Text;
using HouseOfChess.Platform.Infrastructure.Contracts.PgnExport;
using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Services.PgnExport;
using Microsoft.Extensions.Options;

namespace HouseOfChess.Platform.Services.PgnExport;

public sealed class PgnExportService(IOptions<PgnExportOptions> options) : IPgnExportService
{
    private readonly PgnExportOptions _options = options.Value;

    public string Build(PgnExportInputs inputs)
    {
        var sb = new StringBuilder();
        AppendTag(sb, "Event",       _options.Event);
        AppendTag(sb, "Site",        _options.Site);
        AppendTag(sb, "Date",        inputs.StartedAtUtc.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture));
        AppendTag(sb, "Round",       _options.Round);
        AppendTag(sb, "White",       inputs.WhiteUsername);
        AppendTag(sb, "Black",       inputs.BlackUsername);
        AppendTag(sb, "Result",      inputs.Result);
        AppendTag(sb, "TimeControl", ToPgnTimeControl(inputs.TimeControl));
        sb.Append('\n');

        for (var i = 0; i < inputs.SanMoves.Count; i++)
        {
            if (i % 2 == 0) sb.Append((i / 2) + 1).Append(". ");
            sb.Append(inputs.SanMoves[i]).Append(' ');
        }
        sb.Append(inputs.Result);

        return sb.ToString();
    }

    private static void AppendTag(StringBuilder sb, string name, string value) =>
        sb.Append('[').Append(name).Append(" \"").Append(value).Append("\"]").Append('\n');

    // Internal time-control format is "M+I" (minutes base + seconds increment).
    // PGN's TimeControl tag expects "S+I" (seconds base + seconds increment). Convert M*60.
    private static string ToPgnTimeControl(string internalSpec)
    {
        var plus = internalSpec.IndexOf('+');
        if (plus <= 0) return internalSpec;

        var minutesPart   = internalSpec[..plus];
        var incrementPart = internalSpec[(plus + 1)..];
        if (!int.TryParse(minutesPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
            return internalSpec;

        return $"{minutes * 60}+{incrementPart}";
    }
}
