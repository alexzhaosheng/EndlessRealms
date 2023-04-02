using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class Region
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Size { get; set; }
    public  int Complexity { get; set; }

    public Region()
    {
        Id = Guid.NewGuid().ToString();
    }

}
