using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts
{
    public class EnumHelper // Source: https://stackoverflow.com/a/54167231
    {
        public static HtmlString EnumToString<T>()
        {
            List<T> enums = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            Dictionary<string, string> dict = new Dictionary<string, string>();

            int count = 0;
            foreach(T value in enums)
            {
                dict.Add(value.ToString(), "ContentTypes_" + count + "_");
                count++;
            }

            return new HtmlString(JsonConvert.SerializeObject(dict));
        }
    }
}
