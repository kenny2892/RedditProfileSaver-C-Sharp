using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public static class ExtensionMethods
    {
        public static string GetDescription(this Enum value) // Source: https://stackoverflow.com/a/1415187
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);

            if(name != null)
            {
                FieldInfo field = type.GetField(name);
                if(field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if(attr != null)
                    {
                        return attr.Description;
                    }
                }
            }

            return value.ToString();
        }
    }
}
