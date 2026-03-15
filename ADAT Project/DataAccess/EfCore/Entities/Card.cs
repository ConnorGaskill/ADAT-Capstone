using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Table("cards")]
[Index(nameof(Name), IsUnique = true)]
public partial class Card
{
    [Key]
    [Column("card_id")]
    public int CardId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("mana_cost")]
    [StringLength(50)]
    public string? ManaCost { get; set; }

    [Column("oracle_text")]
    public string? OracleText { get; set; }

    [Column("power")]
    [StringLength(10)]
    public string? Power { get; set; }

    [Column("toughness")]
    [StringLength(10)]
    public string? Toughness { get; set; }

    [Column("rarity")]
    [StringLength(20)]
    public string? Rarity { get; set; }

    [Column("is_legendary")]
    public bool? IsLegendary { get; set; }

    //One to Many
    [InverseProperty("Card")]
    public virtual ICollection<CardPrinting> CardPrintings { get; set; } = new List<CardPrinting>();

    //Many to Many
    [InverseProperty("Cards")]
    public virtual ICollection<Cardtype> Cardtypes { get; set; } = new List<Cardtype>();

    //Many to Many
    [InverseProperty("Cards")]
    public virtual ICollection<Color> Colors { get; set; } = new List<Color>();

    public override string ToString()
    {
        return $"{Name} [{ManaCost ?? "—"}] ({Rarity ?? "Unknown"})"
             + (IsLegendary == true ? " *" : "");
    }

    public string ToFullDetailString()
    {
        var sb = new StringBuilder();

        sb.Append(Name);

        if (IsLegendary == true)
            sb.Append(" (Legendary)");

        sb.AppendLine();

        sb.AppendLine($"Mana Cost: {ManaCost ?? "—"}");
        sb.AppendLine($"Rarity: {Rarity ?? "Unknown"}");

        if (Cardtypes?.Any() == true)
        {
            var typeNames = Cardtypes.Select(t => t.Name);
            sb.AppendLine($"Type: {string.Join(" ", typeNames)}");
        }

        if (Power != null || Toughness != null)
            sb.AppendLine($"P/T: {Power ?? "?"}/{Toughness ?? "?"}");

        if (!string.IsNullOrWhiteSpace(OracleText))
        {
            sb.AppendLine();
            sb.AppendLine("Text:");
            sb.AppendLine(OracleText);
        }

        if (CardPrintings?.Any() == true)
        {
            sb.AppendLine();
            sb.AppendLine("Printings:");
            foreach (var printing in CardPrintings)
            {
                var setCode = printing.Set?.Code ?? "Unknown";
                sb.AppendLine($"- {setCode} #{printing.CollectorNumber ?? "?"}");
            }
        }

        if (Colors?.Any() == true)
        {
            var colorNames = Colors.Select(c => c.Name);
            sb.AppendLine();
            sb.AppendLine($"Colors: {string.Join(", ", colorNames)}");
        }

        return sb.ToString();
    }
}
