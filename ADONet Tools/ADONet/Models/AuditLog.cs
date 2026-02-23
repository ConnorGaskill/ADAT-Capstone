using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADONet_Tools.ADONet.Models
{
    public class AuditLog
    {
        public int AuditId { get; set; } 
        public int EntityId { get; set; } 
        public string EntityType { get; set; } 
        public string Action { get; set; } 
        public DateTime CreatedAt { get; set; } 
    }
}