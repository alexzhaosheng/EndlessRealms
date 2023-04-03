using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility.Extensions;
public static class YamlExtensions
{
    public static T? YamlToObject<T>(this string data)
    {
        return (T?) data.YamlToObject(typeof(T));
    }

    public static object? YamlToObject(this string data, Type objType)
    {
        if(objType == typeof(string))
        {
            return data;
        }
        return new YamlLikeParser().Parse(objType, data);        
    }
}
