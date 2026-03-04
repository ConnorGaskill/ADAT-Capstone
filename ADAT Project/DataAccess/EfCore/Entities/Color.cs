using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Table("colors")]
[Index(nameof(Name), IsUnique = true)]
public partial class Color
{
    [Key]
    [Column("color_id")]
    public int ColorId { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [ForeignKey("ColorId")]
    [InverseProperty("Colors")]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
}
