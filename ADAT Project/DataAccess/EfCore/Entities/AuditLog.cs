using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAT_Project.DataAccess.EfCore.Entities;

[Table("audit_log")]
public partial class AuditLog
{
    [Key]
    [Column("audit_id")]
    public int AuditId { get; set; }

    [Column("entity_id")]
    public int EntityId { get; set; }

    [Column("entity_type")]
    [StringLength(50)]
    public string EntityType { get; set; } = null!;

    [Column("action")]
    [StringLength(50)]
    public string Action { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
