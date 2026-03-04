using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Code), IsUnique = true)]
[Table("sets")]
public partial class Set
{
    [Key]
    [Column("set_id")]
    public int SetId { get; set; }

    [Column("name")]
    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("code")]
    [StringLength(10)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    [Column("release_date")]
    public DateOnly? ReleaseDate { get; set; }

    [InverseProperty("Set")]
    public virtual ICollection<CardPrinting> CardPrintings { get; set; } = new List<CardPrinting>();
}
