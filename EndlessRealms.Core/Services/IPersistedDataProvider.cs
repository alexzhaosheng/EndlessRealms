using EndlessRealms.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace EndlessRealms.Core.Services;

public interface IPersistedDataProvider
{
	IEnumerable<World> LoadWorlds();	
	ActionPrompt LoadActionPrompt();

    IEnumerable<ApiCallSetting> LoadApiCallSettings();
    Task SaveWorld(World world);
    Task ClearALlGameData();

    ChatHistory GetChatHistory(string characterId);
    Task SaveChatHistory(ChatHistory history);
}