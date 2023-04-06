using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class ActionHistory
{
    public class ActionItem
    {
        public string? Action { get; set; }
        public string Response { get; set; } = string.Empty!;
        public string OriginalResponse { get; set; } = string.Empty!;
    }
    public string CharacterId { get;}
    public List<ActionItem> History = new List<ActionItem>();

    public void Add(string question, string answer, string originalAnswer)
    {
        History.Add(new ActionItem() { Action = question, Response = answer, OriginalResponse = originalAnswer });
    }

    public ActionHistory(string characterId)
    {
        CharacterId = characterId;
    }
}
