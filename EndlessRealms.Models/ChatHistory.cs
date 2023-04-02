using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class ChatHistory
{
    public class ChatItem
    {
        public string? Question { get; set; }
        public string Answer { get; set; } = string.Empty!;
        public string OriginalAnswer { get; set; } = string.Empty!;
    }
    public string CharacterId { get;}
    public List<ChatItem> History = new List<ChatItem>();

    public void Add(string question, string answer, string originalAnswer)
    {
        History.Add(new ChatItem() { Question = question, Answer = answer, OriginalAnswer = originalAnswer });
    }

    public ChatHistory(string characterId)
    {
        CharacterId = characterId;
    }
}
