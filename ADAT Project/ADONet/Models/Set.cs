using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAT_Project.ADONet.Models
{
    public class Set
    {
        public int SetId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
