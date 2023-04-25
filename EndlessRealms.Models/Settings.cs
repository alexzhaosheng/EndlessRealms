using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class Settings
{
    public string ChatGptApiKey { get; set; } = null!;

    public void CopyTo(Settings other)
    {
        other.ChatGptApiKey = ChatGptApiKey;
    }
}
