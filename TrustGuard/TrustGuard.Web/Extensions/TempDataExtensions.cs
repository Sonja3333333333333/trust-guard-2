using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace TrustGuard.Web.Extensions
{
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonSerializer.Serialize(value);
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            if (tempData.TryGetValue(key, out var o) && o is string jsonString)
            {
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            return null;
        }
    }
}