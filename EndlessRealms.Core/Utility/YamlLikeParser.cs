using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility;

//ChatGPT is not smart enought to give out a stable YAML result. The indent is incorrect when it's a long YAML
//Parse it by my self -_-||, igonre indent.
//Using the approach of checking whether the parent object has a property with the same name to infer that the current object has finished reading.
public class YamlLikeParser
{
    private string[] _lines = null!;
    private int _currentLine = 0;
    
    public YamlLikeParser()
    {

    }

    public T Parse<T>(string yaml)
    {
        return (T) Parse(typeof(T), yaml);
    }
    public object Parse(Type objectType, string yaml)
    {
        _lines = yaml.Split('\r', '\n').Where(t =>
        t != "---" 
        && t != "..."
        && !string.IsNullOrWhiteSpace(t)).ToArray();
        

        return ParseInternal(objectType, (s) => false);
    }

    public object ParseInternal(Type type, Func<string, bool> symbolCheck)
    {
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
        {
            return ParseArray(type, symbolCheck);
        }
        else
        {            
            return ParseObject(type, symbolCheck);
        }
    }

    private object ParseObject(Type type, Func<string, bool> symbolCheck)
    {
        var obj  = Activator.CreateInstance(type)!;
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var usedProperties = new HashSet<string>();
        var isFirst = true;
        while(_currentLine < _lines.Length)
        {
            if (string.IsNullOrWhiteSpace(_lines[_currentLine]))
            {
                _currentLine++;
                continue;
            }

            var line = _lines[_currentLine].Trim();
            if (line.StartsWith("-"))
            {
                if (!isFirst)
                    return obj;

                line = line.Substring(1);
            }
            
            if (line.EndsWith(':'))
            {
                var pName = line.Substring(0, line.Length - 1).Trim();
                var property = properties.FirstOrDefault(properties => properties.Name == pName);
                if(property != null && !property.PropertyType.IsValueType && property.PropertyType != typeof(String))
                {
                    _currentLine++;
                    usedProperties.Add(property.Name);
                    property.SetValue(obj, ParseInternal(property.PropertyType, (s)=> properties.Any(p=>p.Name == s)));
                }
                else
                {
                    if (symbolCheck(pName))
                    {
                        return obj;         //this property belongs to the parent, stop reading the current object. otherwise skip
                    }
                    _currentLine++; //ignore this line
                }
            }
            else
            {
                var pos = line.IndexOf(':');
                if(pos > 0)
                {
                    var pName = line.Substring(0, pos).Trim();
                    var value = line.Substring(pos +1).Trim();
                    var property = properties.FirstOrDefault(properties => properties.Name == pName);
                    if (property != null && !usedProperties.Contains(pName))
                    {
                        if ((property.PropertyType.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
                            && (property.PropertyType.GetElementType() == typeof(string) || property.PropertyType.GetElementType()!.IsValueType))
                        {
                            property.SetValue(obj, GetSimpleArray(property.PropertyType, value));
                        }
                        else
                        {
                            usedProperties.Add(property.Name);
                            property.SetValue(obj, Convert.ChangeType(RemoveQuote(value), property.PropertyType));
                        }
                    }
                    else
                    {
                        if (symbolCheck(pName))
                        {
                            return obj;         //this property belongs to the parent, stop reading the current object. otherwise skip
                        }
                    }                    
                }
                _currentLine++;
            }
            
            isFirst = false;
        }

        return obj;
    }

    private string RemoveQuote(string value)
    {
        value = value.Trim();
        if(value.StartsWith("\"") && value.EndsWith("\""))
        {
            return value.Substring(1, value.Length - 2);
        }
        return value;
    }

    private object? GetSimpleArray(Type type, string value)
    {        
        Type eleType;
        if (type.IsArray)
        {
            eleType = type.GetElementType()!;
        }
        else
        {
            eleType = type.GetGenericArguments()[0]; // it's List<>
        }
        var res = value.Split(',').Select(t => Convert.ChangeType(RemoveQuote(t.Trim()), eleType)).ToArray();
        if (type.IsArray)
        {
            var array = Array.CreateInstance(eleType, res.Length);
            Array.Copy(res.ToArray(), array, res.Length);
            return array;
        }
        else
        {
            var resList = Activator.CreateInstance(type)!;
            var addMethod = type.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public)!;
            foreach (var o in res)
            {
                addMethod.Invoke(resList, new object[] { o });
            }
            return resList;
        }
    }

    private object ParseArray(Type type, Func<string, bool> symbolCheck)
    {
        List<object> res = new List<object>();
        Type eleType;
        if (type.IsArray)
        {
            eleType = type.GetElementType()!;
        }
        else
        {
            eleType = type.GetGenericArguments()[0]; // it's List<>
        }

        while (_currentLine < _lines.Length)
        {
            if (string.IsNullOrWhiteSpace(_lines[_currentLine]))
            {
                _currentLine++;
                continue;

            }
            var line = _lines[_currentLine].Trim();
            if (!line.StartsWith("-"))
            {
                break;
            }
            if (eleType.IsValueType || eleType == typeof(string))
            {
                res.Add(Convert.ChangeType(RemoveQuote(line.Substring(1)), eleType));
                _currentLine++;
            }
            else
            {
                res.Add(ParseInternal(eleType, symbolCheck));
            }
        }

        if (type.IsArray)
        {
            var array = Array.CreateInstance(eleType, res.Count);
            Array.Copy(res.ToArray(), array, res.Count);
            return array;
        }
        else
        {            
            var resList =  Activator.CreateInstance(type)!;
            var addMethod = type.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public)!;
            foreach(var o in res)
            {
                addMethod.Invoke(resList, new object[] { o });
            }
            return resList;
        }
        
    }

}
