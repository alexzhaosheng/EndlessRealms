using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility.Extensions;
public static class ActionPromptExtensions
{
    public static void SetPrompt(this ActionPrompt prompt, string pmtType, ActionPrompt.ActionPromptInfo info)
    {
        var prop = typeof(ActionPrompt).GetProperty(pmtType, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (prop == null || prop.PropertyType != typeof(ActionPrompt.ActionPromptInfo))
        {
            throw new EndlessRealmsException($"Unkown action prompt {pmtType}");
        }
        prop.SetValue(prompt, info, null);
    }

    public static void Parse(this ActionPrompt prompt, string content)
    {
        var lines = content.Split('\n', '\r');
        var lineNumber = 0;
        string? currentPmtType = "", apiSettingName="", currentContent = "";
        while (lineNumber < lines.Length)
        {
            var line = lines[lineNumber];
            if (line.StartsWith("!!!!"))
            {
                if (!string.IsNullOrEmpty(currentPmtType))
                {
                    prompt.SetPrompt(currentPmtType, 
                        new ActionPrompt.ActionPromptInfo() 
                        { 
                            ApiCallSettingName=apiSettingName, 
                            Content = currentContent
                        });
                }
                var section = line.Substring(4).Split(';');
                currentPmtType = section[0].Trim();
                apiSettingName = section.Length > 1 ? section[1].Trim() : "DEFAULT";
                currentContent = "";                
            }
            else
            {
                currentContent += "\n" + line;
            }

            lineNumber++;
        }

        if (!string.IsNullOrEmpty(currentPmtType))
        {
            prompt.SetPrompt(currentPmtType,
                         new ActionPrompt.ActionPromptInfo()
                         {
                             ApiCallSettingName = apiSettingName,
                             Content = currentContent
                         });
        }
    }

    public static void CheckAllSet(this ActionPrompt prompt)
    {
        var props = typeof(ActionPrompt).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach ( var prop in props.Where(p=>p.PropertyType == typeof(ActionPrompt.ActionPromptInfo)))
        {
            if (prop.GetValue(prompt, null) == null)
            {
                throw new EndlessRealmsException($"ActionPrompt.{prop.Name} is not set.");
            }
        }

    }
}
