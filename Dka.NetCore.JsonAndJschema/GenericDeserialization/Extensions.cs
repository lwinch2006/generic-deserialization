using System;
using System.Collections.Generic;
using System.Linq;

namespace Dka.NetCore.JsonAndJschema.GenericDeserialization
{
    public static class Extensions
    {
        public static GenericDeserializedClass FlatProperties(this GenericDeserializedClass originalClass, string prefix = "")
        {
            if (originalClass?.AllProperties == null)
            {
                return null;
            }

            prefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : $"{prefix}.";
            
            var flattenProperties = new Dictionary<string, object>();
            
            foreach (var property in originalClass.AllProperties)
            {
                switch (property.Value)
                {
                    case IEnumerable<GenericDeserializedClass> propertyValues:
                        for (var i = 0; i < propertyValues.Count(); i++)
                        {
                            var propertyValue = propertyValues.ElementAt(i);
                            flattenProperties = flattenProperties.Concat(propertyValue.FlatProperties($"{prefix}{property.Key}.{i}").AllProperties).ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);
                        }
                        
                        break;
                        
                    case GenericDeserializedClass propertyValue:
                        flattenProperties = flattenProperties.Concat(propertyValue.FlatProperties($"{prefix}{property.Key}").AllProperties).ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);
                        break;

                    default:
                        flattenProperties.Add($"{prefix}{property.Key}", property.Value);
                        break;
                }                
            }

            originalClass.AllProperties = flattenProperties;

            return originalClass;
        }
        
        public static GenericDeserializedClass UnFlatProperties(this GenericDeserializedClass originalClass)
        {
            if (originalClass?.AllProperties == null)
            {
                return null;
            }

            var unFlattenProperties = new Dictionary<string, object>();

            while (originalClass.AllProperties.Any())
            {
                var property = originalClass.AllProperties.First();
                var subKey = GetSubKey(property.Key);

                if (subKey == null)
                {
                    unFlattenProperties.Add(property.Key, property.Value);
                    originalClass.AllProperties.Remove(property.Key);
                    continue;
                }
                
                var subProperties = new Dictionary<string, object>();
                foreach (var keyWithSubkey in originalClass.AllProperties.Keys.Where(key => key.StartsWith($"{subKey}.")))
                {
                    subProperties.Add(keyWithSubkey.Replace($"{subKey}.", string.Empty), originalClass.AllProperties[keyWithSubkey]);
                    originalClass.AllProperties.Remove(keyWithSubkey);
                }

                object valueToInsert;

                if (int.TryParse(GetSubKey(subProperties.Keys.First()), out _))
                {
                    var newItems = new List<GenericDeserializedClass>();
                    
                    while (subProperties.Any())
                    {
                        var subProperty = subProperties.First();
                        var subSubKey = GetSubKey(subProperty.Key);

                        var subSubProperties = new Dictionary<string, object>();
                        foreach (var subKeyWithSubSubkey in subProperties.Keys.Where(key => key.StartsWith($"{subSubKey}.")))
                        {
                            subSubProperties.Add(subKeyWithSubSubkey.Replace($"{subSubKey}.", string.Empty), subProperties[subKeyWithSubSubkey]);
                            subProperties.Remove(subKeyWithSubSubkey);
                        }

                        var subValueToInsert = new GenericDeserializedClass
                        {
                            AllProperties = subSubProperties
                        };

                        subValueToInsert = subValueToInsert.UnFlatProperties();
                        newItems.Add(subValueToInsert);
                    }

                    valueToInsert = InitEnumerable(newItems);
                }
                else
                {
                    var subValueToInsert = new GenericDeserializedClass
                    {
                        AllProperties = subProperties
                    };

                    valueToInsert = subValueToInsert.UnFlatProperties(); 
                }
                
                unFlattenProperties.Add(subKey, valueToInsert);
            }

            originalClass.AllProperties = unFlattenProperties;

            return originalClass;
        }
        
        public static IEnumerable<GenericDeserializedClass> FlatProperties(this IEnumerable<GenericDeserializedClass> originalClasses)
        {
            if (originalClasses == null)
            {
                yield return null;
            }

            foreach (var originalClass in originalClasses)
            {
                yield return originalClass.FlatProperties();
            }
        }
        
        public static IEnumerable<GenericDeserializedClass> UnFlatProperties(this IEnumerable<GenericDeserializedClass> originalClasses)
        {
            if (originalClasses == null)
            {
                yield return null;
            }

            foreach (var originalClass in originalClasses)
            {
                yield return originalClass.UnFlatProperties();
            }
        }

        private static IEnumerable<GenericDeserializedClass> InitEnumerable(IEnumerable<GenericDeserializedClass> items)
        {
            if (items == null)
            {
                yield break;
            }
            
            foreach (var item in items)
            {
                yield return item;
            }
        }

        private static string GetSubKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return key;
            }
            
            var indexOfDot = key.IndexOf(".", StringComparison.OrdinalIgnoreCase);
            var subkey = indexOfDot == -1 ? null : key.Substring(0, indexOfDot);

            return subkey;
        }
    }
}