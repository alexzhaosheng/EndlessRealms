using EndlessRealms.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace EndlessRealms.Core.Services;

public interface IPersistedDataProvider
{
    Task<Settings?> LoadSettings();
    Task SaveSettings(Settings settings);

	IEnumerable<World> LoadWorlds();	
	ActionPrompt LoadActionPrompt();

    IEnumerable<ApiCallSetting> LoadApiCallSettings();
    Task SaveWorld(World world);
    Task ClearAllGameData();

    Task<ActionHistory> GetActionHistory(string characterId);
    Task SaveActionHistory(ActionHistory history);

    Task<PlayerInfo?> LoadPlayerInfo();
    Task SavePlayerInfo(PlayerInfo playerInfo);
}