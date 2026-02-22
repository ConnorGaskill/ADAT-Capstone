using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Table("cards")]
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

    [InverseProperty("Card")]
    public virtual ICollection<CardPrinting> CardPrintings { get; set; } = new List<CardPrinting>();

    [ForeignKey("CardId")]
    [InverseProperty("Cards")]
    public virtual ICollection<Cardtype> Cardtypes { get; set; } = new List<Cardtype>();

    [ForeignKey("CardId")]
    [InverseProperty("Cards")]
    public virtual ICollection<Color> Colors { get; set; } = new List<Color>();
}
