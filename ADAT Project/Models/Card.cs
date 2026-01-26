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
    public override string ToString()
    {
        return $"{Name} [{ManaCost ?? "—"}] ({Rarity ?? "Unknown"})"
             + (IsLegendary ? " *" : "");
    }

}
