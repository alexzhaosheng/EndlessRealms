using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility.Extensions;
public static class JsonExtensions
{
    public static T? JsonToObject<T>(this string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch(Exception ex) 
        {
            Console.WriteLine($"DeserializeObject failed. {ex.Message}");
            Console.WriteLine("JSON:");
            Console.WriteLine(json);
            throw;
        }
    }
    public static string ToJsonString(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
    public static T JsonClone<T>(this T obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj))!;
    }
}
