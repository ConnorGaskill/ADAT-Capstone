using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Table("cardtypes")]
[Index("Name", Name = "UQ_types_name", IsUnique = true)]
public partial class Cardtype
{
    [Key]
    [Column("cardtype_id")]
    public int CardtypeId { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [ForeignKey("CardtypeId")]
    [InverseProperty("Cardtypes")]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
}
