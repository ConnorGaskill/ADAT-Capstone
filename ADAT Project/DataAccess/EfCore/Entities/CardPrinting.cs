using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Table("card_printings")]
public partial class CardPrinting
{
    [Key]
    [Column("printing_id")]
    public int PrintingId { get; set; }

    [Column("card_id")]
    public int CardId { get; set; }

    [Column("set_id")]
    public int SetId { get; set; }

    [Column("collector_number")]
    [StringLength(20)]
    public string? CollectorNumber { get; set; }

    [ForeignKey("CardId")]
    [InverseProperty("CardPrintings")]
    public virtual Card Card { get; set; } = null!;

    [ForeignKey("SetId")]
    [InverseProperty("CardPrintings")]
    public virtual Set Set { get; set; } = null!;
}
