using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class ApiCallSetting
{
    public class ApiCallObject
    {
        public string model { get; set; } = null!;
        public string prompt { get; set; } = null!;
        public float temperature { get; set; }
        public int max_tokens { get; set; }
        public int top_p { get; set; }
        public float frequency_penalty { get; set; }
        public float presence_penalty { get; set; }
    }

    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;

    public ApiCallObject DataObject { get; set; } = null!;
}
