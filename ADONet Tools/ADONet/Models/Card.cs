using ADONet_Tools.ADONet.Models;
using System.Text;

namespace ADONet_Tools.ADONet.Models
{
    public class Card
    {
        public int CardId { get; set; }
        public string Name { get; set; } = null!;
        public string? ManaCost { get; set; }
        public string? OracleText { get; set; }
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public string? Rarity { get; set; }
        public bool IsLegendary { get; set; }
        public List<Color> Colors { get; set; } = new();
        public List<string> Types { get; set; } = new();
        public List<Printing> Printings { get; set; } = new();
        public override string ToString()
        {
            return $"{Name} [{ManaCost ?? "—"}] ({Rarity ?? "Unknown"})"
                 + (IsLegendary ? " *" : "");
        }
        public string ToFullDetailString()
        {
            var sb = new StringBuilder();

            sb.Append(Name);

            if (IsLegendary)
                sb.Append(" (Legendary)");

            sb.AppendLine();

            sb.AppendLine($"Mana Cost: {ManaCost ?? "—"}");
            sb.AppendLine($"Rarity: {Rarity ?? "Unknown"}");

            if (Types?.Count > 0)
                sb.AppendLine($"Type: {string.Join(" ", Types)}");

            if (Power != null || Toughness != null)
                sb.AppendLine($"P/T: {Power ?? "?"}/{Toughness ?? "?"}");

            if (!string.IsNullOrWhiteSpace(OracleText))
            {
                sb.AppendLine();
                sb.AppendLine("Text:");
                sb.AppendLine(OracleText);
            }

            if (Printings?.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Printings:");
                foreach (var p in Printings)
                {
                    sb.AppendLine($"- {p.Set.Code} #{p.CollectorNumber}");
                }
            }

            return sb.ToString();
        }
    }
}