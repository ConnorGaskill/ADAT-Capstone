using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADONet_Tools.ADONet.Models
{
    public class Printing
    {
        public int PrintingId { get; set; }
        public string CollectorNumber { get; set; } = null!;
        public Set Set { get; set; } = null!;
    }
}
