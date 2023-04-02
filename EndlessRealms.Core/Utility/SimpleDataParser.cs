using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility;
public class SimpleDataParser
{
    class SectionInfo
    {             
        public Action<string>? SetTextCallback { get; set; }

        public Type? ItemDataType { get; set; }
        public string[]? PropertiesInSection { get; set; }
        public Action<IEnumerable<object>>? SetDataCallback { get; set; }
    }

    private Dictionary<string, SectionInfo> _sections = new Dictionary<string, SectionInfo>();

    public SimpleDataParser() { }

    public SimpleDataParser PureTextSection(string sectionFlag, Action<string> setTextCallback)
    {
        _sections.Add(sectionFlag, new SectionInfo { SetTextCallback = setTextCallback });
        return this;
    }

    public SimpleDataParser DataSection<TDataType>(string sectionFlag, string[] dataProperties, Action<IEnumerable<TDataType>> callback)
    {
        _sections[sectionFlag] = new SectionInfo
        {
            ItemDataType = typeof(TDataType),
            PropertiesInSection = dataProperties,
            SetDataCallback = (data) => callback(data.Select(d => (TDataType)d))
        };
        return this;
    }

    public void Parse(string data)
    {
        SectionInfo? currentSection = null;

        var lines = data.Split('\r', '\n');
        
        var things = new List<object>();
        var description = "";
        
        foreach (var line in lines)
        {

            if (string.IsNullOrWhiteSpace(line) && currentSection?.SetDataCallback == null)
            {
                continue;
            }
            if (line.StartsWith("---"))
            {
                if(currentSection?.SetTextCallback != null)
                {
                    currentSection.SetTextCallback(description);
                    description = "";
                }
                else if (currentSection?.SetDataCallback != null)
                {
                    currentSection.SetDataCallback(things);
                    things.Clear();
                }
                var sectionName = line.Substring(3).Trim();
                if (_sections.ContainsKey(sectionName))
                {
                    currentSection = _sections[sectionName];
                }
                else
                {
                    currentSection = null;
                }
            }                        
            else
            {
                if (currentSection?.SetTextCallback != null)
                {
                    description += line + Environment.NewLine;
                }
                else if (currentSection?.SetDataCallback != null)
                {
                    var parts = line.Split('|');
                    if (parts.Length == currentSection!.PropertiesInSection!.Length)
                    {
                        var dataObj = Activator.CreateInstance(currentSection.ItemDataType!);
                        foreach(var p in currentSection!.PropertiesInSection!.Select((p, i)=>(p, parts[i])))
                        {
                            var propertyInfo = currentSection!.ItemDataType!.GetProperty(p.Item1, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                            if(propertyInfo == null)
                            {
                                throw new EndlessRealmsException($"Can't find property {p.Item1} on type {currentSection!.ItemDataType}");
                            }
                            propertyInfo.SetValue(dataObj, Convert.ChangeType(p.Item2, propertyInfo.PropertyType));
                        }
                        things.Add(dataObj!);
                    }                    
                }
            }
        }

        if(currentSection?.SetTextCallback != null)
                {
            currentSection.SetTextCallback(description);            
        }
        else if (currentSection?.SetDataCallback != null)
        {
            currentSection.SetDataCallback(things);            
        }
    }
    
}
