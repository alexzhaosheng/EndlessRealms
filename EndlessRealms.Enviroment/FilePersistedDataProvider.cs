using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace EndlessRealms.LocalEnv;

[Service(typeof(IPersistedDataProvider))]
public class FilePersistedDataProvider: IPersistedDataProvider
{
	private string GetStatusFolder(string path)
	{
		var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Status", path);
		if (!Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}
		return folder;
	}
	private string GetPredefinedFolder(string? path = null)
	{
		var folder = path == null
			? Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Predefined")
			: Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Predefined", path);

		if (!Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}
		return folder;
	}

	public IEnumerable<World> LoadWorlds()
	{
		var files = Directory.GetFiles(GetStatusFolder("Worlds"), "*.erworld.json");
		foreach (var file in files)
		{
			JsonSerializer serializer = new JsonSerializer();
			using var reader = new JsonTextReader(new StreamReader(file));
			var world = serializer.Deserialize<World>(reader)!;
			if (world != null)
				yield return world;
		}
	}

    public Task SaveWorld(World world)
    {
		var file = Path.Combine(GetStatusFolder("Worlds"), $"{world.Id}.erworld.json");
		using var writer = new StreamWriter(file);
		writer.Write(world.ToJsonString());
        return Task.CompletedTask;
    }

    public ActionPrompt LoadActionPrompt()
    {
		var actionPrompt = new ActionPrompt();
        var files = Directory.GetFiles(GetPredefinedFolder("Actions"), "*.actionprompt.txt");
        foreach (var file in files)
        {           
            using var reader =new StreamReader(file);
			actionPrompt.Parse(reader.ReadToEnd());            
        }
		actionPrompt.CheckAllSet();

		return actionPrompt;
    }

    public IEnumerable<ApiCallSetting> LoadApiCallSettings()
    {
		using var streamReader = new StreamReader(Path.Combine(GetPredefinedFolder(), "ChaptgptCall.txt"));
		var lines = streamReader.ReadToEnd().Split('\n', '\r');

        var lineNumber = 0;
        string? currentName = "", currentUrl = "", currentContent = "";
        while (lineNumber < lines.Length)
        {
            var line = lines[lineNumber];
            if (line.StartsWith("!!!!"))
            {
                if (!string.IsNullOrEmpty(currentName))
                {								
					yield return new ApiCallSetting()
					{
						Name = currentName,
						Url = currentUrl,
						DataObject =  currentContent.JsonToObject<ApiCallSetting.ApiCallObject>()!
					};                    
                }

                var sections = line.Substring(4).Trim().Split(';');
                if (sections.Length != 2)
                {
                    throw new EndlessRealmsException($"Unkown setting at line '{lineNumber + 1}'");
                }
                currentName = sections[0];
                currentUrl = sections[1];
                currentContent = "";
            }
            else
            {
                currentContent += "\n" + line.Trim();
            }

            lineNumber++;
        }

        if (!string.IsNullOrEmpty(currentName))
        {
			yield return new ApiCallSetting()
			{
				Name = currentName,
				Url = currentUrl,
                DataObject = currentContent.JsonToObject<ApiCallSetting.ApiCallObject>()!
            };
        }
    }

    public Task ClearALlGameData()
    {
        var files = Directory.GetFiles(GetStatusFolder("Worlds"), "*.erworld.json");
		foreach (var file in files)
		{
			File.Delete(file);
		}
		return Task.CompletedTask;
    }

    public ChatHistory GetChatHistory(string characterId)
    {
        var file = Path.Combine(GetStatusFolder("ChatHistory"), $"{characterId}.chat.json");
        if(!File.Exists(file))
        {
            return new ChatHistory(characterId);
        }
        using var sr = new StreamReader(file);
        var json = sr.ReadToEnd();
        return json.JsonToObject<ChatHistory>()!;
    }

    public Task SaveChatHistory(ChatHistory history)
    {
        var file = Path.Combine(GetStatusFolder("ChatHistory"), $"{history.CharacterId}.chat.json");
        using var sw = new StreamWriter(file);
        sw.Write(history.ToJsonString());
        return Task.CompletedTask;
    }
}