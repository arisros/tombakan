using System.Collections.Generic;
using System.Text;

/// <summary>
/// Aggregates a flat list of caught fish colours (hex strings) into a compact,
/// overflow-safe, localised summary like "Merah ×3, Biru ×1" (Week 2 UX-07).
/// First-seen order is preserved so the readout feels chronological.
/// Pure + static so it is unit-testable.
/// </summary>
public static class ColorSummary
{
    public const string EmptyText = "Tidak ada ikan dikumpulkan";

    public static string Format(IList<string> hexColors)
    {
        if (hexColors == null || hexColors.Count == 0)
            return EmptyText;

        // Preserve first-seen order while counting occurrences.
        var order = new List<string>();
        var counts = new Dictionary<string, int>();

        foreach (var hex in hexColors)
        {
            string name = ColorHexLocalization.ToIndonesian(hex);
            if (!counts.ContainsKey(name))
            {
                counts[name] = 0;
                order.Add(name);
            }
            counts[name]++;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < order.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");

            string name = order[i];
            int cnt = counts[name];
            sb.Append(name);
            if (cnt > 1) sb.Append(" ×").Append(cnt);
        }

        return sb.ToString();
    }
}
